using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.StockDeal
{
    public class StockDealSearchCriteria
    {
        public Guid? StockDealId { get; set; }

        public Guid? TicketId { get; set; }

        public Guid? LoginedContactId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public int CurrentPage { get; set; } = 1;

        [Range(1, int.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public int PerPage { get; set; } = 20;

        public bool IncludeEmptyDeal { get; set; } = false;

        /// <summary>
        /// 0: tất cả
        /// 1: người bán
        /// 2: người mua
        /// </summary>
        public int? Type { get; set; } = 0;
    }
}
