using Microsoft.EntityFrameworkCore;
using StockDealDal.Dto.StockDeal;
using StockDealDal.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealBusiness.RequestDB
{
    public class StockDealDB
    {
        /// <summary>
        /// đánh dấu tin nhắn đã đọc cho người gửi deal hoặc người nhận deal
        /// </summary>
        /// <param name="stockDealId"></param>
        /// <param name="isUpdateForSender">true: đánh dấu tin nhắn đã đọc cho người gửi deal, ngược lại đánh dấu tin nhắn đã đọc cho người nhận deal</param>
        /// <returns></returns>
        public static async Task ReadStockDealDetailAsync(Guid stockDealId, bool isUpdateForSender)
        {
            var context = new StockDealServiceContext();

            string sql = @"UPDATE [dbo].[ST_StockDealDetail]
                        SET [{0}] = 1
                        WHERE [StockDealId] = '{1}' and ([{0}] is null or [{0}] <> 1)";

            var userRead = isUpdateForSender ? "SenderRead" : "ReceiverRead";

            sql = string.Format(sql, userRead, stockDealId);

            await context.Database.ExecuteSqlRawAsync(sql);

        }



        /// <summary>
        /// lấy danh sách stockdeal
        /// </summary>
        /// <param name="stockDealSearch"></param>
        /// <returns></returns>
        public static async Task<List<ViewListStockDeals>> ListStockDealAsync(StockDealSearchCriteria stockDealSearch)
        {

            var context = new StockDealServiceContext();

            var sql = string.Format(@"EXECUTE [GetListStockDeals] @userId = '{0}', @currentPage = {1}, @pageSize = {2}, @includeEmptyDeal = {3}, @stockDealId = {4}",
                stockDealSearch.LoginedContactId,
                stockDealSearch.CurrentPage,
                stockDealSearch.PerPage,
                stockDealSearch.IncludeEmptyDeal,
                stockDealSearch.StockDealId.HasValue ? $"'{stockDealSearch.StockDealId}'" : "null"
                );

            return await context.ViewListStockDeals.FromSqlRaw(sql).ToListAsync();
        }
    }
}
