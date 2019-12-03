using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Cosmos_DB.Help;
using Cosmos_DB.Object;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;



namespace Cosmos_DB
{
    public class DeleteReservation{

        private readonly Container reservationContainer;

        public DeleteReservation(Container container){
            this.reservationContainer = container;
        }

        public async void Start(){
            const string sqlQueryText = "SELECT * FROM c";

            var queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = this.reservationContainer.GetItemQueryIterator<Reservation>(queryDefinition);      

            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (var reservation in currentResultSet)
                {
                    Console.WriteLine("-----------------------");
                    Console.WriteLine("Customer " + reservation.customer_id);
                    Console.WriteLine("Reservation " + reservation.apartment_id);
                    Console.WriteLine("Booking Date " + reservation.booking_date);
                    Console.WriteLine("From " + reservation.from);
                    Console.WriteLine("To " + reservation.to);
                    Console.WriteLine("Type " + reservation.type);
                    Console.WriteLine("------------------------");
                }
            } 

                 DeleteReservationInput();
        }
                private void DeleteReservationInput()
        {
                Console.Write("ID input: ");
                var customerId = Console.ReadLine();
                Console.Write("Tyoe input: ");
                var partitionKeyValue = Console.ReadLine();

                DeleteReservationItemAsync(customerId,partitionKeyValue);

            
        }
        private async Task DeleteReservationItemAsync(String customerId, String partitionKeyValue){
            await this.reservationContainer.DeleteItemAsync<Reservation>(customerId, new PartitionKey(partitionKeyValue));
            Console.WriteLine("Deleted Customer [{0},{1}]\n", customerId, partitionKeyValue);
        }
    }
}