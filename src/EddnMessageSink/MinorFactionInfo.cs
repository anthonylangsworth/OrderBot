using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddnMessageProcessor
{
    // Must be public for tests
    public record MinorFactionInfo(string minorFaction, double influence, string[] states);
}
