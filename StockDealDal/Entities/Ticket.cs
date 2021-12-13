using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Entities
{
    [Table("ST_Ticket")]
    public class Ticket : Ientity
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [MaxLength(255)]
        public string Code { get; set; }

        [MaxLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime? PublishDate { get; set; }

        public decimal? PriceFrom { get; set; }

        public decimal? PriceTo { get; set; }

        public int? Quantity { get; set; }

        [MaxLength(255)]
        public string FullName { get; set; }

        [MaxLength(255)]
        public string Email { get; set; }

        [MaxLength(50)]
        public string Phone { get; set; }

        public int Status { get; set; }

        public DateTime? DueDate { get; set; }

        public Guid TicketTypeId { get; set; }

        [ForeignKey(nameof(TicketTypeId))]
        public TicketType TicketType { get; set; }

    }
}
