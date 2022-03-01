using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBot.Core
{
    public class SystemMinorFaction
    {
        public int ID { get; }
        public string StarSystem { get; set; } = null!;
        public string MinorFaction { get; set; } = null!;   
        public double Influence { get; set; }
        public DateTime LastUpdated { get; set; } 
        public List<SystemMinorFactionState> States { get; } = new List<SystemMinorFactionState>();
    }
}
