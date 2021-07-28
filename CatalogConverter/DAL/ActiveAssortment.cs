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
    class ActiveAssortment
    {
        [Key]
        public string RATETABLERECID { get; set; }
        public string RECID { get; set; }
        public string ITEMID { get; set; }
        public decimal PURCMINPRICEPROD { get; set; }
        public decimal PURCMINPRICEVEND { get; set; }
        public decimal RETAILMINPRICE { get; set; }
    }
}
