using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.Ticket
{
    public class UpdateTicketDto
    {
        [Required(ErrorMessage = "ERR_REQUIRED")]
        public Guid? Id { get; set; }

        [MaxLength(300, ErrorMessage = "ERR_MAX_LENGTH_300")]
        public string Description { get; set; }

    }
}
