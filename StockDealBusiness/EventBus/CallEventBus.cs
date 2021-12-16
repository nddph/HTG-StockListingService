using Newtonsoft.Json;
using StockDealDal.Dto;
using StockDealDal.Dto.EventBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealBusiness.EventBus
{
    public static class CallEventBus
    {
        public static int GetStockHolderLimit(Guid stockHolderId, Guid stockId)
        {
            return int.MaxValue;
        }



        /// <summary>
        /// Lấy thông tin chi tiết nhà cổ đông
        /// </summary>
        /// <param name="stockHolderId"></param>
        /// <param name="isReply"></param>
        /// <returns></returns>
        public static async Task<StockHolderDto> GetStockHolderDetail(Guid stockHolderId, bool isReply = true)
        {
            var res = await EventBusPublisher.CallEventBusAsync(
                ConstEventBus.Publisher_GetStockHolderDetailById, stockHolderId.ToString(), 
                ConstEventBus.EXCHANGE_MEMBER, isReply);
            var resData = ReturnData(res, isReply);
            if (resData.StatusCode != 200) return null;

            return JsonConvert.DeserializeObject<StockHolderDto>(resData.Data.ToString());
        }



        /// <summary>
        /// lấy thông tin stock
        /// </summary>
        /// <param name="stockId"></param>
        /// <param name="isReply"></param>
        /// <returns></returns>
        public static async Task<StockDto> GetStockDetailById(Guid stockId, bool isReply = true)
        {
            var res = await EventBusPublisher.CallEventBusAsync(
                ConstEventBus.Publisher_GetStockDetailById, stockId.ToString(),
                ConstEventBus.EXCHANGE_STOCKTRANS, isReply);
            var resData = ReturnData(res, isReply);
            if (resData.StatusCode != 200) return null;

            return JsonConvert.DeserializeObject<StockDto>(resData.Data.ToString());
        }
        


        /// <summary>
        /// lấy thông tin stock type
        /// </summary>
        /// <param name="stockTypeId"></param>
        /// <param name="isReply"></param>
        /// <returns></returns>
        public static async Task<StockTypeDto> GetStockTypeDetailOrDefault(Guid? stockTypeId = null, bool isReply = true)
        {
            var res = await EventBusPublisher.CallEventBusAsync(
                ConstEventBus.Publisher_GetStockTypeDetailById, stockTypeId.HasValue ? stockTypeId.ToString() : "",
                ConstEventBus.EXCHANGE_STOCKTRANS, isReply);
            var resData = ReturnData(res, isReply);
            if (resData.StatusCode != 200) return null;

            return JsonConvert.DeserializeObject<StockTypeDto>(resData.Data.ToString());
        }

        


        public static BaseResponse ReturnData(string res, bool isReply = true)
        {

            if (isReply)
            {
                if (string.IsNullOrEmpty(res)) return new BaseResponse() { StatusCode = 400 };
                return JsonConvert.DeserializeObject<BaseResponse>(res);
            }
            else
            {
                return new BaseResponse();
            }
        }

    }
}
