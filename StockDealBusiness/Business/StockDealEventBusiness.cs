using StockDealBusiness.RequestDB;
using StockDealDal.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealBusiness.Business
{
    /// <summary>
    /// Business xử lý yêu cầu từ EventBus liên quan đến stockdeal
    /// </summary>
    public class StockDealEventBusBusiness : BaseBusiness
    {
        /// <summary>
        /// đếm số lượng stockdeal chưa đọc
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<BaseResponse> CountUnreadDealAsync(Guid userId)
        {
            var listStockDeal = await StockDealDB.ListStockDealAsync(new()
            {
                PerPage = 1,
                CurrentPage = 1,
                IncludeEmptyDeal = false,
                LoginedContactId = userId
            });

            var count = listStockDeal.Select(e => e.TotalUnread).FirstOrDefault();

            return SuccessResponse(count);
        }
    }
}
