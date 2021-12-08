using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto
{
    public class CreateStockDealDto
    {
        [Required(ErrorMessage = "ERR_REQUIRED")]
        public Guid? SenderId { get; set; }

        public string SenderName { get; set; }

        [Required(ErrorMessage = "ERR_REQUIRED")]
        public Guid? ReceiverId { get; set; }

        public string ReceiverName { get; set; }

        public Guid? TickeId { get; set; }

        public CreateStockDetailDto StockDetail { get; set; }

    }
}
