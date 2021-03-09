using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;

namespace httpEater {
	class postRequest {
		private HttpWebRequest request;
        public string url { get; set; }
        public string accept { get; set; }
        public string referer { get; set; }
        public string host { get; set; }
        public string contentType { get; set; }
        public string response { get; set; }
        public string responseEncoding { get; set; }
        public string data { get; set; }
        public string userAgent { get; set; }
        public bool? keepAlive { get; set; }
		Dictionary<string, string> headers = new Dictionary<string, string>();
            
		public bool run(ref CookieContainer cookies) {
			request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            request.Host = host;
			
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            
            request.Accept = accept;

			if (contentType != null) 
                request.ContentType = contentType;

            if (referer != null)
                request.Referer = referer;

			if (keepAlive != null)
				request.KeepAlive = (bool)keepAlive;

			// Добавление пользовательских заголовков
			foreach (KeyValuePair<string, string> keyValuePair in headers) {
                request.Headers.Add(keyValuePair.Key, keyValuePair.Value);
            }

			request.CookieContainer = cookies;

            if (userAgent != null) 
                request.UserAgent = userAgent;

			byte[] sentData = Encoding.UTF8.GetBytes(data);

			request.ContentLength = sentData.Length;

			Stream sendStream = request.GetRequestStream();
			sendStream.Write(sentData, 0, sentData.Length);
			sendStream.Close();

			// Достаём результирующую страницу
			try {
				HttpWebResponse resp = (HttpWebResponse)request.GetResponse();
				StreamReader reader = new StreamReader(resp.GetResponseStream());

				response = reader.ReadToEnd();

				reader.Close();
				resp.Close();

				return true;
			} catch (WebException ex) {
				Console.WriteLine("Произошла ошибка >.<");
				// Достаем код http-ошибки
				response = ex.Message;
				return false;
			}

		}

		// Поиск нужной информации в html документе по:
		// Какой-то строки до неё: startString
		// Расстояние до самой инофрмации: aInt
		// Строка у конца нкжной информации: endString
		public string getInfo(string startString, string endString, int aInt) {
			try {
				int temp = response.IndexOf(startString);
				int tokenStart;

				if (temp != -1)
					tokenStart = temp + aInt;
				else
					return "-1";

				int tokenEnd = response.IndexOf(endString, tokenStart);
				return response.Substring(tokenStart, tokenEnd - tokenStart);
			} catch (Exception e) {
				return e.Message;
			}
		}

		// Добавление заголовков
		public void AddHeader(string headerName, string headerValue) {
            headers[headerName] = headerValue;
        }
	}
}
