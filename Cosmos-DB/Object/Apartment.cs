using Newtonsoft.Json;

namespace Cosmos_DB.Object
{
    public class Apartment
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }
        public string plz { get; set; }
        public string street { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string apartment { get; set; }
        public double qm { get; set; }
        public double price { get; set; }
        public string description { get; set; }
        public string [] additional_equipment { get; set; }
        public string [] pictures { get; set; }

    }
}