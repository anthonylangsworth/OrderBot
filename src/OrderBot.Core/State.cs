using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBot.Core
{
    public class State
    {
        public int Id { get; }
        public List<StarSystemMinorFaction> StarSystemMinorFactions { get; } = new ();
        public string Name = null!;
    }
}
    