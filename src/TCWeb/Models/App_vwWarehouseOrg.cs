using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class App_vwWarehouseSubject
    {
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(255)]
        public string DocumentName { get; set; }
        [Required]
        [StringLength(255)]
        public string AccountName { get; set; }
        [Column(TypeName = "image")]
        public byte[] DocumentImage { get; set; }
        [Column(TypeName = "ntext")]
        public string DocumentDescription { get; set; }
        [Required]
        [StringLength(50)]
        public string InsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }
        [Required]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime UpdatedOn { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
    }
}
