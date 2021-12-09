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
        public string SenderName { get; set; }

        public string Description { get; set; }

        [Range(0, ulong.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public ulong Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public decimal TotalPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public decimal UnitPrice { get; set; }
    }
}
