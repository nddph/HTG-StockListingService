using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto
{
    public class BaseResponse
    {
        public int StatusCode { get; set; } = 200;
        public object Message { get; set; } = "";
        public object Data { get; set; }

    }
}
