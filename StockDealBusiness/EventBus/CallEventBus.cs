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

        public static async Task<StockHolderDto> GetStockHolderDetail(Guid stockHolderId, bool isReply = true)
        {
            var res = await EventBusPublisher.CallEventBusAsync(
                ConstEventBus.Publisher_GetStockHolderDetailById, stockHolderId.ToString(), 
                ConstEventBus.EXCHANGE_MEMBER, isReply);
            var resData = ReturnData(res, isReply);
            if (resData.StatusCode != 200) return null;

            return JsonConvert.DeserializeObject<StockHolderDto>(resData.Data.ToString());
        }

        public static BaseResponse ReturnData(string res, bool isReply = true)
        {

            if (isReply)
            {
                return JsonConvert.DeserializeObject<BaseResponse>(res);
            }
            else
            {
                return new BaseResponse();
            }
        }

    }
}
