namespace CatalogConverter.Data
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [Serializable]
    public class TreeItem
    {
        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlArray("Attributes")]
        [XmlArrayItem(typeof(LeafInfo), ElementName = "Attribute")]
        public List<LeafInfo> Attributes { get; set; }
    }
}
