using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogConverter.DAL
{
    [Table("CRM_InventItemRange", Schema = "dbo")]
    class InventItemRange
    {
        [Key]
        public string RANGEID { get; set; }
        public string RANGEIDPARENT { get; set; }
        public string NAMEALIAS { get; set; }
        public int ITEMRANGEID_CRYSTALL { get; set; }
    }
}
