using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogConverter.DAL
{
    [Table("CRM_InventTable", Schema = "dbo")]
    class InventTable
    {
        [Key]
        public string ITEMID { get; set; }
        public string ITEMNAME { get; set; }
        public string ITEMRANGEID { get; set; }
    }
}
