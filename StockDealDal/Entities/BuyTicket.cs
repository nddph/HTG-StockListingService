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
        public List<string> StockCode { get; set; }
    }
}
