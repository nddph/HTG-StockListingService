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
        [EnumDataType(typeof(TicketType), ErrorMessage = "ERR_INVALID_VALUE")]
        public int? TicketType { get; set; }

        public int? Status { get; set; }

        public List<string> StockCode { get; set; }

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
        Buy = 1,
        Sale = 2
    }
}
