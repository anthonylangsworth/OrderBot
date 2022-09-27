using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBot.Core
{
    public class Goals
    {
        public static Goal Ignore => new Goal("Ignore", "Generate no orders for this system. Useful systems that you do not care about.");

        public static Goal Control => new Goal("Control", "Be the highest influence minor faction. Keep influence between 50% and 60%.");

        public static Goal Maintain => new Goal("Control", "Neither retreat nor control. Aim to keep influence up to 10% less than the controlling minor faction.");

        public static Goal Retreat => new Goal("Retreat", "Retreat from the system by reducing influence below 5% and keeping it there.");

        public static Goal Default => Ignore;

        public static Goal[] All => new []
        {
            Ignore,
            Control,
            Maintain,
            Retreat
        };
    }
}
