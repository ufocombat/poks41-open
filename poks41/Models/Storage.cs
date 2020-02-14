using System;

using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using poks41.Models;

namespace poks41
{

    public static class ServiceProviderExtensions
    {
        public static void AddStorage(this IServiceCollection services)
        {
            services.AddSingleton<Storage>();
        }
    }

    public static class IEnumerableExtension
    {
        public static T RandomElement<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.RandomElementUsing<T>(new Random());
        }

        public static T RandomElementUsing<T>(this IEnumerable<T> enumerable, Random rand)
        {
            int index = rand.Next(0, enumerable.Count());
            return enumerable.ElementAt(index);
        }
    }

    public static class CloudTableExtension
    {
        public static async Task<TableResult> Insert(this CloudTable table, ITableEntity rec)
        {
            TableOperation operation = TableOperation.Insert(rec);
            return await table.ExecuteAsync(operation);
        }

        public static async Task Insert(this CloudTable table, IEnumerable<ITableEntity> recs)
        {
            for (int j = 0; j < recs.Count(); j += 100)
            {
                var batch = new TableBatchOperation();
                var m = recs.Skip(j).Take(100).ToList();
                foreach (ITableEntity r in m)
                {
                    batch.Insert(r);
                }
                await table.ExecuteBatchAsync(batch);
            }
        }

        public static async Task<TableResult> Delete(this CloudTable table, ITableEntity rec)
        {
            TableOperation operation = TableOperation.Delete(rec);
            return await table.ExecuteAsync(operation);
        }

        public static async Task Delete(this CloudTable table, IEnumerable<ITableEntity> recs)
        {
            for (int j = 0; j < recs.Count(); j += 100)
            {
                var batch = new TableBatchOperation();
                var m = recs.Skip(j).Take(100).ToList();
                foreach (ITableEntity r in m)
                {
                    batch.Delete(r);
                }
                await table.ExecuteBatchAsync(batch);
            }
        }
    }

    public class Storage
    {
        public static String StorageName = "rksi";
        public static String StorageKey = "z+G6KzbQLJMfnzjQ==";

        public static Uri TableUri;
        public static Uri BlobUri;
        public static Uri QueueUri;

        public static CloudTableClient tableClient;
        public static CloudBlobClient blobClient;
        public static CloudQueueClient queueClient;

        public static Microsoft.WindowsAzure.Storage.Auth.StorageCredentials Creditals;

        public Storage()
        {
            if (Creditals == null)
            {
                Creditals = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(StorageName, StorageKey);

                TableUri = new Uri($"https://{StorageName}.table.core.windows.net/");
                BlobUri = new Uri($"https://{StorageName}.blob.core.windows.net/");
                QueueUri = new Uri($"https://{StorageName}.queue.core.windows.net/");

                tableClient = new CloudTableClient(TableUri, Creditals);
                blobClient = new CloudBlobClient(BlobUri, Creditals);
                queueClient = new CloudQueueClient(QueueUri, Creditals);
            }
        }

        public CloudTable GetTable(String TableName)
        {
            return tableClient.GetTableReference(TableName);
        }

        public CloudBlobContainer GetBlob(String BlobName)
        {
            return blobClient.GetContainerReference(BlobName);
        }

        public async Task<CloudQueue> GetQueue(String QueueName)
        {
            var queue = queueClient.GetQueueReference(QueueName);
            await queue.CreateIfNotExistsAsync();

            return queue;
        }

        public async Task InsertIn(String TableName, ITableEntity rec)
        {
            var _table = GetTable(TableName);
            TableOperation operation = TableOperation.Insert(rec);
            await _table.ExecuteAsync(operation);
        }

        public async Task InsertOrReplaceIn(String TableName, ITableEntity rec)
        {
            var _table = GetTable(TableName);
            TableOperation operation = TableOperation.InsertOrReplace(rec);
            await _table.ExecuteAsync(operation);
        }

        public async Task DeleteIn(String TableName, ITableEntity rec)
        {
            rec.ETag = "*";
            var _table = GetTable(TableName);
            TableOperation operation = TableOperation.Delete(rec);
            await _table.ExecuteAsync(operation);
        }

        public async Task<T> GetRec<T>(String TableName, ITableEntity rec) where T : ITableEntity, new()
        {
            var _table = GetTable(TableName);
            TableOperation retrieveOperation = TableOperation.Retrieve<T>(rec.PartitionKey, rec.RowKey);
            var res = await _table.ExecuteAsync(retrieveOperation);
            if (res.Result == null)
                return default(T);
            else
                return (T)res.Result;
        }

        public async Task<List<T>> GetList<T>(String TableName) where T : ITableEntity, new()
        {
            TableQuery<T> wordQuery = new TableQuery<T>();

            var recs = new List<T>();
            var tabWords = GetTable(TableName);

            TableContinuationToken continuationToken = null;
            do
            {
                var ws = await tabWords.ExecuteQuerySegmentedAsync(wordQuery, continuationToken);
                recs.AddRange(ws);

                continuationToken = ws.ContinuationToken;
            } while (continuationToken != null);
            return recs;
        }

        public async Task<List<T>> GetList<T>(String TableName, String pk, int take = -1) where T : ITableEntity, new()
        {
            string filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, pk);
            TableQuery<T> wordQuery;

            if (take >= 0)
            {
                wordQuery = new TableQuery<T>().Where(filter).Take(take);
            }
            else
            {
                wordQuery = new TableQuery<T>().Where(filter);
            }

            var recs = new List<T>();
            var tabWords = GetTable(TableName);

            TableContinuationToken continuationToken = null;
            do
            {
                var ws = await tabWords.ExecuteQuerySegmentedAsync(wordQuery, continuationToken);
                recs.AddRange(ws);

                continuationToken = ws.ContinuationToken;
            } while ((continuationToken != null) && (take < 0));
            return recs;
        }

        public async Task<List<T>> GetListFavorite<T>(String TableName, String pk, int take = -1) where T : ITableEntity, new()
        {
            string filter1 = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, pk);
            string filter2 = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, pk);
            string filter = TableQuery.CombineFilters(filter1, TableOperators.And, filter2);

            TableQuery<T> wordQuery;

            if (take >= 0)
            {
                wordQuery = new TableQuery<T>().Where(filter).Take(take);
            }
            else
            {
                wordQuery = new TableQuery<T>().Where(filter);
            }

            var recs = new List<T>();
            var tabWords = GetTable(TableName);

            TableContinuationToken continuationToken = null;
            do
            {
                var ws = await tabWords.ExecuteQuerySegmentedAsync(wordQuery, continuationToken);
                recs.AddRange(ws);

                continuationToken = ws.ContinuationToken;
            } while ((continuationToken != null) && (take < 0));
            return recs;
        }

        public async Task SendMessage(String queueName, String message)
        {
            var queue = await GetQueue(queueName);
            queue.AddMessageAsync(new CloudQueueMessage(message));
            return;
        }
    }

}