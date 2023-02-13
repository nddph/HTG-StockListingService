using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.DTO
{
    public class DTOStockPolicyResponse
    {
        public Guid Id { get; set; }
        public Guid? StockDetailId { get; set; }
        public Guid? StockId { get; set; }
        public string StockCode { get; set; }
        public Guid? StockTypeId { get; set; }
        public int? StockTypeCode { get; set; }
        public string StockTypeName { get; set; }
        public int? MinSaleTrans { get; set; }
        public int? MaxSaleTrans { get; set; }
        public long? MinPriceSale { get; set; }
        public long? MinPriceBuy { get; set; }
        public long? MaxPriceSale { get; set; }
        public long? MaxPriceBuy { get; set; }
        public DateTime? EffectDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public Guid? DeletedBy { get; set; }
        public int? TotalItems { get; set; }
        public int Active { get; set; }
    }
}
