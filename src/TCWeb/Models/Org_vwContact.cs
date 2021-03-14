using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Org_vwContact
    {
        [Required]
        [StringLength(100)]
        public string ContactName { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        public int Tasks { get; set; }
        [StringLength(50)]
        public string PhoneNumber { get; set; }
        [StringLength(50)]
        public string HomeNumber { get; set; }
        [StringLength(50)]
        public string MobileNumber { get; set; }
        [StringLength(255)]
        public string EmailAddress { get; set; }
        [Required]
        [StringLength(255)]
        public string AccountName { get; set; }
        [Required]
        [StringLength(50)]
        public string OrganisationType { get; set; }
        [StringLength(255)]
        public string OrganisationStatus { get; set; }
        [StringLength(25)]
        public string NameTitle { get; set; }
        [StringLength(100)]
        public string NickName { get; set; }
        [StringLength(100)]
        public string JobTitle { get; set; }
        [StringLength(50)]
        public string Department { get; set; }
    }
}
