using StockDealCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.StockDeal
{
    public class CreateStockDetailDto : IValidatableObject
    {
        /// <summary>
        /// 1: tin nhắn thường
        /// 2: tin nhắn chờ phản hồi
        /// 3: tin nhắn tạo thương lượng
        /// </summary>

        [EnumDataType(typeof(TypeStockDealDetail), ErrorMessage = "ERR_INVALID_VALUE")]
        public int Type { get; set; } = 1;

        public string SenderName { get; set; }

        public string Description { get; set; }

        [Range(0, (long)999999999, ErrorMessage = "ERR_INVALID_VALUE")]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public decimal TotalPrice { get; set; }

        [Range((double)0, (double)999999999, ErrorMessage = "ERR_INVALID_VALUE")]
        public decimal UnitPrice { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Type == ((int)TypeStockDealDetail.DealDetail))
            {
                if (Quantity <= 0) yield return new ValidationResult("ERR_INVALID_VALUE", new[] { nameof(Quantity) });
                if (TotalPrice <= 0) yield return new ValidationResult("ERR_INVALID_VALUE", new[] { nameof(TotalPrice) });
                if (UnitPrice <= 0) yield return new ValidationResult("ERR_INVALID_VALUE", new[] { nameof(UnitPrice) });
            }
        }
    }
}
