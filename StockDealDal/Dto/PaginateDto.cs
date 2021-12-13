using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto
{
    public class PaginateDto
    {
        public int TotalItems { get; set; } = 0;
        public int TotalPages
        {
            get
            {
                if (PerPage == 0) return 0;
                return (TotalItems / PerPage) + ((TotalItems % PerPage == 0) ? 0 : 1);
            }
        }
        public int CurrentPage { get; set; } = 1;
        public int PerPage { get; set; } = 20;
        public object Data { get; set; }

    }
}
