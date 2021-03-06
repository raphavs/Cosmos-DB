﻿using System;
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
            Console.WriteLine(">>>> SEARCH CUSTOMER");
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
                    Console.WriteLine("Name: " + customer.Firstname + " " + customer.Lastname);
                    Console.WriteLine("Email: " + customer.Email);
                    Console.WriteLine("Date of Birth: " + customer.DateOfBirth);
                    Console.WriteLine("Phone Number: " + customer.Phone);
                    Console.WriteLine("Street: " + customer.Street);
                    Console.WriteLine("Postcode: " + customer.Postcode);
                    Console.WriteLine("City: " + customer.City);
                    Console.WriteLine("Country: " + customer.Country);
                    Console.WriteLine("Bank Code: " + customer.BankCode); 
                    Console.WriteLine("Bank Account Number: " + customer.BankAccountNumber); 
                    Console.WriteLine();
                }
            }
        }
    }
}