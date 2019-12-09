using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Cosmos_DB.HelpData;
using Cosmos_DB.Object;
using Microsoft.Azure.Cosmos;

namespace Cosmos_DB
{
    public class SearchCustomer
    {
        private readonly Container customerContainer;

        public SearchCustomer(Container container)
        {
            this.customerContainer = container;
        }

        public async void Start()
        {
           Console.WriteLine("Please enter the string you are searching for: ");
           var name = Console.ReadLine();

           //Search Customer Query
           var sqlStatement ="SELECT * FROM customer c WHERE CONTAINS (UPPER(c.firstname), '"+ name.ToUpper() +"') OR CONTAINS (UPPER(c.lastname), '" + name.ToUpper() +"')";

           Console.WriteLine(sqlStatement);
           Console.WriteLine("Running query: {0}\n", sqlStatement);

           var queryDefinition = new QueryDefinition(sqlStatement);
           var queryResultSetIterator = this.customerContainer.GetItemQueryIterator<Customer>(queryDefinition);
            
            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (var customer in currentResultSet)
                {
                    Console.WriteLine("name:" + customer.firstname + " " + customer.lastname);
                    Console.WriteLine("customer number: " + customer.id);
                    Console.WriteLine("email: " + customer.email);
                    Console.WriteLine("birthday: " + customer.date_of_birth);
                    Console.WriteLine("phone number: " + customer.phone);
                    Console.WriteLine("street: " + customer.street);
                    Console.WriteLine("city: " + customer.city);
                    Console.WriteLine("postcode: " + customer.postcode);
                    Console.WriteLine("country: " + customer.country);
                    Console.WriteLine("bank code: " + customer.bank_code); 
                    Console.WriteLine("bank account number: " + customer.bank_account_number); 
                    Console.WriteLine();
                }
            }
        }
    }
}