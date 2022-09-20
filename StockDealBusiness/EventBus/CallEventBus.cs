using Newtonsoft.Json;
using StockDealDal.Dto;
using StockDealDal.Dto.EventBus;
using StockDealDal.DTO;
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
        /// Gửi thông báo đề xuất tin đăng phù hợp
        /// </summary>
        /// <param name="suggestTicketDto"></param>
        /// <param name="isReply"></param>
        /// <returns></returns>
        public static async Task<BaseResponse> NotificationSuggestTicketAsync(SuggestTicketDto suggestTicketDto, bool isReply = false)
        {
            var res = await EventBusPublisher.CallEventBusAsync(ConstEventBus.Publisher_NotificationSuggestTicket,
                        JsonConvert.SerializeObject(suggestTicketDto), ConstEventBus.EXCHANGE_NOTIFY, isReply);

            var resData = ReturnData(res, isReply);

            return resData;
        }



        /// <summary>
        /// Gửi thông báo cho user về tin hết hạn
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="isReply"></param>
        /// <returns></returns>
        public static async Task<BaseResponse> NotificationExpiredTicketAsync(Guid userId, bool isReply = false)
        {
            var res = await EventBusPublisher.CallEventBusAsync(ConstEventBus.Publisher_NotificationExpiredTicket,
                        JsonConvert.SerializeObject(userId), ConstEventBus.EXCHANGE_NOTIFY, isReply);

            var resData = ReturnData(res, isReply);

            return resData;
        }


        /// <summary>
        /// Gửi thông báo cho user về tin bị admin ẩn
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="isReply"></param>
        /// <returns></returns>
        public static async Task<BaseResponse> NotificationAdminHiddenTicketAsync(List<AdminHiddenTicketDto> adminHiddenTickets, bool isReply = false)
        {
            var res = await EventBusPublisher.CallEventBusAsync(ConstEventBus.Publisher_NotificationAdminHiddenTicket,
                        JsonConvert.SerializeObject(adminHiddenTickets), ConstEventBus.EXCHANGE_NOTIFY, isReply);

            var resData = ReturnData(res, isReply);

            return resData;
        }


        /// <summary>
        /// gửi thông báo thương lượng
        /// </summary>
        /// <param name="dealNofifyDto"></param>
        /// <returns></returns>
        public static async Task<BaseResponse> SendDealNofify(SendDealNofifyDto dealNofifyDto, bool isReply = false)
        {

            var res = await EventBusPublisher.CallEventBusAsync(ConstEventBus.Publisher_SendDealNofify,
                        JsonConvert.SerializeObject(dealNofifyDto), ConstEventBus.EXCHANGE_NOTIFY, isReply);

            var resData = ReturnData(res, isReply);

            return resData;
        }



        /// <summary>
        /// Lấy giới hạn số lượng 1 loại cổ phiếu của 1 user
        /// </summary>
        /// <param name="stockHolderId"></param>
        /// <param name="stockId"></param>
        /// <param name="stockTypeId"></param>
        /// <returns></returns>
        public static async Task<int> GetStockHolderLimitAsync(Guid stockHolderId, Guid stockId, Guid stockTypeId, bool isReply = true)
        {
            var request = new GetStockQuantityRequest
            {
                StockHolderId = stockHolderId,
                StockId = stockId,
                StockTypeId = stockTypeId
            };

            var res = await EventBusPublisher.CallEventBusAsync(ConstEventBus.Publisher_GetStockAvailableQty,
                        JsonConvert.SerializeObject(request), ConstEventBus.EXCHANGE_STOCKTRANS, isReply);

            var resData = ReturnData(res, isReply);
            if (resData.StatusCode != 200 || (resData.StatusCode == 200 && resData.Data == null)) return 0;

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
            if (resData.StatusCode != 200 || (resData.StatusCode == 200 && resData.Data == null)) return null;

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
            if (resData.StatusCode != 200 || (resData.StatusCode == 200 && resData.Data == null)) return null;

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
            if (resData.StatusCode != 200 || (resData.StatusCode == 200 && resData.Data == null)) return null;

            return JsonConvert.DeserializeObject<StockTypeDto>(resData.Data.ToString());
        }


        /// <summary>
        /// lấy thông tin hạn mức giao dịch
        /// </summary>
        /// <returns></returns>
        public static async Task<List<DTOStockPolicyResponse>> GetStockPolicyList(Guid stockId, Guid stockTypeId, bool isReply = true)
        {
            var request = new DTOSearchStockPolicy()
            {
                StockId = stockId,
                StockTypeId = stockTypeId,
                GetOldEffectDate = 0
            };
            string bodyString = JsonConvert.SerializeObject(request);
            var res = await EventBusPublisher.CallEventBusAsync(ConstEventBus.Publisher_GetStockPolicyList, bodyString, ConstEventBus.EXCHANGE_STOCKTRANS, isReply);
            var resData = ReturnData(res, isReply);
            if (resData.StatusCode != 200 || (resData.StatusCode == 200 && resData.Data == null)) return null;

            return JsonConvert.DeserializeObject<List<DTOStockPolicyResponse>>(resData.Data.ToString());
        }

        /// <summary>
        /// Lấy danh sách các mã CP trong hệ thống
        /// </summary>
        /// <param name="isReply"></param>
        /// <returns></returns>
        public static async Task<List<StockDto>> GetStockList(bool isReply = true)
        {
            var res = await EventBusPublisher.CallEventBusAsync(
                ConstEventBus.Publisher_GetStockList, "",
                ConstEventBus.EXCHANGE_STOCKTRANS, isReply);
            var resData = ReturnData(res, isReply);
            if (resData.StatusCode != 200 || (resData.StatusCode == 200 && resData.Data == null)) return null;

            return JsonConvert.DeserializeObject<List<StockDto>>(resData.Data.ToString());
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
