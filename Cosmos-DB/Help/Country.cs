using System.Collections.Generic;

namespace Cosmos_DB.Help
{
    public class Country
    {
        public string Name { get; set; }
        public List<City> Cities { get; set; }
    }
}