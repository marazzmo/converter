using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Xml;
using CatalogConverter.DB;

namespace CatalogConverter
{
    using System;
    using System.IO;

    using CatalogConverter.Data;
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
                                         "where rtrim(ltrim([RANGEIDPARENT])) = '' and rtrim(ltrim([RANGEID])) <> '' " +
                                         "union all " +
                                         "select  t.[RANGEID], t.[RANGEIDPARENT], t.[PREFIX], t.[NAMEALIAS], t.[ITEMRANGEID_CRYSTALL], cast((tree.[RANGELEVEL] + 1) as bigint) " +
                                         "from [CRM_InventItemRange] t " +
                                         "inner join tree on tree.[RANGEID] = t.[RANGEIDPARENT] and rtrim(ltrim(t.[RANGEIDPARENT]))<> '' and rtrim(ltrim(t.[RANGEID])) <> '') " +
                                         "select * " +
                                         "from tree " +
                                         "order by cast([RANGELEVEL] as bigint)";
                        Console.WriteLine("Начинается обработка ветвей.");
                        var dbNodes = db.CRM_InventItemRange.SqlQuery(getTree);
                        foreach (var dbNode in dbNodes)
                        {
                            TreeNode node = new TreeNode() {Name = dbNode.NAMEALIAS, ID = dbNode.RANGEID};
                            catalog.AddNode(node, dbNode.RANGEIDPARENT);
                        }

                        Console.WriteLine("Начинается обработка листьев.");

                        /*var dbLeafs = db.CRM_InventTable;*/

                        var dbbs = from a in db.CRM_InventTable
                            join b in db.CRM_BrandTable on a.BRANDID equals b.BRANDID into g
                            from x in g.DefaultIfEmpty()
                            select new
                            {
                                a.ITEMID,
                                a.ITEMNAME,
                                a.ITEMRANGEID,
                                BRANDNAME = (x == null ? string.Empty : x.NAME)
                            };


                        int counter = 0;
                        int count = dbbs.Count();

                        foreach (var dbLeaf in dbbs)
                        {
                            counter += 1;
                            Console.WriteLine($"{counter}/{count}");

                            TreeLeaf leaf = null;
                            if (string.IsNullOrEmpty(dbLeaf.BRANDNAME))
                            {
                                leaf = new TreeLeaf() { Name = dbLeaf.ITEMNAME, ID = dbLeaf.ITEMID };
                            }
                            else
                            {
                                var info = new LeafInfo() { Name = "Brand", Value = dbLeaf.BRANDNAME };
                                leaf = new TreeLeaf() { Name = dbLeaf.ITEMNAME, ID = dbLeaf.ITEMID, AdditionalInfo = new List<LeafInfo>() { info } };
                            }

                            /*var leaf = new TreeLeaf() { Name = dbLeaf.ITEMNAME, ID = dbLeaf.ITEMID };*/
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
                        Console.WriteLine("Начинается загрузка на сервер Loymax, это может занять некоторое время.");
                        using (CatalogConverterHttpManager httpClient = new CatalogConverterHttpManager())
                        {
                            httpClient.Init("address", "login", "password");
                            System.Net.Http.HttpResponseMessage result = null;
                            using (var ms = new MemoryStream())
                            {
                                using (var sw = new StreamWriter(ms))
                                {
                                    XmlSerializerCache.GetXmlSerializer(typeof(TreeNode)).Serialize(sw, catalog.CollectCatalog());
                                    ms.Position = 0;

                                    FileStream file = new FileStream("c:\\file.txt", FileMode.Create);
                                    ms.WriteTo(file);
                                    file.Close();
                                    /*result = httpClient.PostStream(ms);

                                    if (result.StatusCode != System.Net.HttpStatusCode.OK)
                                    {
                                        Console.WriteLine($"От сервера получен код ошибки HTTP {result.StatusCode}");
                                    }

                                    var loymaxResponse = new LoymaxResponseParser(result.Content.ReadAsStreamAsync().Result);
                                    Console.WriteLine($"Loymax Result = {loymaxResponse.Response.Result}, Message = {loymaxResponse.Response.Message}");
                                    if (!loymaxResponse.Response.Result.Equals("Ok", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        logger.Debug($"Loymax Result = {loymaxResponse.Response.Result}, Message = {loymaxResponse.Response.Message}");
                                    }*/

                                    //TODO: нужна ли чистка? Наврядли, из бд же берём данные. Хммм. 
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Возникла ошибка при загрузке на сервер Lymax, подробности в логах.");
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
