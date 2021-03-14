using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbBucket", Schema = "App")]
    public partial class App_tbBucket
    {
        [Key]
        public short Period { get; set; }
        [Required]
        [StringLength(10)]
        public string BucketId { get; set; }
        [StringLength(50)]
        public string BucketDescription { get; set; }
        public bool AllowForecasts { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
    }
}
