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
            Ticket = new()
            {
                Id = viewListStockDeal.TicketId.GetValueOrDefault(),
                Code = viewListStockDeal.TicketCode,
                TicketType = viewListStockDeal.TicketType.GetValueOrDefault(),
                Title = viewListStockDeal.TicketTitle,
                IsNegotiate = viewListStockDeal.TicketIsNegotiate.GetValueOrDefault(),
                PriceFrom = viewListStockDeal.TicketPrice,
                Quantity = viewListStockDeal.TicketQuantity,
                StockTypeName = viewListStockDeal.TicketStockTypeName,
                StockCode = viewListStockDeal.TicketStockCode,
                StockId = viewListStockDeal.TicketStockId,
                StockTypeId = viewListStockDeal.TicketStockTypeId,
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
            DealDetailNotByUser = new()
            {
                LastStockDetailId = viewListStockDeal.NotByUserLastStockDetailId,
                Description = viewListStockDeal.NotByUserDescription,
                Quantity = viewListStockDeal.NotByUserQuantity,
                TotalPrice = viewListStockDeal.NotByUserTotalPrice,
                UnitPrice = viewListStockDeal.NotByUserUnitPrice,
                IsDeletedDealDetail = viewListStockDeal.NotByUserIsDeletedDealDetail,
                IsOnwerLastDealDetail = false,
                StockDetailType = viewListStockDeal.NotByUserStockDetailType
            };
            DealDetailByUser = new()
            {
                LastStockDetailId = viewListStockDeal.ByUserLastStockDetailId,
                Description = viewListStockDeal.ByUserDescription,
                Quantity = viewListStockDeal.ByUserQuantity,
                TotalPrice = viewListStockDeal.ByUserTotalPrice,
                UnitPrice = viewListStockDeal.ByUserUnitPrice,
                IsDeletedDealDetail = viewListStockDeal.ByUserIsDeletedDealDetail,
                IsOnwerLastDealDetail = true,
                StockDetailType = viewListStockDeal.ByUserStockDetailType
            };
            if (isDetail)
            {
                Ticket.QuantityStatus = viewListStockDeal.Ticket != null ? viewListStockDeal.Ticket.QuantityStatus : 2;
                Ticket.IsExpTicket = viewListStockDeal.Ticket != null ? viewListStockDeal.Ticket.IsExpTicket : null;
                Ticket.Status = viewListStockDeal.Ticket != null ? viewListStockDeal.Ticket.Status : 1;
                Ticket.IsNegotiate = viewListStockDeal.Ticket != null ? viewListStockDeal.Ticket.IsNegotiate : true;
                Ticket.AllowDeal = viewListStockDeal.Ticket != null ? viewListStockDeal.Ticket.AllowDeal : true;
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

        public TicketStockDealResponseDto Ticket { get; set; }
        public DealDetailStockDealResponseDto LastDealDetail { get; set; }
        public DealDetailStockDealResponseDto DealDetailNotByUser { get; set; }
        public DealDetailStockDealResponseDto DealDetailByUser { get; set; }

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

        public string Reason { get; set; }


        // sale ticket
        public Guid? StockId { get; set; }

        public Guid? StockTypeId { get; set; }

        public string StockCode { get; set; }

        public string StockTypeName { get; set; }

        public bool? IsNegotiate { get; set; }

        public bool? AllowDeal { get; set; }

        public decimal? PriceFrom { get; set; }

        public int? Quantity { get; set; }

        public int? QuantityStatus { get; set; }

        public DateTime? DeletedDate { get; set; }

    }

}
