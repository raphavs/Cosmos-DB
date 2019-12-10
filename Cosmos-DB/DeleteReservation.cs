using System;
using System.Collections.Generic;
using System.Linq;
using Cosmos_DB.Object;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;

namespace Cosmos_DB
{
    public class DeleteReservation
    {
        private readonly Container reservationContainer;
        private readonly List<Reservation> reservations;

        public DeleteReservation(Container container)
        {
            this.reservationContainer = container;
            this.reservations = new List<Reservation>();
        }

        public async void Start()
        {
            const string sqlQueryText = "SELECT * FROM c";
            var queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = this.reservationContainer.GetItemQueryIterator<Reservation>(queryDefinition);
            
            var index = 0;
            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();

                foreach (var reservation in currentResultSet)
                {
                    index++;
                    Console.WriteLine("-----------------------");
                    Console.WriteLine(index + ". Reservation");
                    Console.WriteLine("Customer ID: " + reservation.customer_id);
                    Console.WriteLine("Apartment ID: " + reservation.apartment_id);
                    Console.WriteLine("Booking Date: " + reservation.booking_date);
                    Console.WriteLine(reservation.type.Equals("Booking") ? "Booked" : "Reserved" + " in the period from " + reservation.from + " to " + reservation.to);
                    Console.WriteLine("------------------------");
                    Console.WriteLine();

                    // Add reservation
                    reservations.Add(reservation);
                }
            }
            
            var indexReservation = -1;
            while (indexReservation < 1 || indexReservation > reservations.Count)
            {
                Console.Write("Please select the reservation by entering the corresponding number: ");
                
                try
                {
                    indexReservation = Convert.ToInt32(Console.ReadLine());
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Input is not valid!");
                }
            }
            var selectedReservation = reservations.ElementAt(indexReservation - 1);
            
            DeleteReservationItemAsync(selectedReservation).GetAwaiter().GetResult();
        }
        
        private async Task DeleteReservationItemAsync(Reservation reservation)
        {
            await this.reservationContainer.DeleteItemAsync<Reservation>(reservation.id, new PartitionKey(reservation.type));
            Console.WriteLine("Deleted reservation [{0},{1}]\n", reservation.id, reservation.type);
        }
    }
}