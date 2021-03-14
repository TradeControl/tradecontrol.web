using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Org_vwCompanyHeader
    {
        [Required]
        [StringLength(255)]
        public string CompanyName { get; set; }
        [Column(TypeName = "ntext")]
        public string CompanyAddress { get; set; }
        [StringLength(50)]
        public string CompanyPhoneNumber { get; set; }
        [StringLength(255)]
        public string CompanyEmailAddress { get; set; }
        [StringLength(255)]
        public string CompanyWebsite { get; set; }
        [StringLength(20)]
        public string CompanyNumber { get; set; }
        [StringLength(50)]
        public string VatNumber { get; set; }
    }
}
