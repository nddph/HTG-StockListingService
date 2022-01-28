using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.EventBus
{
    public class SuggestTicketDto
    {
        public Guid TicketId { get; set; }
        public List<Guid> ListReceiverUser { get; set; }
        public Guid UserId { get; set; }
    }
}
