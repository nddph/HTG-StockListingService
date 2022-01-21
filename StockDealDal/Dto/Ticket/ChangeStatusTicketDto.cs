using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.Ticket
{
    public class ChangeStatusTicketDto
    {
        public List<Guid> ListTicket { get; set; } = new();

        [Required(ErrorMessage = "ERR_REQUIRED")]
        [Range(0, 1, ErrorMessage = "ERR_INVALID_VALUE")]
        public int? Status { get; set; }

        [Required(ErrorMessage = "ERR_REQUIRED")]
        public bool? IsAll { get; set; }
    }
}
