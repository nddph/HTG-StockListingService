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
        /// tìm kiếm theo tiêu đề, mã cổ phiếu, loại cổ phiếu
        /// </summary>
        public string SearchText { get; set; }


        /// <summary>
        /// sắp sếp theo tin đăng mới nhất hoặc cổ phiếu quan tâm
        /// true: theo mới nhất
        /// false: theo cổ phiếu quan tâm
        /// </summary>
        public bool ByNewer { get; set; } = true;

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
        [Range(-1, 1, ErrorMessage = "ERR_INVALID_VALUE")]
        public int Status { get; set; } = -1;

        /// <summary>
        /// lọc theo danh sách mã cổ phiếu
        /// </summary>
        public List<string> StockCodes { get; set; } = new();

        /// <summary>
        /// lọc theo danh sách loại cổ phiếu
        /// </summary>
        public List<string> StockTypeIds { get; set; } = new();

        /// <summary>
        /// lọc tin theo người đăng
        /// -1: tất cả tin
        /// 1: tin được đăng bởi người request
        /// 2: tin không được đăng bởi người request
        /// </summary>
        public int ByUserType { get; set; } = -1;

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
        /// -1: tất cả tin
        /// 1: lấy tin chưa xóa
        /// 2: lấy tin đã xóa
        /// </summary>
        [Range(-1, 2, ErrorMessage = "ERR_INVALID_VALUE")]
        public int DelTicketStatus { get; set; } = 1;

        /// <summary>
        /// -1: tất cả tin
        /// 1: tin có số lượng cổ phiếu khả dụng không đủ để bán
        /// 2: tin có số lượng cổ phiếu khả dụng đủ để bán
        /// </summary>
        [Range(-1, 2, ErrorMessage = "ERR_INVALID_VALUE")]
        public int QuantityStatus { get; set; } = 2;


        /// <summary>
        /// lấy tin có số lượng cổ phiếu khả dụng không đủ để bán
        /// lấy tin hết hạn
        /// lấy tin có status = 0 (tắt)
        /// </summary>
        public bool? IsHidden { get; set; } = false;

        /// <summary>
        /// 0: không lọc, sắp sếp theo thời gian mới nhất
        /// 1: lọc và sắp sếp theo giá thấp nhất
        /// 2: lọc và sắp sếp theo giá cao nhất
        /// 3: lọc và sắp sếp theo thương lượng
        /// </summary>
        [Range(0, 3, ErrorMessage = "ERR_INVALID_VALUE")]
        public int OrderByPriceType { get; set; } = 0;

        [Range(1, int.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public int CurrentPage { get; set; } = 1;

        [Range(1, int.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public int PerPage { get; set; } = 20;

        public bool IsPaging { get; set; } = true;
    }

    public enum TicketType
    {
        Buy = 1,
        Sale = 2
    }
}
