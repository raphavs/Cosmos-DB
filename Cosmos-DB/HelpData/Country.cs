using System.Collections.Generic;

namespace Cosmos_DB.HelpData
{
    public class Country
    {
        public string name { get; set; }
        public List<City> cities { get; set; }
    }
}