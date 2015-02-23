using System;
using Microsoft.SPOT;

namespace Idiot.DataTypes
{
    public class Number : IDataPoint
    {
        /// <summary>
        /// Create a data point with data of Number type
        /// </summary>
        /// <param name="key">The name of the property to be added to the data object inside the data point</param>
        /// <param name="value">The value to be added to the given property of the data object inside the data point</param>
        public Number(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }

        /// <summary>
        /// Create a data point with data of Number type that has an int value
        /// </summary>
        /// <param name="key">The name of the property to be added to the data object inside the data point</param>
        /// <param name="value">The value to be added to the given property of the data object inside the data point</param>
        public Number(string key, int value) : this(key, value.ToString()) { }

        /// <summary>
        /// Create a data point with data of Number type that has a double value
        /// </summary>
        /// <param name="key">The name of the property to be added to the data object inside the data point</param>
        /// <param name="value">The value to be added to the given property of the data object inside the data point</param>
        public Number(string key, double value) : this(key, value.ToString()) { }

        /// <summary>
        /// Get the JSON representation of a Number as a key-value pair
        /// </summary>
        /// <returns>A JSON string to be written inside the body of a request in order to create this data point</returns>
        public string toJson()
        {
            return "{ \"" + this.Key + "\" : " + this.Value + " }";
        }

        /// <summary>
        /// The name of the property to be added to the data object inside the data point
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The value to be added to the given property of the data object inside the data point
        /// </summary>
        public string Value { get; set; }
    }
}
