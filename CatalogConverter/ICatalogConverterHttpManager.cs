namespace CatalogConverter
{
    using System;
    using System.IO;
    using System.Net.Http;

    /// <summary>Интерфейс менеджера работы с HTTP.</summary>
    public interface ICatalogConverterHttpManager : IDisposable
    {
        /// <summary>Отправка потока на сервер в команде POST.</summary>
        /// <param name="stream">Отправляемый поток.</param>
        /// <returns>Ответ от сервера.</returns>
        HttpResponseMessage PostStream(Stream stream);
    }
}