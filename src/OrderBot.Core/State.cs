using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBot.Core
{
    public class State
    {
        public int Id;
        public List<StarSystemMinorFaction> StarSystemMinorFactions = new ();
        public string Name = null!;
    }
}
    