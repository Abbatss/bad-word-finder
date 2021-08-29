// <copyright file="CreateRequestExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DynamoDB.Common.Extensions
{
    using System.Collections.Generic;
    using Amazon.DynamoDBv2;
    using Amazon.DynamoDBv2.Model;

    public static class CreateRequestExtensions
    {
        public static void AddIndex(this CreateTableRequest createRequest, DynamoTableIndex tableIndex, Projection projection)
        {
            if (createRequest.GlobalSecondaryIndexes == null)
            {
                createRequest.GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>();
            }

            var gsiIndex = new GlobalSecondaryIndex()
            {
                IndexName = tableIndex.Index,
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement(tableIndex.PartitionKey, KeyType.HASH),
                    new KeySchemaElement(tableIndex.SortKey, KeyType.RANGE),
                },
                Projection = projection,
            };
            if (!createRequest.AttributeDefinitions.Exists(a => a.AttributeName == tableIndex.PartitionKey))
            {
                createRequest.AttributeDefinitions.Add(new AttributeDefinition(tableIndex.PartitionKey, ScalarAttributeType.S));
            }

            if (!createRequest.AttributeDefinitions.Exists(a => a.AttributeName == tableIndex.SortKey))
            {
                createRequest.AttributeDefinitions.Add(new AttributeDefinition(tableIndex.SortKey, ScalarAttributeType.S));
            }

            createRequest.GlobalSecondaryIndexes.Add(gsiIndex);
        }
    }
}
