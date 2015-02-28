using System;

namespace Idiot.DataTypes
{
    public abstract class DataPoint
    {
        public DataPoint(object value, string type, DateTime timeStamp)
        {
            this.data = value;
            this.dataType = type;
            this.timeStamp = timeStamp;
        }

        public DataPoint(object value, string type) : this(value, type, DateTime.Now) { }

        public string dataType { get; set; }

        public DateTime timeStamp { get; set; }

        public object data { get; set; }
    }
}
