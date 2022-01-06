using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealCommon
{
    public enum TypeStockDealDetail
    {
        // tin nhắn chi tiết thương lượng
        DealDetail = 1,

        // tin nhắn chờ phản hồi
        WaitingForResponse = 2,

        // tin nhắn tạo deal
        CreateDeal = 3
    }
}
