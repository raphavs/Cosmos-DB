using Newtonsoft.Json;

namespace Cosmos_DB
{
    public class Person
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }
        public string name { get; set; }
        public string passion { get; set; }
    }
}