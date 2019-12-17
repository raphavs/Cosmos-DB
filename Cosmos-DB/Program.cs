﻿using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Cosmos_DB
{
    internal class Program
    {
        // The Azure Cosmos DB endpoint for running this sample.
        private const string ENDPOINT_URI = "https://dominik.documents.azure.com:443/";
        
        // The primary key for the Azure Cosmos account.
        private const string PRIMARY_KEY = "2re6gMOGOcTjdDpRle0PpsI5sFGv1WNdiYdr0yHffSGA5voKqcMxyMoSWP9GsPGKczpWYqVlW6dqAvCxxhjBdQ==";

        // The Cosmos client instance
        private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The containers we will create
        private Container customerContainer;
        private Container apartmentContainer;
        private Container reservationContainer;

        // The name of the database and containers we will create
        private const string DATABASE_ID = "HolidayDatabase";
        private const string CUSTOMER_CONTAINER_ID = "Customer";
        private const string APARTMENT_CONTAINER_ID = "Apartment";
        private const string RESERVATION_CONTAINER_ID = "Reservation";
        
        // Use Cases
        private AddCustomer addCustomer;
        private SearchCustomer searchCustomer;
        
        public static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("Beginning operations...\n");
                var p = new Program();
                await p.GetStartedDemoAsync();

            }
            catch (CosmosException de)
            {
                var baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}", de.StatusCode, de);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
            }
            finally
            {
                Console.WriteLine("End of demo, press any key to exit.");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Entry point to call methods that operate on Azure Cosmos DB resources in this sample
        /// </summary>
        private async Task GetStartedDemoAsync()
        {
            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(ENDPOINT_URI, PRIMARY_KEY);
            await this.CreateDatabaseAsync();
            await this.CreateContainerAsync();
            
            addCustomer = new AddCustomer(customerContainer);
            searchCustomer = new SearchCustomer(customerContainer);
            
            Console.WriteLine();
            Console.WriteLine("Hello :)");
            Console.WriteLine("I am an intelligent system to manage apartments and clients of a holiday resort.");

            var input = "";
            do
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("xxxxxxxxxxxxxxx   MENU   xxxxxxxxxxxxxxx");
                Console.WriteLine();
                Console.WriteLine("Please press s to search a customer.");
                Console.WriteLine("Please press a to add a customer.");
                Console.WriteLine("Please press r to reserve a apartment.");
                Console.WriteLine("Please press d to delete a reservation.");
                Console.WriteLine();
                Console.WriteLine("xxxxxxxxxxxxxxx   MENU   xxxxxxxxxxxxxxx");
                Console.WriteLine();
                Console.WriteLine();
                Console.Write("User input: ");
                var action = Console.ReadLine();

                switch (action)
                {
                    case "s":
                        searchCustomer.Start();
                        break;
                    case "a":
                        addCustomer.Start();
                        break;
                    case "r":
                        // useCaseReserveApartment.Start();
                        break;
                    case "d":
                        // useCaseDeleteReservation.Start();
                        break;
                    default:
                        Console.WriteLine();
                        Console.WriteLine("Incorrect input!");
                        break;
                }
                
                Console.WriteLine();
                Console.Write("Do want to continue? (y|n) ");
                input = Console.ReadLine();
                if (input == null) break;

            } while (input.Equals("y"));
        }

        /// <summary>
        /// Create the database if it does not exist
        /// </summary>
        private async Task CreateDatabaseAsync()
        {
            // Create a new database
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(DATABASE_ID);
            Console.WriteLine("Created Database: {0}", this.database.Id);
        }

        /// <summary>
        /// Create the containers if they do not exist. 
        /// </summary>
        /// <returns></returns>
        private async Task CreateContainerAsync()
        {
            // Create container for customers
            this.customerContainer = await this.database.CreateContainerIfNotExistsAsync(CUSTOMER_CONTAINER_ID, "/country");
            Console.WriteLine("Created Container: {0}", this.customerContainer.Id);
            
            // Create container for apartments
            this.apartmentContainer = await this.database.CreateContainerIfNotExistsAsync(APARTMENT_CONTAINER_ID, "/country");
            Console.WriteLine("Created Container: {0}", this.apartmentContainer.Id);
            
            // Create container for reservations
            this.reservationContainer = await this.database.CreateContainerIfNotExistsAsync(RESERVATION_CONTAINER_ID, "/type");
            Console.WriteLine("Created Container: {0}", this.reservationContainer.Id);
        }
    }
}