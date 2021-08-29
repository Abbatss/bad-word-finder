using System;
using System.Runtime.Serialization;

namespace DynamoDB.Common.Exceptions
{
    [Serializable]
    public class DynamoDBConcurrencyException : Exception
    {
        public DynamoDBConcurrencyException()
        {
        }

        public DynamoDBConcurrencyException(string message) : base(message)
        {
        }

        public DynamoDBConcurrencyException(string message, Exception inner) : base(message, inner)
        {
        }

        protected DynamoDBConcurrencyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
