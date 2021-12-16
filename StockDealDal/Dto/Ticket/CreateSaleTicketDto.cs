using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.Ticket
{
    public class CreateSaleTicketDto : CreateTicketDto, IValidatableObject
    {
        [MaxLength(50, ErrorMessage = "ERR_MAX_LENGTH_50")]
        public string Title { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public decimal? PriceFrom { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public decimal? PriceTo { get; set; }

        [Required(ErrorMessage = "ERR_REQUIRED")]
        public Guid? StockId { get; set; }

        [Required(ErrorMessage = "ERR_REQUIRED")]
        [MaxLength(255, ErrorMessage = "ERR_MAX_LENGTH_255")]
        public string StockCode { get; set; }

        public Guid? StockTypeId { get; set; }

        [MaxLength(255)]
        public string StockTypeName { get; set; }

        [Required(ErrorMessage = "ERR_REQUIRED")]
        [Range(0, int.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public int? Quantity { get; set; }

        public bool IsNegotiate { get; set; } = false;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsNegotiate)
            {
                if (!PriceFrom.HasValue)
                {
                    yield return new ValidationResult("ERR_REQUIRED", new[] { nameof(PriceFrom) });
                }
                else
                {
                    PriceTo = PriceFrom;
                    yield return ValidationResult.Success;
                }
            } else
            {
                PriceFrom = PriceTo = null;
            }
        }
    }
}
