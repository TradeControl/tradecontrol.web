using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbEth", Schema = "App")]
    public partial class App_tbEth
    {
        [Key]
        [StringLength(200)]
        public string NetworkProvider { get; set; }
        [Required]
        [StringLength(42)]
        public string PublicKey { get; set; }
        [StringLength(64)]
        public string PrivateKey { get; set; }
        [StringLength(42)]
        public string ConsortiumAddress { get; set; }
    }
}
