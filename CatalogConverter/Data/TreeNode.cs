namespace CatalogConverter.Data
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [Serializable]
    public class TreeNode : TreeItem
    {
        private bool? isDiff;

        [XmlAttribute("IsDiff")]
        public string IsDiffAttribute
        {
            get
            {
                return !this.IsDiff.HasValue ? (string)null : this.IsDiff.ToString();
            }

            set
            {
                if (value == null)
                {
                    this.isDiff = new bool?();
                }
                else
                {
                    bool result;
                    if (bool.TryParse(value, out result))
                    {
                        this.IsDiff = new bool?(result);
                    }
                }
            }
        }

        [XmlArray("Nodes")]
        [XmlArrayItem(typeof(TreeNode), ElementName = "TreeNode")]
        public List<TreeNode> Nodes { get; set; }

        [XmlArray("Leafs")]
        [XmlArrayItem(typeof(TreeLeaf), ElementName = "TreeLeaf")]
        public List<TreeLeaf> Leafs { get; set; }

        [XmlIgnore]
        public bool? IsDiff
        {
            get
            {
                return this.isDiff;
            }

            set
            {
                this.isDiff = value;
            }
        }

        public void AddItem(TreeItem treenode)
        {
            if (treenode is TreeNode)
            {
                if (this.Nodes == null)
                {
                    this.Nodes = new List<TreeNode>();
                }

                this.Nodes.Add(treenode as TreeNode);
            }

            if (!(treenode is TreeLeaf))
            {
                return;
            }

            if (this.Leafs == null)
            {
                this.Leafs = new List<TreeLeaf>();
            }

            this.Leafs.Add(treenode as TreeLeaf);
        }
    }
}

