using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealCommon
{
    public enum TicketType
    {
        Buy = 1,
        Sale = 2
    }

    public enum TicketStatus
    {
        HideQuantity = 0,
        Show = 1,
        HideAdmin = 2
    }
}
