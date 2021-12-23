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
        /// <summary>
        /// gửi thông báo thương lượng
        /// </summary>
        /// <param name="dealNofifyDto"></param>
        /// <returns></returns>
        public static async Task<BaseResponse> SendDealNofify(SendDealNofifyDto dealNofifyDto)
        {

            var res = await EventBusPublisher.CallEventBusAsync(ConstEventBus.Publisher_SendDealNofify,
                        JsonConvert.SerializeObject(dealNofifyDto), ConstEventBus.EXCHANGE_NOTIFY, false);

            var resData = ReturnData(res, false);

            return resData;
        }



        /// <summary>
        /// Lấy giới hạn số lượng 1 loại cổ phiếu của 1 user
        /// </summary>
        /// <param name="stockHolderId"></param>
        /// <param name="stockId"></param>
        /// <param name="stockTypeId"></param>
        /// <returns></returns>
        public static async Task<int> GetStockHolderLimitAsync(Guid stockHolderId, Guid stockId, Guid stockTypeId)
        {
            var request = new GetStockQuantityRequest
            {
                StockHolderId = stockHolderId,
                StockId = stockId,
                StockTypeId = stockTypeId
            };

            var res = await EventBusPublisher.CallEventBusAsync(ConstEventBus.Publisher_GetStockAvailableQty,
                        JsonConvert.SerializeObject(request), ConstEventBus.EXCHANGE_STOCKTRANS, true);

            var resData = ReturnData(res, true);
            if (resData.StatusCode != 200) return 0;

            return int.Parse(resData.Data.ToString());
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
