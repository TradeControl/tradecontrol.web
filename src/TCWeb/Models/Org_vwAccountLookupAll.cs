using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("vwAccountLookupAll", Schema = "Org")]
    public partial class Org_vwAccountLookupAll
    {
        [Key]
        [Required]
        [StringLength(10)]
        [Display(Name = "Account Code")]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(255)]
        [Display(Name = "Name")]
        public string AccountName { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Type")]
        public string OrganisationType { get; set; }
        public short OrganisationTypeCode { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Status")]
        public string OrganisationStatus { get; set; }
        public short OrganisationStatusCode { get; set; }
        [StringLength(10)]
        [Display(Name = "Cash Mode")]
        public string CashMode { get; set; }
        public short CashModeCode { get; set; }
    }
}
