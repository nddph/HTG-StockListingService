using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Entities
{
    [Table("ST_BuyTicket")]
    public class BuyTicket : Ticket
    {
        public string StockCodes { get; set; }

        [NotMapped]
        public List<string> StockCode { get => StockCodes.Split(",").ToList(); }
    }
}
