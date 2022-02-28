using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddnMessageSink
{
    record Goal(string name, string description);

    record MinorFactionSystemGoal(string system, string minorFaction, Goal goal);
}
