using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.Ticket
{
    public class CreateTicketDto : IValidatableObject
    {
        [Required(ErrorMessage = "ERR_REQUIRED")]
        [MaxLength(100, ErrorMessage = "ERR_MAX_LENGTH_100")]
        public string Title { get; set; }

        [Range((double)1, (double)99999999, ErrorMessage = "ERR_INVALID_VALUE")]
        public decimal? PriceFrom { get; set; }

        [Range((double)1, (double)99999999, ErrorMessage = "ERR_INVALID_VALUE")]
        public decimal? PriceTo { get; set; }

        [Required(ErrorMessage = "ERR_REQUIRED")]
        public Guid? StockId { get; set; }

        [Required(ErrorMessage = "ERR_REQUIRED")]
        public Guid? StockTypeId { get; set; }

        [Required(ErrorMessage = "ERR_REQUIRED")]
        [Range(1, (long)99999999, ErrorMessage = "ERR_INVALID_VALUE")]
        public int? Quantity { get; set; }

        public bool IsNegotiate { get; set; } = false;

        [MaxLength(300, ErrorMessage = "ERR_MAX_LENGTH_300")]
        public string Description { get; set; }

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
            }
            else
            {
                PriceFrom = PriceTo = null;
            }
        }

    }
}
