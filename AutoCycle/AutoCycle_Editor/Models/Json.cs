using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCycle.Models
{
    internal class Json
    {
        public Type Type { get; set; }
        public int Count { get; set; }
        public int Result { get; set; }
        public string Message { get; set; } = string.Empty;
    }
    
    public enum Type
    {
        None,
        Video,
        HIIT
    }
}
