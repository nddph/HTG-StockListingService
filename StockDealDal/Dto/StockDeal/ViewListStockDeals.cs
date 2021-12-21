using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.StockDeal
{
    [Keyless]
    public class ViewListStockDeals
    {
        public Guid Id { get; set; }
        public Guid? TicketId { get; set; }
        public Guid? LastStockDetailId { get; set; }
        public string TicketCode { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public bool IsOnwerLastDealDetail { get; set; }
        
        /// <summary>
        /// 1 mua, 2 ban
        /// </summary>
        public int ReceiverType { get; set; }
        public string Description { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalPrice { get; set; }
        public int CountUnread { get; set; }
        public DateTime? LastUpdate { get; set; }
        public int TotalCount { get; set; }
        public int TotalUnread { get; set; }

    }
}
