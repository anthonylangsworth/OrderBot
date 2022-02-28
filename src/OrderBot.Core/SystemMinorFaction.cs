using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBot.Core
{
    public class SystemMinorFaction
    {
        // public int ID { get; }
        public string? StarSystem { get; set; }
        public string? MinorFaction { get; set; }   
        public double Influence { get; set; }
        public string? Goal { get; set; }
        public DateTime LastUpdated { get; set; } 
        public List<string> States { get; set; } = null!;
    }
}
