using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.Ticket
{
    public class ListTicketDto
    {
        /// <summary>
        /// sắp sếp theo tin đăng mới nhất hoặc cổ phiếu quan tâm
        /// </summary>
        public bool byNewer { get; set; } = true;

        /// <summary>
        /// lọc theo loại tin mua/bán
        /// </summary>
        [EnumDataType(typeof(TicketType), ErrorMessage = "ERR_INVALID_VALUE")]
        public int? TicketType { get; set; }

        /// <summary>
        /// lọc theo status
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// lọc theo danh sách mã cổ phiếu
        /// </summary>
        public List<string> StockCode { get; set; }

        /// <summary>
        /// lọc tin được đăng bởi người request
        /// </summary>
        public bool IsUser { get; set; } = false;

        [Range(0, double.MaxValue, ErrorMessage= "ERR_INVALID_VALUE")]
        public decimal? PriceFrom { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public decimal? PriceTo { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public int? QuantityFrom { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public int? QuantityTo { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public int CurrentPage { get; set; } = 1;

        [Range(1, int.MaxValue, ErrorMessage = "ERR_INVALID_VALUE")]
        public int PerPage { get; set; } = 20;
    }

    public enum TicketType
    {
        Buy = 0,
        Sale = 1
    }
}
