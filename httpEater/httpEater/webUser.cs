using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Net;

namespace httpEater {




	class webUser {

		const string cookieFileName = "httpEaterFile.bin";
		const string teacherFileName = "teacherList.bin";
		const string taskFileName = "taskList.bin";
		const string authDataFileName = "auth.bin";
		const string userAgentString = "Atlanta ATH-5431 230V Dry Iron (Yandere's Chainsaw; x64)";

		const string acceptHeaderString = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";

		public string login { get; set; }
		public string password { get; set; }
		private CookieContainer cookie = new CookieContainer();

		enum TaskType {Quiz, Task};

		// Список - ФИО, Ссылка
		private List<KeyValuePair<string, string>> teachers = new List<KeyValuePair<string, string>>();
		// Список - Группа_Задание, ссылка, тип_задания
		private List<Tuple<string, string, TaskType>> tasks = new List<Tuple<string, string, TaskType>>();

		public webUser() {

			Console.WriteLine("Считываю куки...");
			if (readCookie())
				Console.WriteLine("Куки успешно найдены и загружены");
			else
				Console.WriteLine("Файлы с куками не были найдены");
			
			Console.WriteLine("Ищу твой логин и пароль...");
			readLoginAndPassword();

			// Проверка авторизации
			Console.WriteLine("Проверяю сессию...");
			CheckSession();
			Console.WriteLine("Готово");

			Console.WriteLine("Считываю список учителей...");
			readTeachersList();

			Console.WriteLine("Считываю список заданий...");
			readTasksList();
			
			Console.WriteLine("Удачной работы");
		}
		
		//Кол-во элементов в списке учителей
		public int getTeachersCount() {
			return teachers.Count;
		}

		//Кол-во элементов в списках
		public int getTaskCount() {
			return tasks.Count;
		}




		// Сохранение логина и пароля
		// Сохранение списка учителей
		public void saveLoginAndPassword() {
			
			string data = login + " - " + password;
			
			try {
				IFormatter formatter = new BinaryFormatter();
				Stream stream = new FileStream(authDataFileName, FileMode.Create, FileAccess.Write, FileShare.None);
				formatter.Serialize(stream, data);
				stream.Close(); 
			} catch (Exception e) {
				Console.WriteLine("Произошла ошибка при сохранении Логина и пароля");
				Console.WriteLine(e.Message);
			}
		}
	
		// Считываение списка
		private void readLoginAndPassword() {

			IFormatter formatter = new BinaryFormatter();

			string temp;

			if (File.Exists(authDataFileName)) {
                try {

					Stream stream = new FileStream(authDataFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
					temp = (string)formatter.Deserialize(stream);

					stream.Close();

				} catch (Exception e) {

					Console.WriteLine("Не удалось загрузить логин и пароль");
					Console.WriteLine(e.Message);
					return;

				} 

				string[] data = temp.Split('-');
				login = data[0].Trim();
				password = data[1].Trim();


			} else {
				Console.WriteLine("Ты не указывал еще логина и пароля");
			}
		}

		public enum UrlType {Quiz, Task, Teacher};

		// Проверка передаваемой сыслки 
		public bool CheckUrlForELearning(string url, UrlType aType) {

			if (!url.Contains("http://") && !url.Contains("https://")) 
				url = "http://" + url;

			// Если это ссылка
			if (Uri.IsWellFormedUriString(url, UriKind.Absolute)) {
				
				string temp = "http://";

				// И ссылка содержит, что нужно, а так же параметр имеет какое-то значение

				if (aType == UrlType.Quiz) 
					temp += "e-learning.bmstu.ru/mtkp/mod/quiz/view.php?id=";
						
				else if (aType == UrlType.Task) 
					temp += "e-learning.bmstu.ru/mtkp/mod/assign/view.php?id=";

				else if (aType == UrlType.Teacher)
					temp += "e-learning.bmstu.ru/mtkp/user/profile.php?id=";

				if (url.Contains(temp) && (url.Length != temp.Length))
					return true;

			}  
			
			return false;
			
				
		}




		// Чтение куков из файла
		private bool readCookie() {
			IFormatter formatter = new BinaryFormatter();

			if (File.Exists(cookieFileName)) {
                try {
					Stream stream = new FileStream(cookieFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
					cookie = (CookieContainer)formatter.Deserialize(stream);

					stream.Close();

					return true;

				} catch (Exception e) {
					Console.WriteLine(e.Message);
					return false;
				} 

			} else { 
				return false;
			} 
		}

		// Сохранение файлов cookie
		private void cookieSave() {
			IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(cookieFileName, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, cookie);
            stream.Close();
		}

		// Запрос к сайту
		private getRequest CreateELearningRequest(string url) {

			getRequest gReq = new getRequest();
            gReq.url = url;
            gReq.accept = acceptHeaderString;
            gReq.referer = "http://e-learning.bmstu.ru/mtkp/";
            gReq.keepAlive = true;
            gReq.host = "e-learning.bmstu.ru";

            gReq.AddHeader("Upgrade-Insecure-Requests", "1");

            if (!(gReq.run(ref cookie))) {
				Console.WriteLine(gReq.response);
				return null;
			}

			return gReq;
		}

		// Проверка произведен ли вход на сайт или нет
		private bool isAuthorized(getRequest req) {
			
			// Поиск элемента
			string searching = "login-header";
			string loginHeader = req.getInfo(searching, "</", searching.Length + 15);

			if (loginHeader == "ЕДИНАЯ СЛУЖБА WEB АУТЕНТИФИКАЦИИ")
				return false;
			else
				return true;
		}
	
		// Авторизация
		public bool auth() {

			// Проверка ввода логина и пароля
			if ((login == null) || (password == null)) {
				Console.WriteLine("Введи логин и пароль");
				return false;
			}

			// Достаём фигов дофига длинный код для передачи в POST запрос
            getRequest gReq = new getRequest();
            gReq.url = "https://proxy.bmstu.ru:8443/cas/login?service=http%3A%2F%2Fe-learning.bmstu.ru%2Fmtkp%2Flogin%2Findex.php";
            gReq.accept = acceptHeaderString;
            gReq.host = "proxy.bmstu.ru:8443";
            gReq.keepAlive = true;

            gReq.referer = "http://e-learning.bmstu.ru/mtkp/";
            gReq.AddHeader("Sec-Fetch-Dest", "document");
            gReq.AddHeader("Sec-Fetch-Mode", "navigate");
            gReq.AddHeader("Sec-Fetch-Site", "cross-site");
            gReq.AddHeader("Sec-Fetch-User", "?1");
            gReq.AddHeader("Upgrade-Insecure-Requests", "1");

            if (!(gReq.run(ref cookie))) {
				Console.WriteLine("Произошла ошибка при обработке запроса - " + gReq.response);
				return false;
			}

			// Вытаскиваем ту самую строку для перердачи
			string token = gReq.getInfo("execution", "\"", 18);

			// Формируем данные для POST запроса 
			string data = 
				"username=" + WebUtility.UrlEncode(login) +
				"&password=" + WebUtility.UrlEncode(password) +
				"&execution=" + WebUtility.UrlEncode(token) +
				"&_eventId=submit&geolocation=";

			postRequest pReq = new postRequest();

			pReq.data = data;
            pReq.url = "https://proxy.bmstu.ru:8443/cas/login?service=http%3A%2F%2Fe-learning.bmstu.ru%2Fmtkp%2Flogin%2Findex.php";
            pReq.accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            
			pReq.userAgent = userAgentString;
            //pReq.userAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.183 Safari/537.36 OPR/72.0.3815.320";
            pReq.contentType = "application/x-www-form-urlencoded";
            pReq.referer = "https://proxy.bmstu.ru:8443/cas/login?service=http%3A%2F%2Fe-learning.bmstu.ru%2Fmtkp%2Flogin%2Findex.php";
            pReq.keepAlive = true;
            pReq.host = "proxy.bmstu.ru:8443";

			pReq.AddHeader("Origin", "https://proxy.bmstu.ru:8443");
                
            pReq.AddHeader("Sec-Fetch-Dest", "document");
            pReq.AddHeader("Sec-Fetch-Mode", "navigate");
            pReq.AddHeader("Sec-Fetch-Site", "same-origin");
            pReq.AddHeader("Sec-Fetch-User", "?1");
            pReq.AddHeader("Upgrade-Insecure-Requests", "1");

			if (!(pReq.run(ref cookie))) {
				Console.WriteLine("Произошла ошибка при обработке запроса - " + pReq.response);

				if (pReq.response == "401")
					Console.WriteLine("Ошибка связанна с авториазцией, проверьте введенные логин и пароль - " + login + " : " + password);

				return false;
			}

			string userName = pReq.getInfo("usertext mr-1", "</", 15);

			Console.WriteLine("Пользователь: " + userName);
			
			cookieSave();

			return true;
		}
		
		// Проверка сессии
		private bool CheckSession() {

			getRequest gReq = CreateELearningRequest("http://e-learning.bmstu.ru/mtkp/my/");

			if (!isAuthorized(gReq)) {
				Console.WriteLine("Сессия истекла, я попытаюсь залогинить тебя");

				if (!auth()) {
					Console.WriteLine("Я не смогла тебя залогинить");
					return false;
				} else {
					Console.WriteLine("Я тебя залогинила");
				}
			}

			
			return true;
		}
	
		// Автоматическая авторизация
		private bool AutoAuthorization(ref getRequest req) {

			if (!isAuthorized(req)) {

				Console.WriteLine("Сессия истекла, произвожу авторизацию...");

				if (auth()) {

					Console.WriteLine("Повторяю запрос...");
					req.run(ref cookie);

				} else {

					Console.WriteLine("Авторизация не удалась");
					return false;

				}

			}

			return true;

		}



		// Работа со списком учителей
		// Сохранение списка учителей
		private void saveTeachersList() {
			try {
				IFormatter formatter = new BinaryFormatter();
				Stream stream = new FileStream(teacherFileName, FileMode.Create, FileAccess.Write, FileShare.None);
				formatter.Serialize(stream, teachers);
				stream.Close(); 
			} catch (Exception e) {
				Console.WriteLine("Произошла ошибка при сохранении списка учителей");
				Console.WriteLine(e.Message);
			}
		}
	
		// Считываение списка
		private void readTeachersList() {
			teachers.Clear();

			IFormatter formatter = new BinaryFormatter();

			if (File.Exists(teacherFileName)) {
                try {

					Stream stream = new FileStream(teacherFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
					teachers = (List<KeyValuePair<string, string>>)formatter.Deserialize(stream);

					stream.Close();

				} catch (Exception e) {

					Console.WriteLine("Не удалось загрузить файл с ссылками учителей");
					Console.WriteLine(e.Message);

				} 
			} 
		}

		// Находим имя учителя
		private string getTeacherName(string url) {

			getRequest gReq = CreateELearningRequest(url);

			if ((gReq == null) || !AutoAuthorization(ref gReq))
				return null;

			string searching = "page-header-headings";

			string res = gReq.getInfo(searching, "</", searching.Length + 6);

			return res;

		}

		// Находим последний онлайн
		private string getTeacherTime(string url) {
			
			getRequest gReq = CreateELearningRequest(url);

			if ((gReq == null) || !AutoAuthorization(ref gReq))
				return null;

			string searching = "Последний доступ к сайту";

			string res = gReq.getInfo(searching, "</", searching.Length + 9);

			if (res == "-1")
				return "Не найдено";

			res = res.Replace("&nbsp;", null);

			return res;

		}

		// Добавление учителя
		public void addTeacherUrl(string url) {

			string res = getTeacherName(url);

			if (res == null) {
				Console.WriteLine("Добавление учителя было остановлено");
				return;
			}

			foreach (KeyValuePair<string, string> teacher in teachers)
				if (teacher.Key == res) {
					Console.WriteLine("Данное учитель уже в списке");
					return;
				}

			teachers.Add(new KeyValuePair<string, string>(res, url));
			saveTeachersList();
			
		}

		// Просмотр списка учителей
		public void viewTeacherList() {
			if (teachers.Count > 0) {

				Console.WriteLine();

				foreach(KeyValuePair<string, string> teacher in teachers) 
					Console.WriteLine(teachers.IndexOf(teacher) + ". " + teacher.Key + " - " + teacher.Value);

				Console.WriteLine();

			} else
				Console.WriteLine("Список пуст");
		}

		// Удаление 
		public void removeATeacher(int index) {
			if ((index >= 0) && (index < teachers.Count())) {
				teachers.RemoveAt(index);
				saveTeachersList();
			} else 
				Console.WriteLine("Ты ввел недопустимое число");
			
		}

		// Мониторинг входов учителей
		public void monitorTeachers() {

			if (!CheckSession()) {
				Console.WriteLine("Я остановила мониторинг");
				return;
			}

			if (teachers.Count == 0) {
				Console.WriteLine("Список пуст");
				return;
			}

			Console.WriteLine();

			int maxNameLength = 0;
			foreach (KeyValuePair<string, string> teacher in teachers) {
				if (maxNameLength < teacher.Key.Length) 
					maxNameLength = teacher.Key.Length;
			}

			int secondColumnWidth = 65;

			// Вывод шапки таблицы
			Console.WriteLine(
				"+" + 
				string.Concat(Enumerable.Repeat("-", maxNameLength + 2)) + 
				"+" + 
				string.Concat(Enumerable.Repeat("-", secondColumnWidth)) +
				"+"
			);

			string temp = "Учителя";

			Console.WriteLine(
				"| " +
				temp + 
				string.Concat(Enumerable.Repeat(" ", maxNameLength + 1 - temp.Length)) +
				"| Был(а) онлайн" + 
				string.Concat(Enumerable.Repeat(" ", secondColumnWidth - 14)) + 
				"|"
			);

			Console.WriteLine(
				"+" + 
				string.Concat(Enumerable.Repeat("-", maxNameLength + 2)) + 
				"+" + 
				string.Concat(Enumerable.Repeat("-", secondColumnWidth)) +
				"+"
			);

			// Тело таблицы
			foreach (KeyValuePair<string, string> teacher in teachers) {
				string res = getTeacherTime(teacher.Value);
				
				// res[0] имя учителя, res[1] время последнего онлайна
				if (res != null)
					Console.WriteLine(
						"| " +
						teacher.Key +
						string.Concat(Enumerable.Repeat(" ", maxNameLength + 1 - teacher.Key.Length)) + 
						"|" +
						res +
						string.Concat(Enumerable.Repeat(" ", secondColumnWidth - res.Length)) +
						"|"
					);
			}

			// Граница таблицы
			Console.WriteLine(
				"+" + 
				string.Concat(Enumerable.Repeat("-", maxNameLength + 2)) + 
				"+" + 
				string.Concat(Enumerable.Repeat("-", secondColumnWidth)) +
				"+"
			);

			Console.WriteLine();
		}





		// Работа с заданиями
		// Сохранение списка заданий
		private void saveTasksList() {
			try {
				IFormatter formatter = new BinaryFormatter();
				Stream stream = new FileStream(taskFileName, FileMode.Create, FileAccess.Write, FileShare.None);
				formatter.Serialize(stream, tasks);
				stream.Close();
			} catch (Exception e) {
				Console.WriteLine("Произошла ошибка при сохранении списка файлов");
				Console.WriteLine(e.Message);
			}
		}

		// Чтение списка заданий из файла
		private void readTasksList() {
			tasks.Clear();

			IFormatter formatter = new BinaryFormatter();

			if (File.Exists(taskFileName)) {
                try {

					Stream stream = new FileStream(taskFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
					tasks = (List<Tuple<string, string, TaskType>>)formatter.Deserialize(stream);

					stream.Close();

				} catch (Exception e) {
					Console.WriteLine("Не удалось загрузить файл с заданиями");
					Console.WriteLine(e.Message);
				} 
			} 
		}

		// Запрос на информацию о задании [0] - Название, [1] - Тип
		private string[] getTaskNameAndType(string url) {
			
			getRequest gReq = CreateELearningRequest(url);

			if ((gReq == null) || !AutoAuthorization(ref gReq)) 
				return null;

			// Результат
			string[] res = new string[2];

			// Искомое значение
			string searching = "title";

			// Название пары
			res[0] = gReq.getInfo(searching, "_", searching.Length + 1);

			// Если не найдена пара
			if (res[0] == "-1")
				res[0] = "404";

			// Проверка задание ли это или тест
			if (url.IndexOf("quiz") > 0) 
				searching = "Тест";
			else 
				searching = "Задание";

			res[1] = searching;

			string temp = gReq.getInfo(searching, "</", searching.Length + 2);

			if (temp == "-1")
				temp = "404";

			res[0] += " - " + temp;

			return res;

		}

		// Добавление задания
		public void addTaskUrl(string url) {

			string[] res = getTaskNameAndType(url);

			if (res == null) {
				Console.WriteLine("Добавление задании было остановлено");
				return;
			}

			string key = res[0];

			// Проверка на существование этого задания
			foreach (Tuple<string, string, TaskType> task in tasks) 
				// Если найдено задание с таким именем
				if (task.Item1 == key) 
					// Сравниваем ссылки
					if (task.Item2 == url) {
						Console.WriteLine("Данное задание уже добавлено");
						return;
					}
			
			Tuple<string, string, TaskType> NewTask;

			if (res[1] == "Тест")
				NewTask = new Tuple<string,string,TaskType>(key, url, TaskType.Quiz);
			else
				NewTask = new Tuple<string,string,TaskType>(key, url, TaskType.Task);


			tasks.Add(NewTask);

			saveTasksList();						
		}

		// Запрос на оценку
		private string getGrade(Tuple<string, string, TaskType> task) {
			
			getRequest gReq = CreateELearningRequest(task.Item2);

			if ((gReq == null) || !AutoAuthorization(ref gReq))
				return null;

			string searching;
			string res;
			
			if (task.Item3 == TaskType.Task) {
				searching = "submissiongraded cell c1 lastcol";

				res = gReq.getInfo(searching, "</", 0);

				if (res != "-1") {
					searching ="&nbsp;/&nbsp;";

					// Необработтаная оценка, может содержать ">" в начале
					string RawGrade = gReq.getInfo(searching, searching, -5);
					RawGrade += "/" + gReq.getInfo(searching, "</", searching.Length);

					RawGrade = RawGrade.Replace(">", "");

					res = "оценено, " + RawGrade;

				} else
					res = "не оценено";

			} else {
				searching = "cell c2";

				// Та же функция getRequest.getInfo, но измененная ввиду необходимости
				int tokenStart = gReq.response.IndexOf(searching) - 32;
				tokenStart = gReq.response.IndexOf(">", tokenStart) + 1;
				int tokenEnd = gReq.response.IndexOf("</", tokenStart);

				string RawGrade = gReq.response.Substring(tokenStart, tokenEnd-tokenStart);

				if (RawGrade != "Еще не оценено")
					res = "оценено, " + RawGrade;
				else
					res = "не оценено";

			}

			return res;
		}

		// Мониторинг заданий
		public void monitorTasks() {

			if (!CheckSession()) {
				Console.WriteLine("Я остановила мониторинг");
				return;
			}


			if (tasks.Count == 0) {
				Console.WriteLine("Список пуст");
				return;
			}

			Console.WriteLine();

			int maxNameLength = 0;
			foreach (Tuple<string, string, TaskType> task in tasks) {
				if (maxNameLength < task.Item1.Length) 
					maxNameLength = task.Item1.Length;
			}

			int secondColumnWidth = 25;

			// Вывод шапки таблицы
			Console.WriteLine(
				"+" + 
				string.Concat(Enumerable.Repeat("-", maxNameLength + 2)) + 
				"+" + 
				string.Concat(Enumerable.Repeat("-", secondColumnWidth)) +
				"+"
			);

			string temp = "Задание";

			Console.WriteLine(
				"| " +
				temp + 
				string.Concat(Enumerable.Repeat(" ", maxNameLength + 1 - temp.Length)) +
				"| Оценка" + 
				string.Concat(Enumerable.Repeat(" ", secondColumnWidth - 7)) + 
				"|"
			);

			Console.WriteLine(
				"+" + 
				string.Concat(Enumerable.Repeat("-", maxNameLength + 2)) + 
				"+" + 
				string.Concat(Enumerable.Repeat("-", secondColumnWidth)) +
				"+"
			);

			// Тело таблицы
			foreach (Tuple<string, string, TaskType> task in tasks) {
				string res = getGrade(task);
				
				// res - оценка
				Console.WriteLine(
					"| " +
					task.Item1 +
					string.Concat(Enumerable.Repeat(" ", maxNameLength + 1 - task.Item1.Length)) + 
					"|" +
					res +
					string.Concat(Enumerable.Repeat(" ", secondColumnWidth - res.Length)) +
					"|"
				);
			}

			// Граница таблицы
			Console.WriteLine(
				"+" + 
				string.Concat(Enumerable.Repeat("-", maxNameLength + 2)) + 
				"+" + 
				string.Concat(Enumerable.Repeat("-", secondColumnWidth)) +
				"+"
			);

			Console.WriteLine();
		}

		// Просмотр списка заданий
		public void viewTaskList() {

			if (tasks.Count > 0) {

				Console.WriteLine();

				foreach(Tuple<string, string, TaskType> task in tasks) 
					Console.WriteLine(tasks.IndexOf(task) + ". " + task.Item1 + " - " + task.Item2);

				Console.WriteLine();

			} else
				Console.WriteLine("Список пуст");
		}
	
		// Удаление задания
		public void deleteATask(int index) {
			if ((index >= 0) && (index < tasks.Count())) {
				tasks.RemoveAt(index);
				saveTasksList();
			} else 
				Console.WriteLine("Ты ввел недопустимое число");
		}
	
	}

}
