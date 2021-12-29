using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.StockDeal
{
    public class CreateStockDetailDto
    {
        /// <summary>
        /// 1: tin nhắn thường
        /// 2: tin nhắn đã tạo thương lượng
        /// </summary>
        [Range(1, 2, ErrorMessage = "ERR_INVALID_VALUE")]
        public int Type { get; set; } = 1;

        public string SenderName { get; set; }

        public string Description { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public decimal TotalPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public decimal UnitPrice { get; set; }
    }
}
