using System;

namespace Data.Models
{
    public class DataSet
    {
        public string ChannelName { get; set; }
        public int Outing { get; set; }
        public double[] TimeArray { get; set; }
        public double[] ValueArray { get; set; }
        public bool Selected { get; set; }
    }
}
