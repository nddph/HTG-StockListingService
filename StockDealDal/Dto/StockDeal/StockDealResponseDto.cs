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
            Ticket = new()
            {
                Id = viewListStockDeal.TicketId,
                Code = viewListStockDeal.TicketCode,
                TicketType = viewListStockDeal.TicketType,
                Title = viewListStockDeal.TicketTitle,
                IsNegotiate = viewListStockDeal.TicketIsNegotiate,
                PriceFrom = viewListStockDeal.TicketPrice,
                Quantity = viewListStockDeal.TicketQuantity,
                StockTypeName = viewListStockDeal.TicketStockTypeName,
                StockCode = viewListStockDeal.TicketStockCode,
                StockId = viewListStockDeal.TicketStockId,
                StockTypeId = viewListStockDeal.TicketStockTypeId,
                StockCodes = viewListStockDeal.TicketStockCodes,
                DeletedDate = viewListStockDeal.TicketDeletedDate
            };
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
                Ticket.QuantityStatus = viewListStockDeal.SaleTicket == null ? null : viewListStockDeal.SaleTicket.QuantityStatus;
                Ticket.IsExpTicket = viewListStockDeal.SaleTicket != null ? viewListStockDeal.SaleTicket.IsExpTicket : viewListStockDeal.BuyTicket != null ? viewListStockDeal.BuyTicket.IsExpTicket : null;
                Ticket.Status = viewListStockDeal.SaleTicket != null ? viewListStockDeal.SaleTicket.Status : viewListStockDeal.BuyTicket != null ? viewListStockDeal.BuyTicket.Status : null;
                Ticket.IsNegotiate = viewListStockDeal.SaleTicket != null ? viewListStockDeal.SaleTicket.IsNegotiate : viewListStockDeal.BuyTicket != null ? viewListStockDeal.BuyTicket.IsNegotiate : null;
                Ticket.PriceFrom = viewListStockDeal.SaleTicket != null ? viewListStockDeal.SaleTicket.PriceFrom : viewListStockDeal.BuyTicket != null ? viewListStockDeal.BuyTicket.PriceFrom : null;
                Ticket.Quantity = viewListStockDeal.SaleTicket != null ? viewListStockDeal.SaleTicket.Quantity : viewListStockDeal.BuyTicket != null ? viewListStockDeal.BuyTicket.Quantity : null;
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

        public bool? IsExpTicket { get; set; }

        public int? Status { get; set; }


        // sale ticket
        public Guid? StockId { get; set; }

        public Guid? StockTypeId { get; set; }

        public string StockCode { get; set; }

        public string StockTypeName { get; set; }

        public bool? IsNegotiate { get; set; }

        public decimal? PriceFrom { get; set; }

        public int? Quantity { get; set; }

        public int? QuantityStatus { get; set; }

        // buy ticket
        public string StockCodes { get; set; }

        public DateTime? DeletedDate { get; set; }

        public string StockCodeView
        {
            get
            {
                if (TicketType == 1) return StockCodes.Replace(",", ", ");
                return StockCode;
            }
        }

    }
}
