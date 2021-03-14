﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbMirror", Schema = "Cash")]
    [Index(nameof(AccountCode), nameof(ChargeCode), Name = "IX_Cash_tbMirror_ChargeCode", IsUnique = true)]
    [Index(nameof(TransmitStatusCode), nameof(ChargeCode), Name = "IX_Cash_tbMirror_TransmitStatusCode")]
    public partial class Cash_tbMirror
    {
        [Key]
        [StringLength(50)]
        public string CashCode { get; set; }
        [Key]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Key]
        [StringLength(50)]
        public string ChargeCode { get; set; }
        public short TransmitStatusCode { get; set; }
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

        [ForeignKey(nameof(AccountCode))]
        [InverseProperty(nameof(Org_tbOrg.TbCashMirror))]
        public virtual Org_tbOrg AccountCodeNavigation { get; set; }
        [ForeignKey(nameof(CashCode))]
        [InverseProperty(nameof(Cash_tbCode.TbMirrors))]
        public virtual Cash_tbCode CashCodeNavigation { get; set; }
        [ForeignKey(nameof(TransmitStatusCode))]
        [InverseProperty(nameof(Org_tbTransmitStatus.TbCashMirrors))]
        public virtual Org_tbTransmitStatus TransmitStatusCodeNavigation { get; set; }
    }
}
