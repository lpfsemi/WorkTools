using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkTools.Models
{
    public class VaccumData
    {
        public DateTime Time { get; set; }
        public float PS1 { get; set; }
        public float PS2 { get; set; }
        public float PS3 { get; set; }
        public float DG { get; set; }
        public float HG { get; set; }
    }
}
