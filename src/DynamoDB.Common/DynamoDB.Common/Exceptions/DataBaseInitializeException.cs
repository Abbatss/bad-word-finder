using System;
using System.Runtime.Serialization;

namespace DynamoDB.Common.Exceptions
{
    [Serializable]
    public class DataBaseInitializationException : Exception
    {
        public DataBaseInitializationException()
        {
        }

        public DataBaseInitializationException(string message) : base(message)
        {
        }

        public DataBaseInitializationException(string message, Exception inner) : base(message, inner)
        {
        }

        protected DataBaseInitializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
