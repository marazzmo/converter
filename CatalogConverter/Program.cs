using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;

namespace CatalogConverter
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;

    using CatalogConverter.Data;
    using CatalogConverter.DB;
    using NLog;

    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            try
            {
                CatalogManager catalog = new CatalogManager();
                bool isSucces = true;

                //TODO: Подтянуть из указанного в настроках места БД с каталогом
                //TODO: затем преобразовать в удобоваримый для xml-дерева формат

                try
                {
                    Console.WriteLine("Начинается обработка каталога, это может занять некоторое время.");
                    using (var db = new DAX2012_PreProdEntities())
                    {
                        string getTree = "with  tree ([RANGEID], [RANGEIDPARENT], [PREFIX], [NAMEALIAS], [ITEMRANGEID_CRYSTALL], [RANGELEVEL]) " +
                                         "as (select  [RANGEID], cast('' as nvarchar(20)), [PREFIX], [NAMEALIAS], [ITEMRANGEID_CRYSTALL], cast(1 as bigint) " +
                                         "from [CRM_InventItemRange] " +
                                         "where rtrim(ltrim([RANGEIDPARENT])) = '' and rtrim(ltrim([RANGEID])) <> '' and rtrim(ltrim([NAMEALIAS])) <> '' " +
                                         "union all " +
                                         "select  t.[RANGEID], t.[RANGEIDPARENT], t.[PREFIX], t.[NAMEALIAS], t.[ITEMRANGEID_CRYSTALL], cast((tree.[RANGELEVEL] + 1) as bigint) " +
                                         "from [CRM_InventItemRange] t " +
                                         "inner join tree on tree.[RANGEID] = t.[RANGEIDPARENT] and rtrim(ltrim(t.[RANGEIDPARENT]))<> '' and rtrim(ltrim(t.[RANGEID])) <> '' and rtrim(ltrim(t.[NAMEALIAS])) <> '') " +
                                         "select * " +
                                         "from tree " +
                                         "order by cast([RANGELEVEL] as bigint)";
                        Console.WriteLine("Начинается обработка ветвей.");
                        var dbNodes = db.CRM_InventItemRange.SqlQuery(getTree);
                        foreach (var dbNode in dbNodes)
                        {
                            TreeNode node = new TreeNode() {Name = dbNode.NAMEALIAS.Trim(), ID = dbNode.RANGEID.Trim()};
                            catalog.AddNode(node, dbNode.RANGEIDPARENT);
                        }

                        Console.WriteLine("Начинается обработка листьев.");

                        var dbLeafs = db.CRM_InventTable.Where(a=>!string.IsNullOrEmpty(a.ITEMNAME));

                        //TODO: добавить мрц к обработке, замедлит ещё на пару-тройку секунд наверное, но не суть
                        /*var dbbs = from a in db.CRM_InventTable
                            join b in db.CRM_RetailItemGroupLine on a.ITEMID equals b.ITEMID into g
                            from x in g.DefaultIfEmpty()
                            select new
                            {
                                a.ITEMID,
                                a.ITEMNAME,
                                a.ITEMRANGEID,
                                PURCHMINPRICE(PROD | VEND | RETAIL) = (x == null ? (decimal?)null : x.PURCHMINPRICE(PROD | VEND | RETAIL))
                            };*/

                        int counter = 0;
                        int count = dbLeafs.Count();

                        foreach (var dbLeaf in dbLeafs)
                        {
                            counter += 1;
                            Console.WriteLine($"{counter}/{count}");

                            /*TreeLeaf leaf = null;
                            if (dbLeaf.PURCHMINPRICE == null)
                            {
                                leaf = new TreeLeaf() { Name = dbLeaf.ITEMNAME.Trim(), ID = dbLeaf.ITEMID.Trim() };
                            }
                            else
                            {
                                var info = new LeafInfo() { Name = "Quant", Value = dbLeaf.PURCHMINPRICE.ToString() };
                                leaf = new TreeLeaf() { Name = dbLeaf.ITEMNAME.Trim(), ID = dbLeaf.ITEMID.Trim(), AdditionalInfo = new List<LeafInfo>() { info } };
                            }*/

                            var leaf = new TreeLeaf() { Name = dbLeaf.ITEMNAME.Trim(), ID = dbLeaf.ITEMID.Trim() };
                            catalog.AddLeaf(leaf, dbLeaf.ITEMRANGEID);
                        }
                        Console.WriteLine("Обработка каталога успешно завершена.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Возникла ошибка при преобразовании каталога, подробности в логах.");
                    logger.Fatal(ex);
                    isSucces = false;
                }

                //TODO: и отправить в лоймакс
                if (isSucces)
                {
                    try
                    {
                        using (CatalogConverterHttpManager httpClient = new CatalogConverterHttpManager())
                        {
                            httpClient.Init("https://okey-dev.loymax.tech/catalogloader/o'kej/catalog_default/", "Default", "498602");
                            System.Net.Http.HttpResponseMessage result = null;
                            using (var ms = new MemoryStream())
                            {
                                using (var sw = new StreamWriter(ms))
                                {
                                    Console.WriteLine("Начинается сериализация каталога, это может занять некоторое время.");
                                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                                    ns.Add("","");

                                    XmlSerializerCache.GetXmlSerializer(typeof(TreeNode)).Serialize(sw, catalog.CollectCatalog(), ns);
                                    ms.Position = 0;

                                    Console.WriteLine("Каталог сериализован успешно, приступаем к загрузке каталога на сервер Loymax.");

                                    /*FileStream file = new FileStream("c:\\file.txt", FileMode.Create);
                                    ms.WriteTo(file);
                                    file.Close();*/

                                    result = httpClient.PostStream(ms);

                                    if (result.StatusCode != System.Net.HttpStatusCode.OK)
                                    {
                                        Console.WriteLine($"От сервера получен код ошибки HTTP {result.StatusCode}");
                                    }

                                    var loymaxResponse = new LoymaxResponseParser(result.Content.ReadAsStreamAsync().Result);
                                    Console.WriteLine($"Loymax Result = {loymaxResponse.Response.Result}, Message = {loymaxResponse.Response.Message}");
                                    if (!loymaxResponse.Response.Result.Equals("Ok", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        logger.Debug($"Loymax Result = {loymaxResponse.Response.Result}, Message = {loymaxResponse.Response.Message}");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Возникла ошибка при загрузке на сервер Loymax, подробности в логах.");
                        logger.Fatal(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникла ошибка, подробности в логах.");
                logger.Fatal(ex);
            }
            finally
            {
                Console.WriteLine("The end.");
            }

            Console.ReadLine();
        }
    }
}
