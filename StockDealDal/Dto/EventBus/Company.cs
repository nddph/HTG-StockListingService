using System;
using System.Collections.Generic;

#nullable disable

namespace StockDealDal.DTO
{
    public partial class Company
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string TaxNumber { get; set; }
        public string LicenseNumber { get; set; }
        public DateTime? LicenseIssueDate { get; set; }
        public string LicenseIssueBy { get; set; }
        public string CompanyName { get; set; }
        public string ShortName { get; set; }
        public string Address { get; set; }
        public string PersonInCharge { get; set; }
        public string Position { get; set; }
        public decimal? CharterCapital { get; set; }
        public bool? IsHeadquarter { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public bool? VoucherTransaction { get; set; }
        public int? TransactionMultiple { get; set; }

        public DateTime? CreatedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
