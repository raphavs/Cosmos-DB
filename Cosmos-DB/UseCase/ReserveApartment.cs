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
        private readonly List<Reservation> reservations;

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
            this.reservations = new List<Reservation>();
        }
        
        public void Start()
        {
            Console.WriteLine();
            Console.WriteLine(">>> RESERVE APARTMENT");
            Console.WriteLine();
            
            const string sqlQueryText = "SELECT * FROM c";
            var queryDefinition = new QueryDefinition(sqlQueryText);

            // Collect all customers
            SetCustomers(queryDefinition).GetAwaiter().GetResult();

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
            Console.WriteLine("You have chosen: " + selectedCustomer.firstname + " " + selectedCustomer.lastname);
            Console.WriteLine();
            
            // Collect all apartments
            SetApartments(queryDefinition).GetAwaiter().GetResult();

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
            Console.WriteLine("You have chosen apartment: " + indexApartment + " in " + selectedApartment.city + ", " + 
                              selectedApartment.country);
            Console.WriteLine();

            // Enter period of time
            // Check if apartment is available
            var apartmentIsAvailable = false;
            while (!apartmentIsAvailable)
            {
                Console.WriteLine("Please enter your desired period of time.");
                var fromDate = new DateTime();
                var toDate = new DateTime();
                var periodOfTimeIsValid = false;
                while (!periodOfTimeIsValid)
                {
                    fromDate = SetPeriodOfTime("arrive");
                    toDate = SetPeriodOfTime("depart");
                    Console.WriteLine();

                    if (toDate > fromDate)
                    {
                        periodOfTimeIsValid = true;
                    }
                    else
                    {
                        Console.WriteLine("Period of Time is not valid!");
                    }
                }
                
                // Check if apartment is available
                var sqlQueryTextCustom = "SELECT * FROM c WHERE c.apartment_id = '" + selectedApartment.id + "'";
                queryDefinition = new QueryDefinition(sqlQueryTextCustom);
                
                // Collect all reservations of corresponding apartment
                SetReservations(queryDefinition).GetAwaiter().GetResult();

                apartmentIsAvailable = true;
                foreach (var reservation in reservations)
                {
                    if (toDate > reservation.from && fromDate < reservation.to)
                    {
                        apartmentIsAvailable = false;
                        Console.WriteLine("Apartment is not available!");
                        Console.WriteLine();
                        break;
                    }
                }
                
                if (!apartmentIsAvailable)
                {
                    // Apartment not available
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
                else
                {
                    // Apartment available
                    // Indicate if you want to book or reserve the apartment
                    var type = "";
                    Console.Write(
                        "The apartment is available. Would you like to book it directly or just make a reservation?\n" +
                        "Press any key to book and r to reserve the apartment: ");
                    var input = Console.ReadLine();
                    Console.WriteLine();

                    type = input == "r" ? "reservation" : "booking";

                    var reservation = new Reservation
                    {
                        customer_id = selectedCustomer.id,
                        apartment_id = selectedApartment.id,
                        booking_date = DateTime.Now,
                        from = fromDate,
                        to = toDate,
                        type = type
                    };
                    
                    // Add reservation to database
                    CreateReservation(reservation, selectedApartment, selectedCustomer).GetAwaiter().GetResult();
                
                    Console.WriteLine("You have successfully " + input == "r" ? "reserved" : "booked" + " the apartment!");
                    Console.WriteLine();
                }
            }
        }

        private async Task CreateReservation(
            Reservation reservation, 
            Apartment selectedApartment, 
            Customer selectedCustomer)
        {
            var sha256 = SHA256.Create();
            var valueToHash = string.Concat(selectedCustomer.id, selectedApartment.id, reservation.from, reservation.to);
            var id = "";
            
            try
            {
                while (true)
                {
                    id = encryptService.GenerateHash(sha256, valueToHash);

                    // Check if the ID is already assigned
                    var reservationResponse = await this.reservationContainer.ReadItemAsync<Reservation>(id, new PartitionKey(reservation.type));
                    Console.WriteLine("Reservation in database with id: {0} already exists\n", reservationResponse.Resource.id);

                    valueToHash = id;
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                reservation.id = id;
                
                // Create the reservation
                var reservationResponse = await this.reservationContainer.CreateItemAsync<Reservation>(reservation, new PartitionKey(reservation.type));
                Console.WriteLine("Created reservation in database with id: {0}\n", reservationResponse.Resource.id);
            }
        }

        private static DateTime SetPeriodOfTime(string text)
        {
            var date = new DateTime();
            var dateFormats = new[] {"dd/MM/yyyy"};
            var dateIsValid = false;
            while (!dateIsValid)
            {
                Console.Write("Please enter the day on which you would like to " + text + " (dd/MM/yyyy): ");
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

            return date;
        }

        private async Task SetCustomers(QueryDefinition queryDefinition)
        {
            var queryResultIterator = this.customerContainer.GetItemQueryIterator<Customer>(queryDefinition);
            while (queryResultIterator.HasMoreResults)
            {
                Console.WriteLine();
                var index = 0;
                var currentResultSet = await queryResultIterator.ReadNextAsync();
                foreach (var customer in currentResultSet)
                {
                    index++;
                    Console.WriteLine(index + ". " + customer.firstname + " " + customer.lastname);
                    Console.WriteLine();

                    // Add customer to list
                    customers.Add(customer);
                }
            }
        }
        
        private async Task SetApartments(QueryDefinition queryDefinition)
        {
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
                    Console.WriteLine("Country: " + apartment.country);
                    Console.WriteLine("City: " + apartment.city);
                    Console.WriteLine("Description: " + apartment.description);
                    Console.WriteLine("Size: " + apartment.qm + " qm");
                    /*
                    Console.Write("Additional equipment: ");
                    Console.WriteLine(apartment.additional_equipment);
                    for (var i = 0; i < apartment.additional_equipment.Length; i++)
                    {
                        Console.Write(apartment.additional_equipment[i]);

                        if (i < apartment.additional_equipment.Length - 1)
                        {
                            Console.Write(", ");
                        }
                    }
                    */
                    Console.WriteLine("Price: " + apartment.price + " $");
                    Console.WriteLine();
                    
                    // Add apartment to list
                    apartments.Add(apartment);
                }
            }
        }
        
        private async Task SetReservations(QueryDefinition queryDefinition)
        {
            var queryResultIterator = this.reservationContainer.GetItemQueryIterator<Reservation>(queryDefinition);
            while (queryResultIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultIterator.ReadNextAsync();
                foreach (var reservation in currentResultSet)
                {
                    // Add reservation to list
                    reservations.Add(reservation);
                }
            }
        }
    }
}