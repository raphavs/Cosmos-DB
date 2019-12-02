﻿using System;
 using System.Collections.Generic;
 using System.Globalization;
 using System.Linq;
 using System.Net;
 using System.Security.Cryptography;
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
        }
        
        public async void Start()
        {
            Console.WriteLine();
            Console.WriteLine(">>> RESERVE APARTMENT");
            Console.WriteLine();
            
            const string sqlQueryText = "SELECT * FROM c";
            var queryDefinition = new QueryDefinition(sqlQueryText);
            
            // Select customer
            var customers = new List<Customer>();
            var queryCustomerResultIterator = this.customerContainer.GetItemQueryIterator<Customer>(queryDefinition);
            while (queryCustomerResultIterator.HasMoreResults)
            {
                Console.WriteLine();
                var index = 0;
                var currentCustomerResultSet = await queryCustomerResultIterator.ReadNextAsync();
                foreach (var customer in currentCustomerResultSet)
                {
                    index++;
                    Console.WriteLine(index + ". " + customer.fullname);
                    Console.WriteLine();
                    
                    // Add customer to list
                    customers.Add(customer);
                }
            }
            
            Console.WriteLine();
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
            Console.WriteLine("You have chosen: " + selectedCustomer.fullname);
            Console.WriteLine();
            
            // Select apartment
            var apartments = new List<Apartment>();
            var queryApartmentResultIterator = this.apartmentContainer.GetItemQueryIterator<Apartment>(queryDefinition);
            while (queryApartmentResultIterator.HasMoreResults)
            {
                Console.WriteLine();
                var index = 0;
                var currentApartmentResultSet = await queryApartmentResultIterator.ReadNextAsync();
                foreach (var apartment in currentApartmentResultSet)
                {
                    index++;
                    Console.WriteLine(index + ". apartment:");
                    Console.WriteLine("Description: " + apartment.description);
                    Console.WriteLine("Country: " + apartment.country);
                    Console.WriteLine("City: " + apartment.city);
                    Console.WriteLine("Price: " + apartment.price + "$");
                    Console.WriteLine("Size: " + apartment.qm + "qm");
                    Console.Write("Additional equipment: ");
                    for (var i = 0; i < currentApartmentResultSet.Count; i++)
                    {
                        Console.Write(apartment.additional_equipment[i]);

                        if (i < currentApartmentResultSet.Count - 1)
                        {
                            Console.Write(", ");
                        }
                    }
                    Console.WriteLine();
                    
                    // Add apartment to list
                    apartments.Add(apartment);
                }
            }
            
            Console.WriteLine();
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
                apartmentIsAvailable = true;
                var sqlQueryTextCustom = "SELECT * FROM c WHERE apartment_id = " + selectedApartment.id;
                queryDefinition = new QueryDefinition(sqlQueryTextCustom);
                var queryReservationResultIterator = this.reservationContainer.GetItemQueryIterator<Reservation>(queryDefinition);
                while (queryReservationResultIterator.HasMoreResults)
                {
                    var currentReservationResultSet = await queryReservationResultIterator.ReadNextAsync();
                    foreach (var reservation in currentReservationResultSet)
                    {
                        if (toDate > reservation.from && toDate <= reservation.to || 
                            fromDate < reservation.to && fromDate >= reservation.from)
                        {
                            apartmentIsAvailable = false;
                            Console.WriteLine("Apartment is not available!");
                            Console.WriteLine();
                            break;
                        }
                    }
                }

                if (!apartmentIsAvailable)
                {
                    Console.Write("Would you like to specify a different period of time or cancel the process?\n" +
                                  "Press any key to enter another time of period and x to cancel the process: ");
                    var input = Console.ReadLine();
                    Console.WriteLine();

                    if (input != "x") continue;
                    Console.WriteLine("Process aborted! You return to the menu...");
                    Console.WriteLine();
                    break;
                }
                else
                {
                    var type = "";
                    Console.Write("The apartment is available. Would you like to book it directly or just make a reservation?\n" +
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
   
                    var sha256 = SHA256.Create();
                    var valueToHash = string.Concat(selectedApartment.id, fromDate, toDate);
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
                    
                    Console.WriteLine("You have successfully booked the apartment. You return to the menu...");
                }
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
    }
}