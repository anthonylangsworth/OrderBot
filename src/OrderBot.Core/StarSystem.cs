using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBot.Core
{
    public class StarSystem
    {
        public int Id { get; private set; }
        public string Name = null!;
        public DateTime LastUpdated;
    }
}
