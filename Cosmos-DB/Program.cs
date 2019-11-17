using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Cosmos_DB
{
    class Program
    {
        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = "https://rapha.documents.azure.com:443/";
        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = "VJPr6SiLxGNJwt5Qf981U2xMn98HbRZefqWRdwRSzcLRQw1SGZ12uHXzAmSIkre9hKdfTltKf6sZOYly2krguA==";

        // The Cosmos client instance
        private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        private Container container;

        // The name of the database and container we will create
        private string databaseId = "HolidayDatabase";
        private string containerId = "Test";


        public static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            try
            {
                Console.WriteLine("Beginning operations...\n");
                Program p = new Program();
                await p.GetStartedDemoAsync();

            }
            catch (CosmosException de)
            {
                Exception baseException = de.GetBaseException();
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
        public async Task GetStartedDemoAsync()
        {
            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            await this.CreateDatabaseAsync();
            await this.CreateContainerAsync();
            await this.QueryItemsAsync();
            
            // await this.DeletePersonItemAsync();
            
            await this.InsertPersonItemAsync();
            await this.QueryItemsAsync();
            
            await this.InsertPersonItemAsync_2();
            await this.QueryItemsAsync();

            /* 
            await this.AddItemsToContainerAsync();
            await this.AddSample();
            await this.ReplaceFamilyItemAsync();
            */
        }

        private async Task AddSample()
        {
            List<Person> persons = LoadJson();

            foreach (var person in persons)
            {
                Console.WriteLine(person.id);
                Console.WriteLine(person.name);
                Console.WriteLine(person.passion);
                
                
                try
                {
                    // Read the item to see if it exists
                    ItemResponse<Person> personResponse = await this.container.ReadItemAsync<Person>(person.id, new PartitionKey(person.name));
                    Console.WriteLine("Item in database with id: {0} already exists\n", personResponse.Resource.id);
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    // Create an item in the container representing the Wakefield family. Note we provide the value of the partition key for this item, which is "Wakefield"
                    ItemResponse<Person> personResponse = await this.container.CreateItemAsync<Person>(person, new PartitionKey(person.name));

                    // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                    Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", personResponse.Resource.id, personResponse.RequestCharge);
                }
            }
        }

        private async Task ReadSample()
        {
            
        }
        
        private List<Person> LoadJson()
        {
            var jsonText = File.ReadAllText("C:/Users/Rapha/source/repos/Cosmos-DB/Cosmos-DB/sample.json");
            return JsonConvert.DeserializeObject<List<Person>>(jsonText);
        }

        /// <summary>
        /// Create the database if it does not exist
        /// </summary>
        private async Task CreateDatabaseAsync()
        {
            // Create a new database
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Console.WriteLine("Created Database: {0}\n", this.database.Id);
        }

        /// <summary>
        /// Create the container if it does not exist. 
        /// Specifiy "/LastName" as the partition key since we're storing family information, to ensure good distribution of requests and storage.
        /// </summary>
        /// <returns></returns>
        private async Task CreateContainerAsync()
        {
            // Create a new container
            this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/name");
            Console.WriteLine("Created Container: {0}\n", this.container.Id);
        }

        /// <summary>
        /// Add Family items to the container
        /// </summary>
        private async Task AddItemsToContainerAsync()
        {
            // Create a family object for the Andersen family
            Family andersenFamily = new Family
            {
                Id = "Andersen.1",
                LastName = "Andersen",
                Parents = new Parent[]
                {
            new Parent { FirstName = "Thomas" },
            new Parent { FirstName = "Mary Kay" }
                },
                Children = new Child[]
                {
            new Child
            {
                FirstName = "Henriette Thaulow",
                Gender = "female",
                Grade = 5,
                Pets = new Pet[]
                {
                    new Pet { GivenName = "Fluffy" }
                }
            }
                },
                Address = new Address { State = "WA", County = "King", City = "Seattle" },
                IsRegistered = false
            };

            try
            {
                // Read the item to see if it exists.  
                ItemResponse<Family> andersenFamilyResponse = await this.container.ReadItemAsync<Family>(andersenFamily.Id, new PartitionKey(andersenFamily.LastName));
                Console.WriteLine("Item in database with id: {0} already exists\n", andersenFamilyResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"
                ItemResponse<Family> andersenFamilyResponse = await this.container.CreateItemAsync<Family>(andersenFamily, new PartitionKey(andersenFamily.LastName));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", andersenFamilyResponse.Resource.Id, andersenFamilyResponse.RequestCharge);
            }

            // Create a family object for the Wakefield family
            Family wakefieldFamily = new Family
            {
                Id = "Wakefield.7",
                LastName = "Wakefield",
                Parents = new Parent[]
                {
            new Parent { FamilyName = "Wakefield", FirstName = "Robin" },
            new Parent { FamilyName = "Miller", FirstName = "Ben" }
                },
                Children = new Child[]
                {
            new Child
            {
                FamilyName = "Merriam",
                FirstName = "Jesse",
                Gender = "female",
                Grade = 8,
                Pets = new Pet[]
                {
                    new Pet { GivenName = "Goofy" },
                    new Pet { GivenName = "Shadow" }
                }
            },
            new Child
            {
                FamilyName = "Miller",
                FirstName = "Lisa",
                Gender = "female",
                Grade = 1
            }
                },
                Address = new Address { State = "NY", County = "Manhattan", City = "NY" },
                IsRegistered = true
            };

            try
            {
                // Read the item to see if it exists
                ItemResponse<Family> wakefieldFamilyResponse = await this.container.ReadItemAsync<Family>(wakefieldFamily.Id, new PartitionKey(wakefieldFamily.LastName));
                Console.WriteLine("Item in database with id: {0} already exists\n", wakefieldFamilyResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Wakefield family. Note we provide the value of the partition key for this item, which is "Wakefield"
                ItemResponse<Family> wakefieldFamilyResponse = await this.container.CreateItemAsync<Family>(wakefieldFamily, new PartitionKey(wakefieldFamily.LastName));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", wakefieldFamilyResponse.Resource.Id, wakefieldFamilyResponse.RequestCharge);
            }
        }

        /// <summary>
        /// Run a query (using Azure Cosmos DB SQL syntax) against the container
        /// </summary>
        private async Task QueryItemsAsync()
        {
            var sqlQueryText = "SELECT * FROM c";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            
            Console.WriteLine("Query Definition: " + queryDefinition);
            Console.WriteLine("Query Definition Query Text: " + queryDefinition.QueryText);
            
            FeedIterator<Person> queryResultSetIterator = this.container.GetItemQueryIterator<Person>(queryDefinition);
            
            Console.WriteLine("Query Result Set Iterator: " + queryResultSetIterator);

            List<Person> persons = new List<Person>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Person> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Person person in currentResultSet)
                {
                    Console.WriteLine();
                    persons.Add(person);
                    Console.WriteLine("\tRead {0}\n", person);
                    Console.WriteLine("Person Nr. " + person.id);
                    Console.WriteLine("Name: " + person.name);
                    Console.WriteLine("Passion: " + person.passion);
                }
            }
        }

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

        /// <summary>
        /// Delete an item in the container
        /// </summary>
        private async Task DeletePersonItemAsync()
        {
            var personId = "2";
            var partitionKeyValue = "Dominik";

            // Delete an item. Note we must provide the partition key value and id of the item to delete
            ItemResponse<Person> personResponse = await this.container.DeleteItemAsync<Person>(personId, new PartitionKey(partitionKeyValue));
            Console.WriteLine("Deleted Person [{0},{1}]\n", personId, partitionKeyValue);
        }
        
        /// <summary>
        /// Insert an item in the container
        /// </summary>
        private async Task InsertPersonItemAsync()
        {
            var rene = new Person
            {
                id = "4",
                name = "Rene",
                passion = "Coden"
            };

            try
            {
                // Read the item to see if it exists
                ItemResponse<Person> personResponse = await this.container.ReadItemAsync<Person>(rene.id, new PartitionKey(rene.name));
                Console.WriteLine("Item in database with id: {0} already exists\n", personResponse.Resource.id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Wakefield family. Note we provide the value of the partition key for this item, which is "Wakefield"
                ItemResponse<Person> personResponse = await this.container.CreateItemAsync<Person>(rene, new PartitionKey(rene.name));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", personResponse.Resource.id, personResponse.RequestCharge);
            }
        }
        
        /// <summary>
        /// Insert an item in the container
        /// </summary>
        private async Task InsertPersonItemAsync_2()
        {
            var nico = new Person
            {
                id = "5",
                name = "Nico",
                passion = "Coden"
            };
            
            Console.WriteLine("Works");
            ItemResponse<Person> personResponse = await this.container.CreateItemAsync<Person>(nico, new PartitionKey(nico.name));
        }
    }
}
