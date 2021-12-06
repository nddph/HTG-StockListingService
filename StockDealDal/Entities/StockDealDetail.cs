using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Entities
{
    [Table("StockDealDetail")]
    public class StockDealDetail
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid SenderId { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? DeletedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        [Required]
        public Guid StockDetailId { get; set; }

        [ForeignKey(nameof(StockDetailId))]
        public StockDeal StockDeal { get; set; }

    }
}
