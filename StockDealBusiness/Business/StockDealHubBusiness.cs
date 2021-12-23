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
        /// Lấy danh sách stockdeal của user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<StockDeal>> ListStockDealAsync(Guid userId)
        {
            var context = new StockDealServiceContext();
            return await context.StockDeals.Where(u => u.SenderId == userId || u.ReceiverId == userId).ToListAsync();
        }



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
            return await context.StockDeals.Include(e => e.Ticket).Where(e => e.Id == stockDealId).FirstOrDefaultAsync();
        }

    }
}
