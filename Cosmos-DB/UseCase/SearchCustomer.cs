using System;
using System.Threading.Tasks;
using Cosmos_DB.Object;
using Microsoft.Azure.Cosmos;

namespace Cosmos_DB.UseCase
{
    public class SearchCustomer
    {
        private readonly Container customerContainer;

        public SearchCustomer(Container container)
        {
            this.customerContainer = container;
        }

        public void Start()
        {
            Console.WriteLine();
            Console.WriteLine(">>>>> SEARCH CUSTOMER");
            Console.WriteLine();
            
            Console.Write("Please enter the string you are searching for: ");
            var input = Console.ReadLine();
            Console.WriteLine();
           
            // Display found customers
            SelectCustomers(input).GetAwaiter().GetResult();
        }

        private async Task SelectCustomers(string input)
        {
            var sqlStatement ="SELECT * FROM c WHERE CONTAINS(UPPER(c.firstname), '" + input.ToUpper() + "') OR " +
                              "CONTAINS(UPPER(c.lastname), '" + input.ToUpper() + "') OR " +
                              "CONTAINS(CONCAT(UPPER(c.firstname), ' ', UPPER(c.lastname)), '" + input.ToUpper() + "')";
            var queryDefinition = new QueryDefinition(sqlStatement);
            var queryResultSetIterator = this.customerContainer.GetItemQueryIterator<Customer>(queryDefinition);
            
            while (queryResultSetIterator.HasMoreResults)
            {
                var index = 0;
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (var customer in currentResultSet)
                {
                    index++;
                    Console.WriteLine(index + ". Customer");
                    Console.WriteLine("Name: " + customer.firstname + " " + customer.lastname);
                    Console.WriteLine("Email: " + customer.email);
                    Console.WriteLine("Date of Birth: " + customer.date_of_birth);
                    Console.WriteLine("Phone Number: " + customer.phone);
                    Console.WriteLine("Street: " + customer.street);
                    Console.WriteLine("Postcode: " + customer.postcode);
                    Console.WriteLine("City: " + customer.city);
                    Console.WriteLine("Country: " + customer.country);
                    Console.WriteLine("Bank Code: " + customer.bank_code); 
                    Console.WriteLine("Bank Account Number: " + customer.bank_account_number); 
                    Console.WriteLine();
                }
            }
        }
    }
}