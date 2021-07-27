namespace CatalogConverter.Data
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [Serializable]
    public class TreeLeaf : TreeItem
    {
        [XmlElement("Barcode")]
        public List<TreeLeafBarcode> Barcodes { get; set; }

        [XmlElement("AdditionalInfo")]
        public List<LeafInfo> AdditionalInfo
        {
            set
            {
                if (this.Attributes == null)
                {
                    this.Attributes = new List<LeafInfo>();
                }

                this.Attributes.AddRange((IEnumerable<LeafInfo>)value);
            }
        }
    }
}
