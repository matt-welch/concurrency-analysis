using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ConcucrrencyTiming
{
    class FileWriter
    {
        public FileWriter(string filename, List<long> data)
        {
            string[] strArray = new string[data.Count];
            for (int i=0; i < data.Count; i++)
            {
                strArray[i] = data[i].ToString();
            }
            string path = Directory.GetCurrentDirectory();
            File.WriteAllLines(filename, strArray);
        }
    }
}
