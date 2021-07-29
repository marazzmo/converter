using System.Configuration;
using CatalogConverter.DAL;

namespace CatalogConverter
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;

    using CatalogConverter.Data;
    using NLog;

    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            //TODO: брать из настроек каких-нибудь
            string loymaxLogin = ConfigurationManager.AppSettings.Get("loymaxLogin");
            string loymaxPassword = ConfigurationManager.AppSettings.Get("loymaxPassword");
            string loymaxAddres = ConfigurationManager.AppSettings.Get("loymaxAddres");
            string catalogAddres = ConfigurationManager.AppSettings.Get("catalogAddres");
            string resultUri = loymaxAddres + catalogAddres;

            try
            {
                CatalogManager catalog = new CatalogManager(loymaxLogin);
                bool isSucces = true;

                try
                {
                    Console.WriteLine("Начинается обработка каталога, это может занять некоторое время.");
                    logger.Debug("Обработка каталога началась.");

                    using (var db = new ConverterDbContext())
                    {
                        string getTree = "with  tree ([RANGEID], [RANGEIDPARENT], [NAMEALIAS], [ITEMRANGEID_CRYSTALL], level) " +
                                         "as (select  [RANGEID], cast('' as nvarchar(20)), [NAMEALIAS], [ITEMRANGEID_CRYSTALL], 0 " +
                                         "from [CRM_InventItemRange] " +
                                         "where rtrim(ltrim([RANGEIDPARENT])) = '' and rtrim(ltrim([RANGEID])) <> '' and rtrim(ltrim([NAMEALIAS])) <> '' " +
                                         "union all " +
                                         "select  t.[RANGEID], t.[RANGEIDPARENT], t.[NAMEALIAS], t.[ITEMRANGEID_CRYSTALL], tree.level + 1 " +
                                         "from [CRM_InventItemRange] t " +
                                         "inner join tree on tree.[RANGEID] = t.[RANGEIDPARENT] and rtrim(ltrim(t.[RANGEIDPARENT])) <> '' and rtrim(ltrim(t.[RANGEID])) <> '' and rtrim(ltrim(t.[NAMEALIAS])) <> '') " +
                                         "select * " +
                                         "from tree " +
                                         "order by level";
                        var dbNodes = db.InventItemRanges.SqlQuery(getTree);
                        Console.WriteLine("Начинается обработка ветвей.");
                        logger.Debug($"Обработка ветвей, найдено {dbNodes.Count()}.");
                        foreach (var dbNode in dbNodes)
                        {
                            TreeNode node = new TreeNode() {Name = dbNode.NAMEALIAS.Trim(), ID = dbNode.RANGEID.Trim()};
                            catalog.AddNode(node, dbNode.RANGEIDPARENT);
                        }

                        var dbLeafs = db.InventTables.Where(a => !string.IsNullOrEmpty(a.ITEMNAME));
                        Console.WriteLine("Начинается обработка листьев.");
                        logger.Debug($"Обработка листьев, найдено {dbLeafs.Count()}.");
                        foreach (var dbLeaf in dbLeafs)
                        {
                            var leaf = new TreeLeaf() { Name = dbLeaf.ITEMNAME.Trim(), ID = dbLeaf.ITEMID.Trim() };
                            catalog.AddLeaf(leaf, dbLeaf.ITEMRANGEID);
                        }

                        Console.WriteLine("Обработка каталога успешно завершена.");
                        logger.Debug("Обработка каталога завершена.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Возникла ошибка при преобразовании каталога, подробности в логах.");
                    logger.Fatal(ex);
                    isSucces = false;
                }

                if (isSucces)
                {
                    try
                    {
                        using (CatalogConverterHttpManager httpClient = new CatalogConverterHttpManager())
                        {
                            httpClient.Init(resultUri, loymaxLogin, loymaxPassword);
                            System.Net.Http.HttpResponseMessage result = null;
                            using (var ms = new MemoryStream())
                            {
                                using (var sw = new StreamWriter(ms))
                                {
                                    Console.WriteLine("Начинается сериализация каталога, это может занять некоторое время.");
                                    logger.Debug("Сериалиазция каталога началась.");

                                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                                    ns.Add("","");
                                    XmlSerializerCache.GetXmlSerializer(typeof(TreeNode)).Serialize(sw, catalog.CollectCatalog(), ns);
                                    ms.Position = 0;

                                    Console.WriteLine("Каталог сериализован успешно, приступаем к загрузке каталога на сервер Loymax.");
                                    logger.Debug("Загрузка каталога на сервер началась.");

                                    result = httpClient.PostStream(ms);
                                    if (result.StatusCode != System.Net.HttpStatusCode.OK)
                                    {
                                        Console.WriteLine($"От сервера получен код ошибки HTTP {result.StatusCode}");
                                        logger.Debug($"От сервера получен код ошибки HTTP {result.StatusCode}");
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
