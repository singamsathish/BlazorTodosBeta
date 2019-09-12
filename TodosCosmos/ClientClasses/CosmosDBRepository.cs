﻿using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TodosCosmos.DocumentClasses;

namespace TodosCosmos.ClientClasses
{
    public class CosmosDBRepository<T> where T : class
    {

        public async Task<T> GetItemAsync(string id, string pk)
        {
            return await CosmosAPI.container.ReadItemAsync<T>(id, new PartitionKey(pk));
        }

        public async Task<IEnumerable<T>> GetItemsAsync()
        {
            List<T> result = new List<T>();

            var setIterator = CosmosAPI.container.GetItemLinqQueryable<T>().ToFeedIterator();


            while (setIterator.HasMoreResults)
            {

                result.AddRange(await setIterator.ReadNextAsync());
            }

            return result.AsEnumerable();
        }

        public async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
        {
            List<T> result = new List<T>();

            var setIterator = CosmosAPI.container.GetItemLinqQueryable<T>().Where(predicate).ToFeedIterator();


            while (setIterator.HasMoreResults)
            {

                result.AddRange(await setIterator.ReadNextAsync());
            }

            return result.AsEnumerable();
        }


        public async Task<T> FindFirstItemsAsync(Expression<Func<T, bool>> predicate)
        {
            List<T> result = new List<T>();


            try
            {

                var feedIterator = CosmosAPI.container.GetItemLinqQueryable<T>().Where(predicate).ToFeedIterator();

                while (feedIterator.HasMoreResults)
                {
                    result.AddRange(await feedIterator.ReadNextAsync());
                }

            }
            catch (CosmosException ex)
            {

                await CosmosAPI.cosmosDBClientError.AddErrorLog(Guid.Empty, ex.Message, MethodBase.GetCurrentMethod());

            }

     
            if (result.Count() > 0)
            {
              
                return result.AsEnumerable().FirstOrDefault();
            }
            else
            {
               
                return null;
            }

            
        }


        public async Task<T> FindLastItemsAsync(Expression<Func<T, bool>> predicate)
        {
            List<T> result = new List<T>();


            try
            {

                var feedIterator = CosmosAPI.container.GetItemLinqQueryable<T>().Where(predicate).ToFeedIterator();

                while (feedIterator.HasMoreResults)
                {
                    result.AddRange(await feedIterator.ReadNextAsync());
                }

            }
            catch (CosmosException ex)
            {

                await CosmosAPI.cosmosDBClientError.AddErrorLog(Guid.Empty, ex.Message, MethodBase.GetCurrentMethod());

            }


            if (result.Count() > 0)
            {

                return result.AsEnumerable().LastOrDefault();
            }
            else
            {

                return null;
            }


        }


        public async Task<T> FindFirstItemsAsync(QueryDefinition sqlQuery)
        {
            List<T> result = new List<T>();

            FeedIterator<T> feedIterator = CosmosAPI.container.GetItemQueryIterator<T>(sqlQuery);

            while (feedIterator.HasMoreResults)
            {
                result.AddRange(await feedIterator.ReadNextAsync());
            }


            if (result.Count() > 0)
            {
                return result.AsEnumerable().FirstOrDefault();
            }
            else
            {
                return null;
            }


        }


        public async Task<IEnumerable<T>> GetItemsAsync(QueryDefinition sqlQuery)
        {

            List<T> result = new List<T>();

            FeedIterator<T> feedIterator = CosmosAPI.container.GetItemQueryIterator<T>(sqlQuery);

            while (feedIterator.HasMoreResults)
            {
                result.AddRange(await feedIterator.ReadNextAsync());
            }


            if (result.Any())
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        

        public async Task<T> CreateItemAsync(T item)
        {

            return await CosmosAPI.container.CreateItemAsync(item);

        }

        public async Task<T> UpdateItemAsync(T item)
        {
            return await CosmosAPI.container.UpsertItemAsync(item);
        }


        public async Task DeleteItemAsync(string id, string pk)
        {
            await CosmosAPI.container.DeleteItemAsync<T>(id, new PartitionKey(pk));
        }


        public int GetCount(Expression<Func<T, bool>> predicate)
        {

            return CosmosAPI.container.GetItemLinqQueryable<T>().Where(predicate).AsEnumerable().Count();

        }

    }
}