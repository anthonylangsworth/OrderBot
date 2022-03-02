using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBot.Core
{
    public class SystemMinorFaction
    {
        public int Id;
        public string StarSystem = null!;
        public string MinorFaction = null!;
        public double Influence;
        public DateTime LastUpdated;
        public List<SystemMinorFactionState> States = new();
    }
}
