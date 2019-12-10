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
using System.Security.Cryptography;
using System.Threading;



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
                    Console.WriteLine("Customer " + reservation.customer_id);
                    Console.WriteLine("Reservation " + reservation.apartment_id);
                    Console.WriteLine("Booking Date " + reservation.booking_date);
                    Console.WriteLine("From " + reservation.from);
                    Console.WriteLine("To " + reservation.to);
                    Console.WriteLine("Type " + reservation.type);
                    Console.WriteLine("------------------------");

                    // Add reservationn
                    reservations.Add(reservation);
                }
            }
            DeleteReservationInput();
        }

        private void DeleteReservationInput()
        {
            var deleteIndex = -1;

            while (deleteIndex < 1 || deleteIndex > reservations.Count)
            {

                Console.Write("Please select the reservation by entering the corresponding number: ");

                try
                {
                    deleteIndex = Convert.ToInt32(Console.ReadLine());
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Input is not valid!");
                }
            }
            var selectedReservation = reservations.ElementAt(deleteIndex - 1);

            //Console.Write("ID input: ");
            //var customerId = Console.ReadLine();
            //Console.Write("Type input: ");
            //var partitionKeyValue = Console.ReadLine();
            DeleteReservationItemAsync(selectedReservation).GetAwaiter().GetResult();

            reservations.RemoveAt(deleteIndex);
        }



        private async Task DeleteReservationItemAsync(Reservation reservation)
        {
            await this.reservationContainer.DeleteItemAsync<Reservation>(reservation.id, new PartitionKey(reservation.type));
            Console.WriteLine("Deleted Customer [{0},{1}]\n", reservation.id, reservation.type);
        }
    }
}