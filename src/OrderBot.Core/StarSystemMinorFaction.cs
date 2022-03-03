using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBot.Core
{
    public class StarSystemMinorFaction
    {
        public int Id;
        public StarSystem StarSystem = null!;
        public MinorFaction MinorFaction = null!;
        public double Influence;
        public List<State> States = new();
    }
}
