using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstantImprovement.DataControl
{
    public class RingBuffer
    {
        public RingBuffer(int capacity)
        {
            Values = new ChartValues<float>();
            RawData = new ChartValues<float>();
            Capacity = capacity;
        }

        public int Capacity {get; set;}
        public ChartValues<float> Values { get; private set; }
        public ChartValues<float> RawData { get; private set; }

        public void Add(float newValue)
        {
            if(Values.Count >= Capacity)
            {
                Values.RemoveAt(0);
            }

            Values.Add(newValue);
            RawData.Add(newValue);
        }

        public void ExportData()
        {
            throw new NotImplementedException();
        }
    }
}
