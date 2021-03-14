using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbChangeType", Schema = "Cash")]
    public partial class Cash_tbChangeType
    {
        public Cash_tbChangeType()
        {
            TbChanges = new HashSet<Cash_tbChange>();
        }

        [Key]
        public short ChangeTypeCode { get; set; }
        [Required]
        [StringLength(20)]
        public string ChangeType { get; set; }

        [InverseProperty(nameof(Cash_tbChange.ChangeTypeCodeNavigation))]
        public virtual ICollection<Cash_tbChange> TbChanges { get; set; }
    }
}
