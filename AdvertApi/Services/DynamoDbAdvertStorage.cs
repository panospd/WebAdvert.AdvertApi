using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvertApi.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using AutoMapper;

namespace AdvertApi.Services
{
    public class DynamoDbAdvertStorage : IAdvertStorageService
    {
        private readonly IMapper _mapper;

        public DynamoDbAdvertStorage(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<AdvertDbModel> GetById(string id)
        {
            using (var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {
                    return await context.LoadAsync<AdvertDbModel>(id);
                }
            }
        }

        public async Task<List<AdvertModel>> GetAll()
        {
            using (var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {
                    var allItems = await context.ScanAsync<AdvertDbModel>(new List<ScanCondition>())
                        .GetRemainingAsync();

                    return allItems.Select(item => _mapper.Map<AdvertModel>(item)).ToList();
                }
            }
        }

        public async Task<string> Add(AdvertModel model)
        {
            var dbModel = _mapper.Map<AdvertDbModel>(model);
            dbModel.Id = Guid.NewGuid().ToString();
            dbModel.CreationDateTime = DateTime.UtcNow;
            dbModel.Status = AdvertStatus.Pending;

            using (var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {
                   await context.SaveAsync(dbModel);
                }
            }
            return dbModel.Id;
        }

        public async Task Confirm(ConfirmAdvertModel model)
        {
            using (var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {
                    var record = await context.LoadAsync<AdvertDbModel>(model.Id);

                    if(record == null)
                        throw new KeyNotFoundException($"A record with Id={model.Id} was not found.");

                    if (model.Status == AdvertStatus.Active)
                    {
                        record.FilePath = model.FilePath;
                        record.Status = AdvertStatus.Active;
                        await context.SaveAsync(record);
                    }
                    else
                    {
                        await context.DeleteAsync(record);
                    }
                }
            }
        }

        public bool CheckHealthAsync()
        {
            using (var client = new AmazonDynamoDBClient())
            {
                var tableData = client.DescribeTableAsync("Adverts").Result;
                return string.Compare(tableData.Table.TableStatus.Value, "active", StringComparison.OrdinalIgnoreCase) == 0;
            }
        }
    }
}