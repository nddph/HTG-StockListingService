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
            using var _context = new StockDealServiceContext();
            return await _context.StockDeals.Where(u => u.SenderId == userId || u.ReceiverId == userId).ToListAsync();
        }



        /// <summary>
        /// Lấy chi tiết stock detail
        /// </summary>
        /// <param name="stockDetailId"></param>
        /// <returns></returns>
        public async Task<StockDealDetail> GetStockDetailAsync(Guid stockDetailId)
        {
            var _context = new StockDealServiceContext();
            return await _context.StockDealDetails.FindAsync(stockDetailId);
        }

    }
}
