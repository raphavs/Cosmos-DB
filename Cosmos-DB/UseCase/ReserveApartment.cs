﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Cosmos_DB.Help;
using Cosmos_DB.Object;
using Microsoft.Azure.Cosmos;

 namespace Cosmos_DB.UseCase
{
    public class ReserveApartment
    {
        private readonly Container customerContainer;
        private readonly Container apartmentContainer;
        private readonly Container reservationContainer;
        private readonly EncryptService encryptService;
        private readonly List<Customer> customers;
        private readonly List<Apartment> apartments;

        public ReserveApartment(
            Container customerContainer, 
            Container apartmentContainer, 
            Container reservationContainer, 
            EncryptService encryptService)
        {
            this.customerContainer = customerContainer;
            this.apartmentContainer = apartmentContainer;
            this.reservationContainer = reservationContainer;
            this.encryptService = encryptService;
            this.customers = new List<Customer>();
            this.apartments = new List<Apartment>();
        }
        
        public void Start()
        {
            Console.WriteLine();
            Console.WriteLine(">>>> RESERVE APARTMENT");
            Console.WriteLine();
            
            const string sqlQueryText = "SELECT * FROM c";

            // Collect all customers
            SetCustomers(sqlQueryText).GetAwaiter().GetResult();

            // Select customer
            var indexCustomer = -1;
            while (indexCustomer < 1 || indexCustomer > customers.Count)
            {
                Console.Write("Please select the customer by entering the corresponding number: ");
                try
                {
                    indexCustomer = Convert.ToInt32(Console.ReadLine());
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Input is not valid!");
                }
            }
            var selectedCustomer = customers.ElementAt(indexCustomer - 1);
            Console.WriteLine("You have chosen: " + selectedCustomer.Firstname + " " + selectedCustomer.Lastname);
            Console.WriteLine();
            
            // Collect all apartments
            SetApartments(sqlQueryText).GetAwaiter().GetResult();

            // Select apartment
            var indexApartment = -1;
            while (indexApartment < 1 || indexApartment > apartments.Count)
            {
                Console.Write("Please select the customer by entering the corresponding number: ");
                try
                {
                    indexApartment = Convert.ToInt32(Console.ReadLine());
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Input is not valid!");
                }
            }
            var selectedApartment = apartments.ElementAt(indexApartment - 1);
            Console.WriteLine("You have chosen apartment: " + indexApartment + " in " + selectedApartment.City + ", " + 
                              selectedApartment.Country);
            Console.WriteLine();
            
            // Making the reservation
            var apartmentIsAvailable = false;
            while (!apartmentIsAvailable)
            {
                // Enter period of time
                Console.WriteLine("Please enter your desired period of time.");
                var ofDate = new DateTime();
                var toDate = new DateTime();
                var periodOfTimeIsValid = false;
                while (!periodOfTimeIsValid)
                {
                    ofDate = SetPeriodOfTime("arrive");
                    toDate = SetPeriodOfTime("depart");
                    Console.WriteLine();

                    if (toDate > ofDate)
                    {
                        periodOfTimeIsValid = true;
                    }
                    else
                    {
                        Console.WriteLine("Period of Time is not valid!");
                    }
                }
                
                // Check if apartment is available
                var ofDateStr = ofDate.ToString("yyyy-MM-ddTHH:mm:ss");
                var toDateStr = toDate.ToString("yyyy-MM-ddTHH:mm:ss");
                var sqlQueryTextCustom = "SELECT * FROM c WHERE c.apartment_id = '2' AND c.of < '" + toDateStr + "' " +
                                         "AND c.to > '" + ofDateStr + "'";
                var queryDefinition = new QueryDefinition(sqlQueryTextCustom);
                var queryResultIterator = this.reservationContainer.GetItemQueryIterator<Reservation>(queryDefinition);

                if (ApartmentIsAvailable(queryResultIterator).GetAwaiter().GetResult())
                {
                    // Apartment available
                    apartmentIsAvailable = true;
                    
                    // Indicate if customer want to book or reserve the apartment
                    var type = "";
                    Console.Write(
                        "The apartment is available. Would you like to book it directly or just make a reservation?\n" +
                        "Press any key to book and r to reserve the apartment: ");
                    var input = Console.ReadLine();
                    Console.WriteLine();
                    type = input == "r" ? "reservation" : "booking";

                    var reservation = new Reservation
                    {
                        CustomerId = selectedCustomer.Id,
                        ApartmentId = selectedApartment.Id,
                        BookingDate = DateTime.Now,
                        Of = ofDate,
                        To = toDate,
                        Type = type
                    };
                    
                    // Add reservation to database
                    CreateReservation(reservation, selectedApartment, selectedCustomer).GetAwaiter().GetResult();
                
                    Console.WriteLine("You have successfully " + input == "r" ? "reserved" : "booked" + " the apartment!");
                    Console.WriteLine();
                }
                else
                {
                    // Apartment not available
                    Console.WriteLine("Apartment is not available!");
                    Console.WriteLine();
                    
                    // Specify another period of time or cancel process
                    Console.Write("Would you like to specify a different period of time or cancel the process?\n" +
                                  "Press any key to enter another time of period and x to cancel the process: ");
                    var input = Console.ReadLine();
                    Console.WriteLine();

                    if (input != "x") continue;
                    Console.WriteLine("Process aborted!");
                    Console.WriteLine();
                    break;
                }
            }
        }

        private async Task CreateReservation(
            Reservation reservation, 
            Apartment selectedApartment, 
            Customer selectedCustomer)
        {
            var sha256 = SHA256.Create();
            var valueToHash = string.Concat(selectedCustomer.Id, selectedApartment.Id, reservation.Of, reservation.To);
            var id = "";
            
            try
            {
                while (true)
                {
                    id = encryptService.GenerateHash(sha256, valueToHash);

                    // Check if the ID is already assigned
                    var reservationResponse = await this.reservationContainer.ReadItemAsync<Reservation>(id, new PartitionKey(reservation.Type));
                    Console.WriteLine("Reservation in database with id: {0} already exists\n", reservationResponse.Resource.Id);

                    valueToHash = id;
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                reservation.Id = id;
                
                // Create the reservation
                var reservationResponse = await this.reservationContainer.CreateItemAsync<Reservation>(reservation, new PartitionKey(reservation.Type));
                Console.WriteLine("Created reservation in database with id: {0}\n", reservationResponse.Resource.Id);
            }
        }

        private static DateTime SetPeriodOfTime(string text)
        {
            var date = new DateTime();
            var dateFormats = new[] {"dd-MM-yyyy"};
            var dateIsValid = false;
            while (!dateIsValid)
            {
                Console.Write("Please enter the day on which you would like to " + text + " (dd-MM-yyyy): ");
                var input = Console.ReadLine();
                dateIsValid = DateTime.TryParseExact(
                    input,
                    dateFormats,
                    DateTimeFormatInfo.InvariantInfo,
                    DateTimeStyles.None, 
                    out date);

                if (!dateIsValid)
                {
                    Console.WriteLine("DATE invalid!");
                }
            }

            if (text.Equals("arrive"))
            {
                date = date.AddHours(15);
            }
            
            if (text.Equals("depart"))
            {
                date = date.AddHours(12);
            }

            return date;
        }

        private async Task SetCustomers(string sqlQueryText)
        {
            var queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultIterator = this.customerContainer.GetItemQueryIterator<Customer>(queryDefinition);
            while (queryResultIterator.HasMoreResults)
            {
                Console.WriteLine();
                var index = 0;
                var currentResultSet = await queryResultIterator.ReadNextAsync();
                foreach (var customer in currentResultSet)
                {
                    index++;
                    Console.WriteLine(index + ". " + customer.Firstname + " " + customer.Lastname);
                    Console.WriteLine();

                    // Add customer to list
                    customers.Add(customer);
                }
            }
        }
        
        private async Task SetApartments(string sqlQueryText)
        {
            var queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultIterator = this.apartmentContainer.GetItemQueryIterator<Apartment>(queryDefinition);
            while (queryResultIterator.HasMoreResults)
            {
                Console.WriteLine();
                var index = 0;
                var currentResultSet = await queryResultIterator.ReadNextAsync();
                foreach (var apartment in currentResultSet)
                {
                    index++;
                    Console.WriteLine(index + ". Apartment:");
                    Console.WriteLine("Country: " + apartment.Country);
                    Console.WriteLine("City: " + apartment.City);
                    Console.WriteLine("Description: " + apartment.Description);
                    Console.WriteLine("Size: " + apartment.SquareMeters + " sqm");
                    Console.WriteLine("Price: " + apartment.Price + " $");
                    Console.WriteLine();
                    
                    // Add apartment to list
                    apartments.Add(apartment);
                }
            }
        }

        private async Task<bool> ApartmentIsAvailable(FeedIterator<Reservation> queryResultIterator)
        {
            if (!queryResultIterator.HasMoreResults) return true;
            var resultSet = await queryResultIterator.ReadNextAsync();
            return resultSet.Count <= 0;
        }
    }
}