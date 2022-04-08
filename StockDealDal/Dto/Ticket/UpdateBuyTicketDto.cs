using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.Ticket
{
    public class UpdateBuyTicketDto : UpdateTicketDto, IValidatableObject
    {
        //public UpdateBuyTicketDto()
        //{
        //    BuyTicketDetailDtos = new List<BuyTicketDetailDto>();
        //}

        [Required(ErrorMessage = "ERR_REQUIRED")]
        [MaxLength(50, ErrorMessage = "ERR_MAX_LENGTH_50")]
        public string Title { get; set; }

        [Required(ErrorMessage = "ERR_REQUIRED")]
        [MinLength(1, ErrorMessage = "ERR_REQUIRED")]
        public List<string> StockCode { get; set; }

        [Range((double)1, (double)99999999, ErrorMessage = "ERR_INVALID_VALUE")]
        public decimal? PriceFrom { get; set; }

        [Range((double)1, (double)99999999, ErrorMessage = "ERR_INVALID_VALUE")]
        public decimal? PriceTo { get; set; }

        [Range(1, (long)99999999, ErrorMessage = "ERR_INVALID_VALUE")]
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
            }
            else
            {
                PriceFrom = PriceTo = null;
            }
        }

        //public List<BuyTicketDetailDto> BuyTicketDetailDtos { get; set; }
    }
}
