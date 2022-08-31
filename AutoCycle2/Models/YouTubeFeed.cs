using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCycle2.Models
{
    internal class YouTubeFeed
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Uri? Uri { get; set; }
        public int MinimumGradient { get; set; }
        public int MaximumGradient { get; set; }
        public Rectangle AreaToMonitor { get; set; }
        public bool IsWhiteTextOnBlackBackground { get; set; }
        public int ConfidenceLevel { get; set; }
    }
}
