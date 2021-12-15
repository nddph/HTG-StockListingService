using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.EventBus
{
    public class StockHolderDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public DateTime? DOB { get; set; }
        public int? Gender { get; set; }
        public string PersonalEmail { get; set; }
        public string WorkingEmail { get; set; }
        public string Phone { get; set; }
        public string CardTypeId { get; set; }
        public string CardNumber { get; set; }
        public DateTime? CardDate { get; set; }
        public string CardPlace { get; set; }
        public long? ProvinceId { get; set; }
        public string ProvinceName { get; set; }
        public int? DistrictId { get; set; }
        public string DistrictName { get; set; }
        public int? WardId { get; set; }
        public string WardName { get; set; }
        public string Address { get; set; }
        public string AddressLine { get; set; }
        public string Description { get; set; }
        public DateTime? DeletedDate { get; set; }
        public Guid? DeletedBy { get; set; }

        public Guid? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public int? Status { get; set; }
    }
}
