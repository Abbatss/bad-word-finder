using System;
using BWF.Api.Services.Models;
using DynamoDB.Common;
using Amazon.Util;
using System.Collections.Generic;

namespace BWF.Api.Services.DynamoDB
{
    public class WordRecord : DynamoDBRecord
    {
        public const string SKValue = "BadWords";
        public BadWord Data { get; set; }
        public WordRecord()
        {
        }

        public WordRecord(string id)
        {
            Id = id;
            SK = SKValue;
        }

        public WordRecord(BadWord data) : base(data.Id, SKValue, data.Word, null)
        {
            Data = data;
        }
    }

    public class AllWordsRecord : DynamoDBRecord
    {
        public const string SKValue = "AllWords";
        public List<BadWord> Data { get; set; } = new List<BadWord>();
        public AllWordsRecord()
        {
        }

        public AllWordsRecord(string id)
        {
            Id = id;
            SK = SKValue;
        }

        public AllWordsRecord(List<BadWord> data, string id) : base(id, SKValue, id, null)
        {
            Data = data;
        }
    }
}