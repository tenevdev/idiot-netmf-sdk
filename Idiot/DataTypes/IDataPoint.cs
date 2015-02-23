using System;
using Microsoft.SPOT;

namespace Idiot.DataTypes
{
    public interface IDataPoint
    {
        /// <summary>
        /// Serialize an object to JSON string
        /// </summary>
        /// <returns>A JSON representing the serialized object</returns>
        string toJson();
    }
}
