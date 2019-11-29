using System;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using Cosmos_DB.Help;
using Cosmos_DB.Object;
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
        
        // The service for encrypting
        private EncryptService encryptService;

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
            
            encryptService = new EncryptService();
            
            addCustomer = new AddCustomer(customerContainer, encryptService);
            
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
                        // useCaseSearchCustomer.Start();
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
                Thread.Sleep(2000);
                Console.Write("Do want to continue? (y|n) ");
                input = Console.ReadLine();
                if (input == null) break;

            } while (input.Equals("y"));

            /*
            await this.QueryCustomerItemsAsync();
            await this.InsertCustomerItemAsync();
            await this.QueryCustomerItemsAsync();
            await this.DeleteCustomerItemAsync(); 
            await this.QueryCustomerItemsAsync();
            await this.ReplaceFamilyItemAsync();
            */
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
        /// Specify "/country" as the partition key to ensure good distribution of requests and storage.
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

        /// <summary>
        /// Run a query (using Azure Cosmos DB SQL syntax) against the container
        /// </summary>
        private async Task QueryCustomerItemsAsync()
        {
            const string sqlQueryText = "SELECT * FROM c";
            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            var queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = this.customerContainer.GetItemQueryIterator<Customer>(queryDefinition);

            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (var customer in currentResultSet)
                {
                    Console.WriteLine(customer.id);
                    Console.WriteLine(customer.firstname + " " + customer.lastname);
                    Console.WriteLine(customer.email);
                    Console.WriteLine();
                }
            }
        }

        /*
        ++++++ UPDATE ITEM: ++++++
        /// <summary>
        /// Replace an item in the container
        /// </summary>
        private async Task ReplaceFamilyItemAsync()
        {
            ItemResponse<Family> wakefieldFamilyResponse = await this.container.ReadItemAsync<Family>("Wakefield.7", new PartitionKey("Wakefield"));
            var itemBody = wakefieldFamilyResponse.Resource;

            // update registration status from false to true
            itemBody.IsRegistered = true;
            // update grade of child
            itemBody.Children[0].Grade = 6;

            // replace the item with the updated content
            wakefieldFamilyResponse = await this.container.ReplaceItemAsync<Family>(itemBody, itemBody.Id, new PartitionKey(itemBody.LastName));
            Console.WriteLine("Updated Family [{0},{1}].\n \tBody is now: {2}\n", itemBody.LastName, itemBody.Id, wakefieldFamilyResponse.Resource);
        }
        */

        /// <summary>
        /// Insert an item in the container
        /// </summary>
        private async Task InsertCustomerItemAsync()
        {
            var customer = new Customer
            {
                id = "78",
                firstname = "Rene",
                lastname = "Borner",
                date_of_birth = new DateTime(1998, 12, 30),
                email = "mail@reneborner.de",
                phone = "123456789",
                street = "Highway 42",
                city = "Constance",
                postcode = "78467",
                country = "Germany",
                bank_code = "1234",
                bank_account_number = "1234567"
            };

            try
            {
                // Read the item to see if it exists
                var customerResponse = await this.customerContainer.ReadItemAsync<Customer>(customer.id, new PartitionKey(customer.country));
                Console.WriteLine("Item in database with id: {0} already exists\n", customerResponse.Resource.id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Rene customer. Note we provide the value of the partition key for this item, which is "Germany"
                var customerResponse = await this.customerContainer.CreateItemAsync<Customer>(customer, new PartitionKey(customer.country));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", customerResponse.Resource.id, customerResponse.RequestCharge);
            }
        }
        
        /// <summary>
        /// Delete an item in the container
        /// </summary>
        private async Task DeleteCustomerItemAsync()
        {
            const string customerId = "1";
            const string partitionKeyValue = "Germany";

            // Delete an item. Note we must provide the partition key value and id of the item to delete
            await this.customerContainer.DeleteItemAsync<Customer>(customerId, new PartitionKey(partitionKeyValue));
            Console.WriteLine("Deleted Customer [{0},{1}]\n", customerId, partitionKeyValue);
        }
    }
}
