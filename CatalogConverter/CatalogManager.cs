namespace CatalogConverter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CatalogConverter.Data;

    /// <summary>Менеджер для работы с каталогом.</summary>
    internal class CatalogManager
    {
        internal CatalogManager(string rootName)
        {
            this.rootName = rootName;
        }

        private Dictionary<string, CatalogItem> catalogItems = new Dictionary<string, CatalogItem>();
        private Dictionary<string, TreeLeaf> barcodes = new Dictionary<string, TreeLeaf>();
        private string rootName;

        /// <summary>Операция по добавлению ноды в каталог.</summary>
        /// <param name="node">Добавляемая нода.</param>
        /// <param name="parentNodeId">Родительская нода.</param>
        public void AddNode(TreeNode node, string parentNodeId)
        {
            if (string.IsNullOrEmpty(parentNodeId))
            {
                parentNodeId = CheckRoot();
            }
            
            if (this.catalogItems.ContainsKey(node.ID))
            {
                var item = this.catalogItems[node.ID];
                item.Item = node;
                item.ParentId = parentNodeId;
            }
            else
            {
                this.catalogItems.Add(node.ID, new CatalogItem() { IsLeaf = false, ParentId = parentNodeId, Item = node });
            }
        }

        /// <summary>Операция по добавлению листа в каталог.</summary>
        /// <param name="leaf">Добавляемый лист.</param>
        /// <param name="nodeId">Идентификатор ноды, к которой присоединён лист.</param>
        public void AddLeaf(TreeLeaf leaf, string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                nodeId = CheckRoot();
            }

            if (this.catalogItems.ContainsKey(leaf.ID))
            {
                var item = this.catalogItems[leaf.ID];
                item.Item = leaf;
                item.ParentId = nodeId;
            }
            else
            {
                this.catalogItems.Add(leaf.ID, new CatalogItem() { Item = leaf, ParentId = nodeId, IsLeaf = true });
            }
        }

        /// <summary>Операция по сбору каталога.</summary>
        /// <returns>Каатлог(?).</returns>
        public TreeNode CollectCatalog()
        {
            //Поиск корня каталога
            var root = this.catalogItems.Values.FirstOrDefault(i => i.ParentId == null).Item;
            if (root == null)
            {
                throw new Exception("Корень каталога не обнаружен.");
            }

            var catalog = this.CopyNode(root);
            catalog.IsDiff = true;
            this.CollectCatalogInternal(catalog);
            return catalog;
        }

        /// <summary>Копирование ноды.</summary>
        /// <param name="item">Изначальная нода.</param>
        /// <returns>Результирующая нода.</returns>
        private TreeNode CopyNode(TreeItem item)
        {
            var result = new TreeNode();
            result.ID = item.ID;
            result.Name = item.Name;
            result.Attributes = item.Attributes;
            return result;
        }

        /// <summary>Копирование листа.</summary>
        /// <param name="item">Изначальный лист.</param>
        /// <returns>Результирующий лист.</returns>
        private TreeLeaf CopyLeaf(TreeLeaf item)
        {
            var result = new TreeLeaf();
            result.ID = item.ID;
            result.Name = item.Name;
            result.Attributes = item.Attributes;
            result.Barcodes = item.Barcodes;
            return result;
        }

        /// <summary>Внутренняя реализация сбора каталога.</summary>
        /// <param name="catalog">Родительская нода.</param>
        private void CollectCatalogInternal(TreeNode catalog)
        {
            //Спускаемся вниз(вглубь?) по дереву
            foreach (var node in this.catalogItems.Values.Where(n => n.ParentId == catalog.ID && n.IsLeaf == false))
            {
                var newNode = this.CopyNode(node.Item);
                catalog.AddItem(newNode);
                this.CollectCatalogInternal(newNode);
            }

            //Добавляем листья
            foreach (var leaf in this.catalogItems.Values.Where(n => n.ParentId == catalog.ID && n.IsLeaf == true))
            {
                catalog.AddItem(this.CopyLeaf((TreeLeaf)leaf.Item));
            }
        }

        /// <summary>Проверка уникальности ШК(?).</summary>
        /// <param name="leaf">Лист.</param>
        private void CheckBarcode(TreeLeaf leaf)
        {
            foreach (var barcodeEntity in leaf.Barcodes)
            {
                var barcode = barcodeEntity.Value;
                if (this.barcodes.ContainsKey(barcode))
                {
                    var existingLeaf = this.barcodes[barcode];
                    var existingBarcodeEntity = existingLeaf.Barcodes.FirstOrDefault(b => b.Value == barcode);
                    existingLeaf.Barcodes.Remove(existingBarcodeEntity);
                    this.barcodes[barcode] = leaf;
                    return;
                }

                this.barcodes.Add(barcode, leaf);
            }
        }

        /// <summary>Проверка наличия корня, и создание его если отсутсвует.</summary>
        /// <returns>Корень.</returns>
        private string CheckRoot()
        {
            if (this.catalogItems.ContainsKey(rootName))
            {
                return rootName;
            }

            this.catalogItems.Add(rootName, new CatalogItem() { IsLeaf = false, ParentId = null, Item = new TreeItem() { ID = rootName, Name = rootName } });
            return rootName;
        }
    }
}
