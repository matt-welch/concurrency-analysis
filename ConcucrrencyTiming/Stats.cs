using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcucrrencyTiming
{
    public class Stats
    {
        public double mean { get; set; }
        public double min { get; set; }
        public double max { get; set; }
        public double std { get; set; }
        public double sum { get; set; }
        public long count { get; set; }
        public double med { get; set; }
        public bool validStats;

        public Stats(long[] data)
        {
            sum = Convert.ToDouble(data.Sum());
            max = Convert.ToDouble(data.Max());
            min = Convert.ToDouble(data.Min());
            count = data.Count();
            mean = sum / Convert.ToDouble(count);
            calcStd(data);
            long [] sorted = data;
            Array.Sort(sorted);
            med = sorted[count / 2];
            validStats = true;
        }
        private void calcStd(long[] data)
        {
            // Calculate the total for the standard deviation
            double sumMeanDiffs = 0;
            for (int i = 0; i < count; i++)
                sumMeanDiffs += Math.Pow(data[i] - mean, 2);
            // calculate the standard deviation (assume sample population)
            double variance = sumMeanDiffs / (count - 1);
            std = Math.Sqrt(variance);
        }
        public override string ToString()
        {
            string output = "Statistics are not yet valid.  Initialize with Stats(long[]).";
            if (validStats)
            {
                output = String.Format("#: {0}, Min:{1}, Mean:{2:0.00}, Med:{3}, Std:{4:0.00}, Max:{5}",
                    count, min, mean, med, std, max);
            }
            return output;
        }
    }
}
