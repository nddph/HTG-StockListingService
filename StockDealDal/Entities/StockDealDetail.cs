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
    public class StockDealDetail : IEntity
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        public string SenderName { get; set; }

        public string Description { get; set; }

        [Required]
        public Guid StockDetailId { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(StockDetailId))]
        public StockDeal StockDeal { get; set; }

        public ulong Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }

    }
}
