using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Org_vwTypeLookup
    {
        [Display(Name = "Org Type Code")]
        public short OrganisationTypeCode { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name ="Type")]
        public string OrganisationType { get; set; }
        [StringLength(10)]
        [Display(Name = "Mode")]
        public string CashMode { get; set; }
    }
}
