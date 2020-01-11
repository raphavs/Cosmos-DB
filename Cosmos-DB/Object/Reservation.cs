using System;
using Newtonsoft.Json;

namespace Cosmos_DB.Object
{
    public class Reservation
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        
        [JsonProperty(PropertyName = "customer_id")]
        public string CustomerId { get; set; }
        
        [JsonProperty(PropertyName = "apartment_id")]
        public string ApartmentId { get; set; }
        
        [JsonProperty(PropertyName = "booking_date")]
        public DateTime BookingDate { get; set; }
        
        [JsonProperty(PropertyName = "invoice_number")]
        public string InvoiceNumber { get; set; }
        
        [JsonProperty(PropertyName = "receipt_of_payment")]
        public DateTime? ReceiptOfPayment { get; set; }
        
        [JsonProperty(PropertyName = "of")]
        public DateTime Of { get; set; }
        
        [JsonProperty(PropertyName = "to")]
        public DateTime To { get; set; }
        
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        
        [JsonProperty(PropertyName = "invoice_date")]
        public DateTime? InvoiceDate { get; set; }
        
        [JsonProperty(PropertyName = "invoice_amount")]
        public double? InvoiceAmount { get; set; }
    }
}