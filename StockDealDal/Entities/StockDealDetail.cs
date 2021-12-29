using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Entities
{
    [Table("ST_StockDealDetail")]
    public class StockDealDetail : BaseEntity
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        public int? Type { get; set; }

        public string SenderName { get; set; }

        public string Description { get; set; }

        [Required]
        public Guid StockDealId { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(StockDealId))]
        public StockDeal StockDeal { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public bool SenderRead { get; set; } = false;

        public bool ReceiverRead { get; set; } = false;

    }
}
