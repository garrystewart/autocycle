using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCycle.Models
{
    internal class Json
    {
        public int Count { get; set; }
        public int? Result { get; set; }
        public string OcrResultText { get; set; } = string.Empty;
        public string Confidence { get; set; } = string.Empty;
        public bool? WithinTolerance { get; set; }
        public int? PreviousResult { get; set; }
        public bool? NoDigits { get; set; }
    }
}
