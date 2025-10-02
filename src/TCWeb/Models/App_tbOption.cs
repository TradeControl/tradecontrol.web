using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbOptions", Schema = "App")]
    public partial class App_tbOption
    {
        [Key]
        [StringLength(4)]
        public string Identifier { get; set; }
        public bool IsInitialised { get; set; }
        [Required]
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [Required]
        [StringLength(50)]
        public string RegisterName { get; set; }
        public short DefaultPrintMode { get; set; }
        public short BucketTypeCode { get; set; }
        public short BucketIntervalCode { get; set; }
        [StringLength(10)]
        public string NetProfitCode { get; set; }
        [StringLength(10)]
        public string VatCategoryCode { get; set; }
        public short TaxHorizon { get; set; }
        public bool IsAutoOffsetDays { get; set; }
        public string UnitOfCharge { get; set; }
        [StringLength(50)]
        public string MinerFeeCode { get; set; }
        [StringLength(10)]
        public string MinerSubjectCode { get; set; }
        public short CoinTypeCode { get; set; }
        public int? HostId { get; set; }
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
        [StringLength(5)]
        [ForeignKey(nameof(SubjectCode))]
        [InverseProperty(nameof(Subject_tbSubject.TbOptionSubjectCodeNavigations))]
        public virtual Subject_tbSubject SubjectCodeNavigation { get; set; }
        [ForeignKey(nameof(BucketIntervalCode))]
        [InverseProperty(nameof(App_tbBucketInterval.TbOptions))]
        public virtual App_tbBucketInterval BucketIntervalCodeNavigation { get; set; }
        [ForeignKey(nameof(BucketTypeCode))]
        [InverseProperty(nameof(App_tbBucketType.TbOptions))]
        public virtual App_tbBucketType BucketTypeCodeNavigation { get; set; }
        [ForeignKey(nameof(CoinTypeCode))]
        [InverseProperty(nameof(Cash_tbCoinType.TbOptions))]
        public virtual Cash_tbCoinType CoinTypeCodeNavigation { get; set; }
        [ForeignKey(nameof(MinerSubjectCode))]
        [InverseProperty(nameof(Subject_tbSubject.TbOptionMinerSubjectCodeNavigations))]
        public virtual Subject_tbSubject MinerSubjectCodeNavigation { get; set; }
        [ForeignKey(nameof(MinerFeeCode))]
        [InverseProperty(nameof(Cash_tbCode.TbOptions))]
        public virtual Cash_tbCode MinerFeeCodeNavigation { get; set; }
        [ForeignKey(nameof(NetProfitCode))]
        [InverseProperty(nameof(Cash_tbCategory.TbOptions))]
        public virtual Cash_tbCategory NetProfitCodeNavigation { get; set; }
        [ForeignKey(nameof(RegisterName))]
        [InverseProperty(nameof(App_tbRegister.TbOptions))]
        public virtual App_tbRegister RegisterNameNavigation { get; set; }
        [ForeignKey(nameof(UnitOfCharge))]
        [InverseProperty(nameof(App_tbUoc.TbOptions))]
        public virtual App_tbUoc UnitOfChargeNavigation { get; set; }
        [ForeignKey(nameof(HostId))]
        [InverseProperty(nameof(App_tbHost.TbOptions))]
        public virtual App_tbHost HostIdNavigation { get; set; }
    }
}
