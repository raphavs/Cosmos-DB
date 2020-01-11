using System;
using System.Collections.Generic;
using System.Linq;
using Cosmos_DB.Object;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;

namespace Cosmos_DB.UseCase
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

        public void Start()
        {
            Console.WriteLine();
            Console.WriteLine(">>>> DELETE RESERVATION");
            Console.WriteLine();
            
            // Set reservations
            SetReservations().GetAwaiter().GetResult();
            
            // Select reservation
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
            
            // Delete reservation
            Delete(selectedReservation).GetAwaiter().GetResult();
        }

        private async Task SetReservations()
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
                    Console.WriteLine("Customer ID: " + reservation.CustomerId);
                    Console.WriteLine("Apartment ID: " + reservation.ApartmentId);
                    Console.WriteLine("Booking Date: " + reservation.BookingDate);
                    Console.WriteLine((reservation.Type.Equals("booking") ? "Booked" : "Reserved") + " in the period " +
                                      "from " + reservation.Of + " to " + reservation.To);
                    Console.WriteLine("------------------------");
                    Console.WriteLine();

                    // Add reservation
                    reservations.Add(reservation);
                }
            }
        }
        
        private async Task Delete(Reservation reservation)
        {
            await this.reservationContainer.DeleteItemAsync<Reservation>(reservation.Id, new PartitionKey(reservation.Type));
            Console.WriteLine("Reservation successfully deleted.");
        }
    }
}