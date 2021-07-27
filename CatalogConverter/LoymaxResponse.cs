namespace CatalogConverter
{
    using System;
    using System.Xml.Serialization;

    /// <summary>Результат обработки на сервере Loymax.</summary>
    [Serializable, XmlRoot(ElementName = "Response")]
    public class LoymaxResponse
    {
        /// <summary>Сообщение об результате.</summary>
        public string Result { get; set; }

        /// <summary>Комментарий результата обрабоки.</summary>
        public string Message { get; set; }
    }
}