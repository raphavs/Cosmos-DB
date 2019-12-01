using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Cosmos_DB.HelpData;
using Cosmos_DB.Object;
using Microsoft.Azure.Cosmos;

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
                    Console.WriteLine("-------------------");
                    Console.WriteLine(reservation.customer_id);
                    Console.WriteLine("-------------------");
                    Console.WriteLine();
                }
            } 
        }

    }
}