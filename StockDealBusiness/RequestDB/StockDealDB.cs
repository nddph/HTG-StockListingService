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
        /// lấy danh sách stockdeal
        /// </summary>
        /// <param name="stockDealSearch"></param>
        /// <returns></returns>
        public static async Task<List<ViewListStockDeals>> ListStockDealAsync(StockDealSearchCriteria stockDealSearch)
        {

            var context = new StockDealServiceContext();

            var sql = string.Format(@"EXECUTE [GetListStockDeals_test] @userId = '{0}', @currentPage = {1}, @pageSize = {2}, @includeEmptyDeal = {3}, @stockDealId = {4}",
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
