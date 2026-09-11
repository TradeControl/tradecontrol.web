using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbAddressType", Schema = "Subject")]
    public partial class Subject_tbAddressType
    {
        public Subject_tbAddressType() => TbAddresses = new HashSet<Subject_tbAddress>();

        [Key]
        public short AddressTypeCode { get; set; }

        [Required]
        [StringLength(50)]
        public string AddressType { get; set; }

        public virtual ICollection<Subject_tbAddress> TbAddresses { get; set; }
    }
}
