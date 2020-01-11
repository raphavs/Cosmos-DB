using Newtonsoft.Json;

namespace Cosmos_DB.Object
{
    public class Apartment
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        
        [JsonProperty(PropertyName = "postcode")]
        public string Postcode { get; set; }
        
        [JsonProperty(PropertyName = "street")]
        public string Street { get; set; }
        
        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }
        
        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }
        
        [JsonProperty(PropertyName = "apartment_number")]
        public string ApartmentNumber { get; set; }
        
        [JsonProperty(PropertyName = "sqm")]
        public int SquareMeters { get; set; }
        
        [JsonProperty(PropertyName = "price")]
        public double Price { get; set; }
        
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        
        [JsonProperty(PropertyName = "additional_equipment")]
        public string [] AdditionalEquipment { get; set; }
        
        [JsonProperty(PropertyName = "pictures")]
        public string [] Pictures { get; set; }
    }
}