using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Entities
{
    [Table("StockDeal")]
    public class StockDeal
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        public Guid? TickeId { get; set; }

        [Required]
        public Guid SenderId { get; set; }

        [Required]
        public Guid ReceiverId { get; set; }

        public ICollection<StockDealDetail> StockDealDetails { get; set; }

        [Required]
        public DateTime? CreatedDate { get; set; }
    }
}
