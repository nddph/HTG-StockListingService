using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.StockDeal
{
    public class GetStockDealResponseDto
    {
        public Guid Id { get; set; }

        public Guid? TicketId { get; set; }

        public Entities.Ticket Ticket { get; set; }

        public Guid SenderId { get; set; }

        public string SenderName { get; set; }

        public Guid ReceiverId { get; set; }

        public string ReceiverName { get; set; }

        public int ReceiverType { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? ModifiedBy { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
