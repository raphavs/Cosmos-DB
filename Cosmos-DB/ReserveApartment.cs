﻿using System;
using Cosmos_DB.Object;
using Microsoft.Azure.Cosmos;

namespace Cosmos_DB
{
    public class ReserveApartment
    {
        private readonly Container customerContainer;
        private readonly Container apartmentContainer;
        private readonly Container reservationContainer;

        public ReserveApartment(Container customerContainer, Container apartmentContainer, Container reservationContainer)
        {
            this.customerContainer = customerContainer;
            this.apartmentContainer = apartmentContainer;
            this.reservationContainer = reservationContainer;
        }
        
        public async void Start()
        {
            const string sqlQueryText = "SELECT * FROM c";
            var queryDefinition = new QueryDefinition(sqlQueryText);

            // Select reservations
            var queryResultReservationIterator = this.reservationContainer.GetItemQueryIterator<Reservation>(queryDefinition);
            while (queryResultReservationIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultReservationIterator.ReadNextAsync();
                foreach (var reservation in currentResultSet)
                {
                    Console.WriteLine();
                    Console.WriteLine(reservation.id);
                    Console.WriteLine(reservation.booking_date);
                    Console.WriteLine(reservation.from);
                    Console.Write(reservation.to);
                    Console.WriteLine();
                }
            }
        }
    }
}