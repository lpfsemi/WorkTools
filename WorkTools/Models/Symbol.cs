using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkTools.Models
{
    public class Symbol
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public Type Type { get; set; }
        public bool CanRead { get; set; }
        public bool IsVisiable { get; set; }
    }
}
