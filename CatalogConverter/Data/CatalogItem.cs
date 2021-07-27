namespace CatalogConverter.Data
{
    internal class CatalogItem
    {
        public TreeItem Item { get; set; }
        public string ParentId { get; set; }
        public bool IsLeaf { get; set; }
    }
}
