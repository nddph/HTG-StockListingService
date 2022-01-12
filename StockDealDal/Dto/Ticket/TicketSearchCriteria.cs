using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.Ticket
{
    public class TicketSearchCriteria
    {
        /// <summary>
        /// sắp sếp theo tin đăng mới nhất hoặc cổ phiếu quan tâm
        /// 1: theo mới nhất
        /// 0: theo cổ phiếu quan tâm
        /// </summary>
        public bool byNewer { get; set; } = true;

        /// <summary>
        /// lọc theo loại tin mua/bán
        /// 1: mua
        /// 2: bán
        /// </summary>
        [EnumDataType(typeof(TicketType), ErrorMessage = "ERR_INVALID_VALUE")]
        public int TicketType { get; set; } = 1;

        /// <summary>
        /// lọc theo status
        /// </summary>
        public int Status { get; set; } = -1;

        /// <summary>
        /// lọc theo danh sách mã cổ phiếu
        /// </summary>
        public List<string> StockCode { get; set; } = new();

        /// <summary>
        /// lọc tin theo người đăng
        /// 0: tất cả tin
        /// 1: tin được đăng bởi người request
        /// 2: tin không được đăng bởi người request
        /// </summary>
        public int byUserType { get; set; } = 0;

        [Range(-1, double.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public decimal PriceFrom { get; set; } = -1;

        [Range(-1, double.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public decimal PriceTo { get; set; } = -1;

        [Range(-1, double.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public int QuantityFrom { get; set; } = -1;

        [Range(-1, double.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public int QuantityTo { get; set; } = -1;

        /// <summary>
        /// -1: tất cả tin
        /// 1: tin chưa hết hạn
        /// 2: tin hết hạn
        /// </summary>
        [Range(-1, 2, ErrorMessage = "ERR_INVALID_VALUE")]
        public int ExpTicketStatus { get; set; } = -1;

        /// <summary>
        /// true: Bao gồm ticket đã xóa
        /// false: không bao gồm ticket đã xóa
        /// </summary>
        public bool IncludeDelTicket { get; set; } = false;

        [Range(1, int.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public int CurrentPage { get; set; } = 1;

        [Range(1, int.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public int PerPage { get; set; } = 20;
        
    }

    public enum TicketType
    {
        Buy = 1,
        Sale = 2
    }
}
