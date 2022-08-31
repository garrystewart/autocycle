using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCycle2.Models
{
    internal class Json
    {
        public int? Result { get; set; }
        public string OcrResultText { get; set; } = string.Empty;
        public string Confidence { get; set; } = string.Empty;
        public bool? WithinTolerance { get; set; }
    }
}
