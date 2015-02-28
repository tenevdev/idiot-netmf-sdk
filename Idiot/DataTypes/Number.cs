namespace Idiot.DataTypes
{
    public class Number : DataPoint
    {
        /// <summary>
        /// Create a data point with data of Number type that has an int value
        /// </summary>
        /// <param name="key">The name of the property to be added to the data object inside the data point</param>
        /// <param name="value">The value to be added to the given property of the data object inside the data point</param>
        public Number(int value) : base(value, "Number") { }

        /// <summary>
        /// Create a data point with data of Number type that has a double value
        /// </summary>
        /// <param name="key">The name of the property to be added to the data object inside the data point</param>
        /// <param name="value">The value to be added to the given property of the data object inside the data point</param>
        public Number(double value) : base(value, "Number") { }
    }
}
