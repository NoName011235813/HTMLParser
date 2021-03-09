using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.IO;

namespace httpEater {
	class getRequest {
		private HttpWebRequest request;
        public string url { get; set; }
        public string accept { get; set; }
        public string referer { get; set; }
        public string host { get; set; }
        public string contentType { get; set; }
        public string response { get; private set; }
		public bool? keepAlive { get; set; }
		Dictionary<string, string> Headers = new Dictionary<string, string>();
           
		public bool run(ref CookieContainer cookies) {
			request = (HttpWebRequest)HttpWebRequest.Create(url);

			// Установка заголовков запроса
			request.Method = "Get";
			request.Accept = accept;
			request.Host = host;
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

			if (contentType != null) 
                request.ContentType = contentType;

			if (keepAlive != null)
                request.KeepAlive = (bool)keepAlive;

			if (referer != null)
                request.Referer = referer;

			// Добавление доп. заголовков
			foreach (KeyValuePair<string, string> keyValuePair in Headers) {
                request.Headers.Add(keyValuePair.Key, keyValuePair.Value);
            }

			request.CookieContainer = cookies;

			// Достаём результирующую страницу
			try {
				HttpWebResponse resp = (HttpWebResponse)request.GetResponse();
				StreamReader reader = new StreamReader(resp.GetResponseStream());

				response = reader.ReadToEnd();

				reader.Close();
				resp.Close();

				return true;
			} catch (WebException ex) {
				response = ex.Message;
				return false;
			}

			
		}

		// Добавление доп. заголовков
		public void AddHeader(string headerName, string headerValue) {
			Headers[headerName] = headerValue;
		}
	
		// Поиск нужной информации в html документе по:
		// Какой-то строки до неё: startString
		// Расстояние до самой инофрмации: aInt
		// Строка у конца нужной информации: endString
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
	}
}
