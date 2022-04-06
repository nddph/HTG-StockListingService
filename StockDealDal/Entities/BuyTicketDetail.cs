using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Entities
{
    [Table("ST_BuyTicketDetail")]
    public class BuyTicketDetail : BaseEntity
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid BuyTicketId { get; set; }

        public Guid? StockId { get; set; }

        public Guid? StockTypeId { get; set; }

        [MaxLength(255)]
        public string StockCode { get; set; }

        [MaxLength(255)]
        public string StockTypeName { get; set; }

        public bool IsNegotiate { get; set; } = false;

        public decimal? PriceFrom { get; set; }

        public decimal? PriceTo { get; set; }

        public int? Quantity { get; set; }

        [ForeignKey(nameof(BuyTicketId))]
        public BuyTicket BuyTicket { get; set; }
    }
}
