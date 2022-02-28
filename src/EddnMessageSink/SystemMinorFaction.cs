using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddnMessageSink
{
    internal class SystemMinorFaction
    {
        public int ID { get; }
        public string? StarSystem { get; set; }
        public string? MinorFaction { get; set; }   
        public double Influence { get; set; }
        public string? Goal { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
