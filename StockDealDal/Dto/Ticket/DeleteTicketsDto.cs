using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.Ticket
{
    public class DeleteTicketsDto
    {
        public List<Guid> ListTicket { get; set; } = new();
    }
}
