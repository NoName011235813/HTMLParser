using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.IO;

namespace httpEater {

	class Program {

		static class Maoh {
			
			public static void SayWelcome() {
				Console.WriteLine("Добро пожаловать в HTTPEater v 1.1");
				Console.WriteLine("Мао на связи \">.<");
				Console.WriteLine();
			}

			public static void SayGoodBye() {
				Console.WriteLine("Удачки!");
				Console.ReadKey();
			}

			public static void SayInputAString() {
				Console.WriteLine("Введи число, умник");
			}

			public static void SayWaitASecond() {
				Console.WriteLine("Секунду...");
			}

			public static void SayThatsNotAnUrl() {
				Console.WriteLine("Это плохая ссылка.. Решил обмануть меня?");
			}

			public static void SayGoForHelp() {
				Console.WriteLine("Ты ввел какое-то странное заклинание, напиши help, если тебе нужна помощь");
			}


			public static void Help() {
				Console.WriteLine();
				Console.WriteLine("Хм, ну вот смотри");
				Console.WriteLine("login - для ввода логина");
				Console.WriteLine("password - для ввода пароля");
				Console.WriteLine("auth - для входа в систему");
				Console.WriteLine("view tasks - для просмотра списка заданий");
				Console.WriteLine("view teachers - для просмотра списка учителей");
				Console.WriteLine("monitor tasks - для отслеживания результатов работы");
				Console.WriteLine("monitor teachers - для отслеживания онлайна учиетелй");
				Console.WriteLine("add task - для добавления задачи");
				Console.WriteLine("add teacher - для добавления учителя");
				Console.WriteLine("delete task - для удаления задачи");
				Console.WriteLine("delete teacher - для удаления учителя");
				Console.WriteLine("exit - ну не знаю, попытайся догадаться сам, может быть для аннигиляции всего живого?");
				Console.WriteLine("И запомни не надо ничего дописать к командам, просто напиши что я тебе объяснила");
				Console.WriteLine();
			}

		}

		static void Main(string[] args) {

			Maoh.SayWelcome();

            bool working = true;

			webUser user = new webUser();

			while (working) { 
                
                Console.Write("> ");
                string answ = Console.ReadLine();

				// Авторизирование
				if (answ == "auth") {

					Maoh.SayWaitASecond();

					user.auth();

				// Ввод логина
				} else if (answ == "login") {
					
					Console.Write("login> ");
					user.login = Console.ReadLine();
					user.saveLoginAndPassword();

				// Ввод пароля 
				} else if (answ == "password") {

					Console.Write("password> ");
					user.password = Console.ReadLine();
					user.saveLoginAndPassword();

				// Выход
				} else if (answ == "exit") {

					working = false;

				// Просмотр списка учителей
				} else if (answ == "view teachers") {
					
					user.viewTeacherList();

				// Добавление ссылки учителя
				} else if (answ == "add teacher") {

					Console.Write("teacher_url> ");
					string url = Console.ReadLine();

					if (user.CheckUrlForELearning(url, webUser.UrlType.Teacher))
						user.addTeacherUrl(url);
					else
						Maoh.SayThatsNotAnUrl();

				// Удаление учителя
				} else if (answ == "delete teacher") {
					
					user.viewTeacherList();
					
					if (user.getTeachersCount() > 0) {

						Console.Write("teacher_index> ");
						int index;
						string input = Console.ReadLine();

						if (int.TryParse(input, out index)) {
							user.removeATeacher(index);
						} else
							Maoh.SayInputAString();
					}

				// Мониторинг учителей
				} else if (answ == "monitor teachers") {
					
					user.monitorTeachers();

				// Добапвление задачи
				} else if (answ == "add task") {
					Console.Write("task_url> ");
					string url = Console.ReadLine();

					if (user.CheckUrlForELearning(url, webUser.UrlType.Quiz) || user.CheckUrlForELearning(url, webUser.UrlType.Task))
						user.addTaskUrl(url);
					else
						Maoh.SayThatsNotAnUrl();

				// Удаление задачи
				} else if (answ == "delete task") {
					
					user.viewTaskList();
					
					if (user.getTaskCount() > 0) {

						Console.Write("task_index> ");
						int index;
						string input = Console.ReadLine();

						if (int.TryParse(input, out index)) {
							user.deleteATask(index);
						} else
							Maoh.SayInputAString();
					}
				
				// Просмотр списка заданий
				} else if (answ == "view tasks") {
					
					user.viewTaskList();
				

				// Мониторинг задач
				} else if (answ == "monitor tasks") {
					
					user.monitorTasks();

				} else if (answ == "help")

					Maoh.Help();

				else

					Maoh.SayGoForHelp();
				
            }

			Maoh.SayGoodBye();

		}
	}
}
