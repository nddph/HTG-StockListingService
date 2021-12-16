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
        /// </summary>
        public bool byNewer { get; set; } = true;

        /// <summary>
        /// lọc theo loại tin mua/bán
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
        /// lọc tin được đăng bởi người request
        /// </summary>
        public bool IsUser { get; set; } = false;

        [Range(-1, double.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public decimal PriceFrom { get; set; } = -1;

        [Range(-1, double.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public decimal PriceTo { get; set; } = -1;

        [Range(-1, double.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public int QuantityFrom { get; set; } = -1;

        [Range(-1, double.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public int QuantityTo { get; set; } = -1;

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
