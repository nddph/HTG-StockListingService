using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.EventBus
{
    public class AdminHiddenTicketDto
    {
        public Guid UserId { get; set; }
        public List<Guid> TicketId { get; set; } = new List<Guid>();

    }
}
