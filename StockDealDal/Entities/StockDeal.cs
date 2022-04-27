using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Entities
{
    [Table("ST_StockDeal")]
    public class StockDeal : BaseEntity
    {
        public StockDeal()
        {
            StockDealDetails = new List<StockDealDetail>();
        }

        [Key]
        [Required]
        public Guid Id { get; set; }

        public Guid? TicketId { get; set; }

        [ForeignKey(nameof(TicketId))]
        public Ticket Ticket { get; set; }

        [Required]
        public Guid SenderId { get; set; }

        public string SenderName { get; set; }

        [Required]
        public Guid ReceiverId { get; set; }

        public string ReceiverName { get; set; }

        public List<StockDealDetail> StockDealDetails { get; set; }

    }
}
