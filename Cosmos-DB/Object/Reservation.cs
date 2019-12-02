using System;
using Newtonsoft.Json;

namespace Cosmos_DB.Object
{
    public class Reservation
    {
        [JsonProperty(PropertyName = "id")] 
        public string id { get; set; }
        public string customer_id { get; set; }
        public string apartment_id { get; set; }
        public DateTime? booking_date { get; set; }
        public string invoice_number { get; set; }
        public DateTime? receipt_of_payment { get; set; }
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        public string type { get; set; }
        public DateTime? invoice_date { get; set; }
        public double? invoice_amount { get; set; }
    }
}