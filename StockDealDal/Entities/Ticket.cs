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
    public class Ticket : BaseEntity
    {
        [Key]
        [Required]
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

    }
}
