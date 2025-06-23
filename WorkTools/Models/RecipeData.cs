using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkTools.Models
{
    public enum Function
    {
        Delay,
        OverEtch,
        SlowPurge,
        Wait,
        Finish
    }
    public class RecipeStep: RecipeStepBase
    {
        public bool FanOn { get; set; }
    }
}
