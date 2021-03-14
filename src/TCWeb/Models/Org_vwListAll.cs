using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Org_vwListAll
    {
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(255)]
        public string AccountName { get; set; }
        [Required]
        [StringLength(50)]
        public string OrganisationType { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        public short CashModeCode { get; set; }
        public short OrganisationStatusCode { get; set; }
        [StringLength(50)]
        public string CashCode { get; set; }
    }
}
