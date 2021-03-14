using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class App_vwIdentity
    {
        [Required]
        [StringLength(255)]
        public string AccountName { get; set; }
        [Required]
        [Column(TypeName = "ntext")]
        public string Address { get; set; }
        [StringLength(50)]
        public string PhoneNumber { get; set; }
        [StringLength(255)]
        public string EmailAddress { get; set; }
        [StringLength(255)]
        public string WebSite { get; set; }
        [Column(TypeName = "image")]
        public byte[] Logo { get; set; }
        [Required]
        [StringLength(50)]
        public string UserName { get; set; }
        [Required]
        [StringLength(50)]
        public string LogonName { get; set; }
        [Column(TypeName = "image")]
        public byte[] Avatar { get; set; }
        [StringLength(20)]
        public string CompanyNumber { get; set; }
        [StringLength(50)]
        public string VatNumber { get; set; }
        [Required]
        [StringLength(100)]
        public string UocName { get; set; }
        [Required]
        [StringLength(10)]
        public string UocSymbol { get; set; }
    }
}
