using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto
{
    public class CreateStockDetailDto
    {
        [Required(ErrorMessage = "ERR_REQUIRED")]
        public Guid? ReceiverId { get; set; }

        public Guid? TickeId { get; set; }

        [Range(0, ulong.MaxValue, ErrorMessage= "ERR_INVALID_VALUE")]
        public ulong StockQuantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public decimal NegotiatePrice { get; set; }
    }
}
