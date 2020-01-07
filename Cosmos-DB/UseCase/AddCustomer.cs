using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cosmos_DB.Help;
using Cosmos_DB.Object;
using Microsoft.Azure.Cosmos;

namespace Cosmos_DB.UseCase
{
    public class AddCustomer
    {
        private readonly Container customerContainer;
        private readonly EncryptService encryptService;
        private readonly Regex regexEmail;
        private readonly Regex regexPhone;
        private readonly List<Country> countries;

        public AddCustomer(Container container, EncryptService encryptService)
        {
            this.customerContainer = container;
            this.encryptService = encryptService;
            this.countries = new List<Country>();
            this.regexEmail = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            this.regexPhone = new Regex(@"^(\(?\+\d{2,3}\)?|\d)(\s|\-)?\d{3,4}(\s|\-)?\d{6,8}$");
        }
        
        public void Start()
        {
            Console.WriteLine();
            Console.WriteLine(">>>> ADD CUSTOMER");
            Console.WriteLine();
            
            // Collect all countries, cities and postcodes to make them selectable for the user
            SetCountries().GetAwaiter().GetResult();
            
            // Create object
            var customer = GetCustomer();
   
            // Add customer to database
            CreateCustomer(customer).GetAwaiter().GetResult();
        }

        private async Task CreateCustomer(Customer customer)
        {
            var sha256 = SHA256.Create();
            var valueToHash = string.Concat(customer.firstname, customer.lastname, customer.email);
            var id = "";
            
            try
            {
                while (true)
                {
                    id = encryptService.GenerateHash(sha256, valueToHash);

                    // Check if the ID is already assigned
                    var customerResponse = await this.customerContainer.ReadItemAsync<Customer>(id, new PartitionKey(customer.country));
                    Console.WriteLine("Customer in database with id: {0} already exists\n", customerResponse.Resource.id);

                    valueToHash = id;
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                customer.id = id;
                
                // Create the customer
                var customerResponse = await this.customerContainer.CreateItemAsync<Customer>(customer, new PartitionKey(customer.country));
                Console.WriteLine("Created customer in database with id: {0}\n", customerResponse.Resource.id);
            }
        }

        private Customer GetCustomer()
        {
            Console.Write("Please enter your FIRSTNAME: ");
            var firstname = Console.ReadLine();
            Console.WriteLine();
            
            Console.Write("Please enter your LASTNAME: ");
            var lastname = Console.ReadLine();
            Console.WriteLine();
            
            var dateOfBirth = new DateTime();
            var dateFormats = new[] {"dd-MM-yyyy"};
            var dateIsValid = false;
            while (!dateIsValid)
            {
                Console.Write("Please enter your BIRTHDAY (dd-MM-yyyy): ");
                var input = Console.ReadLine();
                dateIsValid = DateTime.TryParseExact(
                    input,
                    dateFormats,
                    DateTimeFormatInfo.InvariantInfo,
                    DateTimeStyles.None, 
                    out dateOfBirth);

                if (!dateIsValid)
                {
                    Console.WriteLine("DATE invalid!");
                }
            }
            Console.WriteLine();
            
            var email = "";
            var emailIsValid = false;
            while (!emailIsValid)
            {
                Console.Write("Please enter your MAIL ADDRESS: ");
                email = Console.ReadLine();

                if (regexEmail.IsMatch(email))
                {
                    emailIsValid = true;
                }
                else
                {
                    Console.WriteLine("MAIL ADDRESS invalid!");
                }
            }
            Console.WriteLine();

            var phone = "";
            var phoneIsValid = false;
            while (!phoneIsValid)
            {
                Console.Write("Please enter your PHONE NUMBER: ");
                phone = Console.ReadLine();

                if (regexPhone.IsMatch(phone))
                {
                    phoneIsValid = true;
                }
                else
                {
                    Console.WriteLine("PHONE NUMBER invalid!");
                }
            }
            Console.WriteLine();
            
            // Selection of country
            var indexCountry = -1;
            while (indexCountry < 1 || indexCountry > countries.Count)
            {
                Console.WriteLine("Select the COUNTRY you live in.");
                foreach (var country in countries)
                {
                    Console.Write(countries.IndexOf(country) + 1 + ". ");
                    Console.WriteLine(country.name);
                }
                Console.Write("Please enter the corresponding number: ");
                
                try
                {
                    indexCountry = Convert.ToInt32(Console.ReadLine());
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Input is not valid!");
                }
            }
            var selectedCountry = countries.ElementAt(indexCountry - 1);
            Console.WriteLine("You have chosen: " + selectedCountry.name);
            Console.WriteLine();

            // Selection of city
            var cities = selectedCountry.cities;
            var indexCity = -1;
            while (indexCity < 1 || indexCity > cities.Count)
            {
                Console.WriteLine("Select the CITY you live in.");
                foreach (var city in cities)
                {
                    Console.Write(cities.IndexOf(city) + 1 + ". ");
                    Console.WriteLine(city.name);
                }
                Console.Write("Please enter the corresponding number: ");

                try
                {
                    indexCity = Convert.ToInt32(Console.ReadLine());
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Input is not valid!");
                }
            }
            var selectedCity = cities.ElementAt(indexCity - 1);
            Console.WriteLine("You have chosen: " + selectedCity.name);
            Console.WriteLine();
            
            // Selection of postcode
            var postcodes = selectedCity.postcodes;
            var indexPostcode = -1;
            while (indexPostcode < 1 || indexPostcode > postcodes.Count)
            {
                Console.WriteLine("Select the POSTCODE you live in.");
                foreach (var postcode in postcodes)
                {
                    Console.Write(postcodes.IndexOf(postcode) + 1 + ". ");
                    Console.WriteLine(postcode);
                }
                Console.Write("Please enter the corresponding number: ");
                
                try
                {
                    indexPostcode = Convert.ToInt32(Console.ReadLine());
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Input is not valid!");
                }
            }
            var selectedPostcode = postcodes.ElementAt(indexPostcode - 1);
            Console.WriteLine("You have chosen: " + selectedPostcode);
            Console.WriteLine();
            
            Console.Write("Please enter the STREET you live in: ");
            var street = Console.ReadLine();
            Console.WriteLine();

            Console.Write("Please enter your BANK CODE: ");
            var bankCode = Console.ReadLine();
            Console.WriteLine();
            
            Console.Write("Please enter your BANK ACCOUNT NUMBER: ");
            var bankAccountNumber = Console.ReadLine();
            Console.WriteLine();
            
            return new Customer
            {
                firstname = firstname,
                lastname = lastname,
                date_of_birth = dateOfBirth,
                email = email,
                phone = phone,
                street = street,
                city = selectedCity.name,
                postcode = selectedPostcode,
                country = selectedCountry.name,
                bank_code = bankCode,
                bank_account_number = bankAccountNumber
            };
        }

        private async Task SetCountries()
        {
            const string sqlQueryText = "SELECT * FROM c";
            var queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = this.customerContainer.GetItemQueryIterator<Customer>(queryDefinition);

            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (var customer in currentResultSet)
                {
                    var country = customer.country;
                    var city = customer.city;
                    var postcode = customer.postcode;
                    
                    // Check if country already exists
                    var tempCountry = countries.Find(a => a.name.Equals(country));

                    if (tempCountry == null)
                    {
                        tempCountry = new Country
                        {
                            name = country,
                            cities = new List<City>()
                        };

                        // Add country
                        countries.Add(tempCountry);
                    }

                    // Check if city already exists in country
                    var tempCity = tempCountry.cities.Find(c => c.name.Equals(city));

                    if (tempCity == null)
                    {
                        tempCity = new City
                        {
                            name = city,
                            postcodes = new List<string>()
                        };

                        // Add city to country
                        tempCountry.cities.Add(tempCity);
                    }
                    
                    // Check if postcode already exists in city
                    if (!tempCity.postcodes.Contains(postcode))
                    {
                        // Add postcode to city
                        tempCity.postcodes.Add(postcode);
                    }
                }
            }
        }
    }
}