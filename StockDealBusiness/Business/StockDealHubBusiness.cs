using Microsoft.EntityFrameworkCore;
using StockDealDal.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealBusiness.Business
{
    public class StockDealHubBusiness : BaseBusiness
    {


        /// <summary>
        /// Lấy chi tiết stock detail
        /// </summary>
        /// <param name="stockDetailId"></param>
        /// <returns></returns>
        public async Task<StockDealDetail> GetStockDetailAsync(Guid stockDetailId)
        {
            var context = new StockDealServiceContext();
            return await context.StockDealDetails.FindAsync(stockDetailId);
        }



        /// <summary>
        /// Lấy chi tiết stockdeal
        /// </summary>
        /// <param name="stockDealId"></param>
        /// <returns></returns>
        public async Task<StockDeal> GetStockDealAsync(Guid stockDealId)
        {
            var context = new StockDealServiceContext();
            return await context.StockDeals.AsNoTracking()
                .Include(e => e.Ticket)
                .Where(e => e.Id == stockDealId)
                .FirstOrDefaultAsync();
        }

    }
}
