using Microsoft.EntityFrameworkCore;
using StockDealDal.Dto.Ticket;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.StockDeal
{
    [Keyless]
    public class ViewListStockDeals
    {
        // deal
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }

        // ticket
        public Guid? TicketId { get; set; }
        public string TicketTitle { get; set; }
        public string TicketCode { get; set; }
        public string TicketStockCode { get; set; }
        public string TicketStockTypeName { get; set; }
        public decimal? TicketPrice { get; set; }
        public int? TicketQuantity{ get; set; }
        public bool? TicketIsNegotiate { get; set; }
        public Guid? TicketStockId { get; set; }
        public Guid? TicketStockTypeId { get; set; }
        public int? TicketType { get; set; }
        public DateTime? TicketDeletedDate { get; set; }

        // deal detail
        public Guid? LastStockDetailId { get; set; }
        public int? StockDetailType { get; set; }
        public bool? IsOnwerLastDealDetail { get; set; }
        
        /// <summary>
        /// 1 deal gửi, 2 deal nhận
        /// </summary>
        public int? ReceiverType { get; set; }
        public string Description { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalPrice { get; set; }
        public bool? IsDeletedDealDetail { get; set; }
        public DateTime? LastUpdate { get; set; }
        public int CountUnread { get; set; }
        public int TotalUnread { get; set; }
        public int TotalCount { get; set; }
        public int Type { get; set; }

        [NotMapped]
        public ViewTickets Ticket { get; set; }

    }
}
