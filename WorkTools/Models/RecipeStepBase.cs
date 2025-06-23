using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkTools.Models
{
    public class RecipeStepBase
    {
            [DisplayName("步骤号")]
            public int Id { get; set; }

            [DisplayName("步骤名称")]
            public string Name { get; set; } = string.Empty;

            [DisplayName("功能")]
            public Function Function { get; set; }

            [DisplayName("步骤执行时间，单位秒")]
            public UInt16 Time { get; set; } = 1;

            [DisplayName("温度，单位℃")]
            public int Temp { get; set; }

            [DisplayName("氧气")]
            public float O2 { get; set; }

            public float N2 { get; set; }

            public string Gas3 { get; set; } = string.Empty;

            public string Gas4 { get; set; } = string.Empty;

            public string Gas5 { get; set; } = string.Empty;

            public UInt16 RFPower { get; set; }

            public float VacuumPressure { get; set; }

            public int EndPointDelect { get; set; }

            public bool RF { get; set; }
        }
}
