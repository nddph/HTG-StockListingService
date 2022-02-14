using LogBusinessSharing.LogBusiness;
using LogBusinessSharing.LogCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealBusiness.EventBus
{
    public class EventBusHandleMessage
    {
        public EventBusHandleMessage()
        {
        }

        public async Task<byte[]> ResponseResult(string method, string message)
        {
            await LogManagerBusiness.LogDBAsync(LogActionType.Info, nameof(ResponseResult), nameof(EventBusHandleMessage), new { method, message }, null);

            var responseBytes = Array.Empty<byte>();
            switch (method)
            {
                default:
                    break;
            }
            return responseBytes;
        }
    }
}
