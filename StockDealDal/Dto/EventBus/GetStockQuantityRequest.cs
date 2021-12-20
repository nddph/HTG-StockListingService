using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.EventBus
{
    public class GetStockQuantityRequest
    {
        public Guid StockHolderId { get; set; }
        public Guid StockId { get; set; }
        public Guid StockTypeId { get; set; }
    }
}
