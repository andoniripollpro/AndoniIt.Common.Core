using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndoIt.Common.Common
{
    public class RandomExtension
    {
        public static Random GetRandomFromDateTick()
        {
            return new Random(GetTickForSeed());
        }
        private static int GetTickForSeed()
        {
            return (int)(DateTime.Now.Ticks % int.MaxValue);
        }
    }
}
