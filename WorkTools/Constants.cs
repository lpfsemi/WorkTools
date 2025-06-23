using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkTools
{
    public class Constants
    {
        private static readonly string BaseDirctory = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string Path_Data_File_Dir = Path.Combine(BaseDirctory, "Datas");
    }
}
