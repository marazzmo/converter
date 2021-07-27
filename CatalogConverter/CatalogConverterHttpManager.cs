namespace CatalogConverter
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Text;

    /// <summary>Реализация  менеджера работы с HTTP.</summary>
    class CatalogConverterHttpManager : ICatalogConverterHttpManager
    {
        private HttpClient clientInternal;
        private string addressInternal;

        /// <summary>Инициализация менеджера для работы с HTTP.</summary>
        /// <param name="address">Адрес сервера HTTP.</param>
        /// <param name="login">Логин для сервера HTTP.</param>
        /// <param name="password">Пароль от логина для сервера HTTP.</param>
        public void Init(string address, string login, string password)
        {
            this.clientInternal = new HttpClient();
            var byteArray = Encoding.ASCII.GetBytes($"{login}:{password}");
            this.clientInternal.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            this.addressInternal = address;
            this.clientInternal.Timeout = new TimeSpan(0, 10, 0);
        }

        /// <summary>Отправка потока на сервер в команде POST.</summary>
        /// <param name="stream">Отправляемый поток.</param>
        /// <returns>Ответ от сервера.</returns>
        public HttpResponseMessage PostStream(Stream stream)
        {
            using (var content = new StreamContent(stream))
            {
                HttpResponseMessage response = this.clientInternal.PostAsync(this.addressInternal, content).Result;
                return response;
            }
        }

        /// <summary>Освобождение ресурсов.</summary>
        public void Dispose()
        {
            if (this.clientInternal == null)
            {
                return;
            }

            this.clientInternal.Dispose();
            this.clientInternal = null;
        }
    }
}

