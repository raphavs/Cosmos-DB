using System;
using Newtonsoft.Json;

namespace Cosmos_DB.Object
{
    public class Customer
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        
        [JsonProperty(PropertyName = "firstname")]
        public string Firstname { get; set; }
        
        [JsonProperty(PropertyName = "lastname")]
        public string Lastname { get; set; }
        
        [JsonProperty(PropertyName = "date_of_birth")]
        public DateTime DateOfBirth { get; set; }
        
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
        
        [JsonProperty(PropertyName = "phone")]
        public string Phone { get; set; }
        
        [JsonProperty(PropertyName = "street")]
        public string Street { get; set; }
        
        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }
        
        [JsonProperty(PropertyName = "postcode")]
        public string Postcode { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }
        
        [JsonProperty(PropertyName = "bank_code")]
        public string BankCode { get; set; }
        
        [JsonProperty(PropertyName = "bank_account_number")]
        public string BankAccountNumber { get; set; }
    }
}