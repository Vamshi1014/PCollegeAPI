using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Models.Datafields
{
    public class CachingSettings
    {
        public int AbsoluteExpirationMinutes { get; set; }
        public int SlidingExpirationMinutes { get; set; }
    }

}
