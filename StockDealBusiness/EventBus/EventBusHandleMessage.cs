using Newtonsoft.Json;
using StockDealBusiness.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealBusiness.EventBus
{
    public class EventBusHandleMessage
    {
        private readonly StockDealEventBusBusiness _stockDealEventBusBusiness;
        public EventBusHandleMessage()
        {
            _stockDealEventBusBusiness = new();
        }

        public async Task<byte[]> ResponseResult(string method, string message)
        {

            var responseBytes = Array.Empty<byte>();

            switch (method)
            {
                case ConstEventBus.Method_CountUnreadDeal:
                    {
                        var userId = JsonConvert.DeserializeObject<Guid>(message);
                        var res = await _stockDealEventBusBusiness.CountUnreadDealAsync(userId);
                        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(res));
                    }
                default:
                    break;
            }
            return responseBytes;
        }
    }
}
