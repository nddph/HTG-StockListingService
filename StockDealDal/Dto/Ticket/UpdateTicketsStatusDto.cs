using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.Ticket
{
    public class UpdateTicketsStatusDto
    {
        public UpdateTicketsStatusDto()
        {
            ticketIds = new List<Guid>();
            IsDelete = false;
            Status = 0;
        }

        [Required(ErrorMessage = "ticketIds_ERR_REQUIRED")]
        [MinLength(1, ErrorMessage = "ticketIds_ERR_REQUIRED")]
        public List<Guid> ticketIds { get; set; } = new();

        public int Status { get; set; }

        public bool IsDelete { get; set; }

        public string Reason { get; set; }
    }
}
