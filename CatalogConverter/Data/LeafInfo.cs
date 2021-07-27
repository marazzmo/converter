namespace CatalogConverter.Data
{
    using System;
    using System.Xml.Serialization;

    [Serializable]
    public class LeafInfo
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Value")]
        public string Value { get; set; }
    }
}
