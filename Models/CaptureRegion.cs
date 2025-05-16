using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroMechanicsHub.Models
{
    public class CaptureRegion
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public bool IsValid()
        {
            return Width > 0 && Height > 0 && X >= 0 && Y >= 0;
        }
    }
}
