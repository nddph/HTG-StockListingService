using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.Ticket
{
    public class CreateTicketDto
    {
        [MaxLength(300, ErrorMessage = "ERR_MAX_LENGTH_300")]
        public string Description { get; set; }

    }
}
