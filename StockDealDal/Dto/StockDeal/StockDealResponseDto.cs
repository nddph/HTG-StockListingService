using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.StockDeal
{
    public class StockDealResponseDto
    {
        // deal
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public int? ReceiverType { get; set; }

        public DateTime? LastUpdate { get; set; }
        public int CountUnread { get; set; }

        public TicketStockDealResponseDto Ticket { get; set; }
        public DealDetailStockDealResponseDto LastDealDetail { get; set; }

    }

    public class DealDetailStockDealResponseDto
    {
        // deal detail
        public Guid? LastStockDetailId { get; set; }
        public int? StockDetailType { get; set; }
        public bool? IsOnwerLastDealDetail { get; set; }

        /// <summary>
        /// 1 deal gửi, 2 deal nhận
        /// </summary>
        public string Description { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalPrice { get; set; }
        public bool? IsDeletedDealDetail { get; set; }
    }

    public class TicketStockDealResponseDto
    {
        public Guid? Id { get; set; }

        public string Code { get; set; }

        public string Title { get; set; }

        public int? TicketType { get; set; }


        // sale ticket

        public string StockCode { get; set; }

        public string StockTypeName { get; set; }

        public bool? IsNegotiate { get; set; }

        public decimal? PriceFrom { get; set; }

        public int? Quantity { get; set; }

        // buy ticket
        public string StockCodes { get; set; }

    }
}
