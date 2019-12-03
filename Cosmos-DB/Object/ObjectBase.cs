using Newtonsoft.Json;

namespace Cosmos_DB.Object
{
    public class ObjectBase
    {
        [JsonProperty(PropertyName = "id")] 
        public string id { get; set; }
    }
}