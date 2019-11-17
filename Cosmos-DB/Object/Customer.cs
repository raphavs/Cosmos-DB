using Newtonsoft.Json;

namespace Cosmos_DB.Object
{
    public class Customer
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string date_of_birth { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string street { get; set; }
        public string city { get; set; }
        public string plz { get; set; }
        public string country { get; set; }
        public string blz { get; set; }
        public string bank_account_number { get; set; }
    }
}