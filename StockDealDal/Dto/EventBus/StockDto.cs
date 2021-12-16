using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.EventBus
{
    public class StockDto
    {
        public Guid Id { get; set; }
        public string StockCode { get; set; }
        public string StockName { get; set; }
        public int? Status { get; set; }
        public bool? IsPrivate { get; set; }
        public Guid? CompanyId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
