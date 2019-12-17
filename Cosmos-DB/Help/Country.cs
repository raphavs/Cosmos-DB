using System.Collections.Generic;

namespace Cosmos_DB.Help
{
    public class Country
    {
        public string name { get; set; }
        public List<City> cities { get; set; }
    }
}