using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealBusiness.EventBus
{
    public class ConstEventBus
    {

        public static readonly IConfiguration _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();


        public const string REQUEST_METHOD = "REQUEST";
        public const string RESPONSE_METHOD = "RESPONSE";

        public static readonly string SERVICE_FILE = _configuration["EventBusConfig:Service:FILE"];
        public static readonly string SERVICE_MEMBER = _configuration["EventBusConfig:Service:MEMBER"];
        public static readonly string SERVICE_STOCKTRANS = _configuration["EventBusConfig:Service:STOCKTRANS"];


        public static readonly string EXCHANGE_FILE = _configuration["EventBusConfig:Exchange:FILE"];
        public static readonly string EXCHANGE_MEMBER = _configuration["EventBusConfig:Exchange:MEMBER"];
        public static readonly string EXCHANGE_STOCKTRANS = _configuration["EventBusConfig:Exchange:STOCKTRANS"];


        public static readonly string CURRENT_SERVICE = _configuration["EventBusConfig:CurrentService"];
        public static readonly string CURRENT_EXCHANGE = _configuration["EventBusConfig:CurrentExchange"];


        public static readonly string Publisher_GetStockHolderDetailById = $"{CURRENT_SERVICE}.{SERVICE_MEMBER}.GetStockHolderDetailById.{REQUEST_METHOD}";
        public static readonly string Publisher_GetStockDetailById = $"{CURRENT_SERVICE}.{SERVICE_STOCKTRANS}.GetStockDetail.{REQUEST_METHOD}";
        public static readonly string Publisher_GetStockTypeDetailById = $"{CURRENT_SERVICE}.{SERVICE_STOCKTRANS}.GetStockTypeDetail.{REQUEST_METHOD}";
    }
}
