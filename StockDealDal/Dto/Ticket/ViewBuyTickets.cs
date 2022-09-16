using StockDealDal.Dto.StockDeal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.Ticket
{
    public class ViewBuyTickets
    {
        public Guid Id { get; set; }

        [MaxLength(255)]
        public string Code { get; set; }

        [MaxLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        [MaxLength(255)]
        public string FullName { get; set; }

        [MaxLength(255)]
        public string Email { get; set; }

        [MaxLength(50)]
        public string Phone { get; set; }

        public int Status { get; set; }

        public DateTime? ExpDate { get; set; }

        public string EmployeeCode { get; set; }

        public bool? IsNegotiate { get; set; } = false;

        public decimal? PriceFrom { get; set; }

        public decimal? PriceTo { get; set; }

        public int? Quantity { get; set; }

        public string Reason { get; set; }



        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? ModifiedBy { get; set; }
        public Guid? DeletedBy { get; set; }


        public string StockCodes { get; set; }

        public int TotalCount { get; set; }

        public string StockCodeView { get; set; }
        public int TicketType { get; set; }
        public bool? IsExpTicket { get; set; }

        [NotMapped]
        public List<StockDealResponseDto> StockDeals { get; set; } = new List<StockDealResponseDto>();
    }
}
