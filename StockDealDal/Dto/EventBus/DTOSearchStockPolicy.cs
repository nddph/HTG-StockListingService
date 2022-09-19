using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.DTO
{
    public class DTOSearchStockPolicy
    {
        public DTOSearchStockPolicy()
        {
            GetOldEffectDate = 0;
        }

        public Guid? StockPolicyId { get; set; }
        public Guid? StockId { get; set; }
        public Guid? StockTypeId { get; set; }
        public string StockCode { get; set; }
        public int? StockTypeCode { get; set; }

        /// <summary>
        /// 0: Lấy các cấu hình hiện tại và tương lai
        /// 1: Lấy các cấu hình đã quá hạn, hiện tại và tương lai
        /// </summary>
        public int? GetOldEffectDate { get; set; }
        public DateTime? EffectDate { get; set; }

        public int? Active { get; set; }
    }
}
