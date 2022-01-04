using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Entities
{
    [Table("ST_SaleTicket")]
    public class SaleTicket : Ticket
    {
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

        [NotMapped]
        public new int TicketType { get; set; } = 2;
    }
}
