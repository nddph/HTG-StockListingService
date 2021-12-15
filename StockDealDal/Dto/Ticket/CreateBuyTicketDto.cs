using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.Ticket
{
    public class CreateBuyTicketDto : CreateTicketDto
    {
        [Required(ErrorMessage = "ERR_REQUIRED")]
        [MaxLength(50, ErrorMessage = "ERR_MAX_LENGTH_50")]
        public string Title { get; set; }

        [Required(ErrorMessage = "ERR_REQUIRED")]
        [MinLength(1, ErrorMessage = "ERR_REQUIRED")]
        public List<string> StockCode { get; set; }
    }
}
