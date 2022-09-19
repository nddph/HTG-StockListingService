using StockDealDal.Dto.Ticket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.StockDeal
{
    public class StockDealResponseDto
    {

        public StockDealResponseDto() { }
        public StockDealResponseDto(ViewListStockDeals viewListStockDeal, bool isDetail = false)
        {
            Id = viewListStockDeal.Id;
            SenderId = viewListStockDeal.SenderId;
            ReceiverId = viewListStockDeal.ReceiverId;
            SenderName = viewListStockDeal.SenderName;
            ReceiverName = viewListStockDeal.ReceiverName;
            ReceiverType = viewListStockDeal.ReceiverType;
            LastUpdate = viewListStockDeal.LastUpdate;
            CountUnread = viewListStockDeal.CountUnread;
            Type = viewListStockDeal.Type;
            Ticket = viewListStockDeal.Ticket;
            LastDealDetail = new()
            {
                LastStockDetailId = viewListStockDeal.LastStockDetailId,
                Description = viewListStockDeal.Description,
                Quantity = viewListStockDeal.Quantity,
                TotalPrice = viewListStockDeal.TotalPrice,
                UnitPrice = viewListStockDeal.UnitPrice,
                IsDeletedDealDetail = viewListStockDeal.IsDeletedDealDetail,
                IsOnwerLastDealDetail = viewListStockDeal.IsOnwerLastDealDetail,
                StockDetailType = viewListStockDeal.StockDetailType
            };

            if (isDetail)
            {
                Ticket.QuantityStatus = viewListStockDeal.Ticket != null ? viewListStockDeal.Ticket.QuantityStatus : 2;
                Ticket.IsExpTicket = viewListStockDeal.Ticket != null ? viewListStockDeal.Ticket.IsExpTicket : null;
                Ticket.Status = viewListStockDeal.Ticket != null ? viewListStockDeal.Ticket.Status : 1;
                Ticket.IsNegotiate = viewListStockDeal.Ticket != null ? viewListStockDeal.Ticket.IsNegotiate : true;
                Ticket.PriceFrom = viewListStockDeal.Ticket != null ? viewListStockDeal.Ticket.PriceFrom : null;
                Ticket.Quantity = viewListStockDeal.Ticket != null ? viewListStockDeal.Ticket.Quantity : null;
                Ticket.Reason = viewListStockDeal.Ticket != null ? viewListStockDeal.Ticket.Reason : "";
            }
        }

        // deal
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public int? ReceiverType { get; set; }

        public DateTime? LastUpdate { get; set; }
        public int CountUnread { get; set; }
        public int Type { get; set; }

        public ViewTickets Ticket { get; set; }
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
}
