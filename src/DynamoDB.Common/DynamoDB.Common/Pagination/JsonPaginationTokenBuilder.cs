using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DynamoDB.Common.Pagination
{
    public class JsonPaginationTokenBuilder
    {
        private JObject token;
        private List<string> keys = new List<string>();

        public JsonPaginationTokenBuilder()
        {
            token = new JObject();
        }

        public JsonPaginationTokenBuilder Add(IDictionary<string, string> tokenKeyValues)
        {
            foreach (var keyValue in tokenKeyValues)
            {
                Add(keyValue.Key, keyValue.Value);
            }

            return this;
        }

        public JsonPaginationTokenBuilder Add(KeyValuePair<string, string> keyValue)
        {
            return Add(keyValue.Key, keyValue.Value);
        }

        public JsonPaginationTokenBuilder Add(string key, string value)
        {
            if (keys.Contains(key))
            {
                return this;
            }

            keys.Add(key);
            JObject childToken = new JObject();
            childToken.Add("S", value);
            token.Add(key, childToken);
            return this;
        }

        // {"SK":{"S":"Template"},"SortData":{"S":"Name01"},"Id":{"S":"f929203b-90d5-490c-a671-1aca75ef8366"}}
        public string Build()
        {
            return token.ToString(Newtonsoft.Json.Formatting.None);
        }
    }
}