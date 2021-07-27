namespace CatalogConverter
{
    using System.IO;
    using System.Xml.Serialization;

    /// <summary>Парсер результата загрузки каталога товаров.</summary>
    class LoymaxResponseParser
    {
        /// <summary>Initializes a new instance of the <see cref="LoymaxResponseParser".</summary>
        /// <param name="stream">Поток с данными.</param>
        public LoymaxResponseParser(Stream stream)
        {
            XmlSerializer s = new XmlSerializer(typeof(LoymaxResponse));
            using (StreamReader streamReader = new StreamReader(stream))
            {
                this.Response = (LoymaxResponse)s.Deserialize(streamReader);
            }
        }

        /// <summary>Результат парсинга.</summary>
        public LoymaxResponse Response { get; private set; }
    }
}
