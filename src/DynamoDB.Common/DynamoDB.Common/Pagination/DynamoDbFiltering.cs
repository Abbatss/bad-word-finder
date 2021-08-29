using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.DocumentModel;

namespace DynamoDB.Common.Pagination
{
    public class DynamoDbFiltering
    {
        private List<DynamoTableIndex> Indexes { get; }
        private readonly List<DynamoTableIndex> availableIndexes;
        private List<(string attributeName, ScanOperator scanOperator, string value)> Filters { get; } = new List<(string attributeName, ScanOperator scanOperator, string value)>();

        public DynamoDbFiltering(IEnumerable<DynamoTableIndex> availableIndexes, DynamoTableIndex defaultIndex)
        {
            if (availableIndexes == null)
            {
                throw new ArgumentNullException(nameof(availableIndexes));
            }

            if (defaultIndex == null)
            {
                throw new ArgumentNullException(nameof(defaultIndex));
            }

            this.availableIndexes = new List<DynamoTableIndex>(availableIndexes);
            Indexes = new List<DynamoTableIndex>() { defaultIndex };
        }

        public DynamoTableIndex ChooseIndex()
        {
            return Indexes.First(i => i.SearchPriority == Indexes.Max(n => n.SearchPriority));
        }

        public void AddFilter(string attributeName, ScanOperator scanOperator, string value)
        {
            if (value == null)
            {
                return;
            }

            (string attributeName, ScanOperator scanOperator, string value) filter = (attributeName, scanOperator, value);
            Filters.Add(filter);
            if (!AddIndexIfExists(scanOperator, i => i.PartitionKey == attributeName && MapOperator(scanOperator) == QueryOperator.Equal))
            {
                AddIndexIfExists(scanOperator, i => i.SortKey == attributeName && MapOperator(scanOperator) != null);
            }
        }

        internal void AddFilters(DynamoTableIndex tableIndex, QueryOperationConfig config)
        {
            foreach (var condition in Filters)
            {
                if ((tableIndex.SortKey == condition.attributeName || tableIndex.PartitionKey == condition.attributeName) && MapOperator(condition.scanOperator) != null)
                {
                    config.Filter.AddCondition(condition.attributeName, MapOperator(condition.scanOperator).Value, condition.value);
                }
                else
                {
                    config.Filter.AddCondition(condition.attributeName, condition.scanOperator, condition.value);
                }
            }
        }

        private bool AddIndexIfExists(ScanOperator scanOperator, Func<DynamoTableIndex, bool> conditionSearchIndex)
        {
            var index = availableIndexes.FirstOrDefault(i => conditionSearchIndex(i));
            if (index != null && MapOperator(scanOperator) != null)
            {
                Indexes.Add(index);
                return true;
            }

            return false;
        }

        private QueryOperator? MapOperator(ScanOperator scanOperator)
        {
            switch (scanOperator)
            {
                case ScanOperator.Equal:
                    return QueryOperator.Equal;
                case ScanOperator.LessThanOrEqual:
                    return QueryOperator.LessThanOrEqual;
                case ScanOperator.LessThan:
                    return QueryOperator.LessThan;
                case ScanOperator.GreaterThan:
                    return QueryOperator.GreaterThan;
                case ScanOperator.GreaterThanOrEqual:
                    return QueryOperator.GreaterThanOrEqual;
                case ScanOperator.BeginsWith:
                    return QueryOperator.BeginsWith;
                case ScanOperator.Between:
                    return QueryOperator.Between;
                default:
                    return null;
            }
        }
    }
}
