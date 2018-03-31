using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Microsoft.Activities.UnitTesting;
using System.Data.SQLite;
using SQLite.Utils;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace WebTester
{
	[TestClass]
	// [DeploymentItem(@"x86\SQLite.Interop.dll", "x86")]
	public class Monitor
	{
		private static string hub_url = "http://localhost:4444/wd/hub";
		public static string HubURL {
			get { return hub_url; }
			set { hub_url = value; }
		}
		private static IWebDriver selenium_driver;
		private static string baseURL = "http://www.carnival.com/";

		public static string BaseURL {
			get { return baseURL; }
			set { baseURL = value; }
		}

		private static string databaseName = "data.db";

		public static string DatabaseName {
			get { return databaseName; }
			set { databaseName = value; }
		}
		private static string dataFolderPath = Directory.GetCurrentDirectory();

		public static string DataFolderPath {
			get { return dataFolderPath; }
			set { dataFolderPath = value; }
		}


		private static string[] expected_states = { "interactive", "complete" };
		// unused
		// private static int max_cnt = 10;
		private static string tableName = "";
		private static string database;
		private static string dataSource;
		private static Object forever;
		private static Boolean waiting = false;
		private static Boolean useRemoteDriver = false;

		public static Boolean UseRemoteDriver {
			get { return useRemoteDriver; }
			set { useRemoteDriver = value; }
		}
		private static Boolean useHeadlessDriver = true;

		public static Boolean UseHeadlessDriver {
			get { return useHeadlessDriver; }
			set { useHeadlessDriver = value; }
		}

		[TestInitialize]
		public void Initialize()
		{
			database = String.Format(@"{0}\{1}", dataFolderPath, databaseName);
			dataSource = "data source=" + database;
			tableName = "product";
			createTable();
			if (useRemoteDriver) {
				selenium_driver = new RemoteWebDriver(new Uri(hub_url), DesiredCapabilities.Chrome());
			} else {
				if (useHeadlessDriver) {
					var chromeOptions = new ChromeOptions();
					chromeOptions.AddArgument("--headless");
					selenium_driver = new ChromeDriver(chromeOptions);
				} else {
					selenium_driver = new ChromeDriver();
				}
			}
			selenium_driver.Manage().Timeouts().ImplicitWait = new System.TimeSpan(0, 0, 30);
		}

		[TestCleanup]
		public void Cleanup()
		{
			if (selenium_driver != null)
				selenium_driver.Close();
		}

		[TestMethod]
		// [ExpectedException(typeof(NoSuchElementException))]
		public void Sample()
		{
			selenium_driver.Navigate().GoToUrl(baseURL);
			// selenium_driver.WaitDocumentReadyState(expected_states[0]);
			selenium_driver.WaitJqueryInActive();
			List<Dictionary<String, String>> result = selenium_driver.Performance();
			// experimental - process JSON.stringify(timings)
			// result = selenium_driver.Performance(true);
			result = selenium_driver.Performance(false);
			var dic = new Dictionary<string, object>();

			foreach (var row in result) {
				dic["caption"] = "dummy";
				foreach (string key in row.Keys) {
					if (Regex.IsMatch(key, "(name|duration)")) {
						Console.Error.WriteLine(key + " " + row[key]);
						if (key.IndexOf("duration") > -1) {
							dic[key] = (Double)Double.Parse(row[key]);
						} else {
							dic[key] = (String)row[key];
						}
					}
				}
				insert(dic);

				foreach (string key in dic.Keys.ToArray()) {
					dic[key] = null;
				}
				Console.Error.WriteLine("");
			}
		}
		public static void Main(string[] args)
		{
			dataFolderPath = Directory.GetCurrentDirectory();
			database = String.Format("{0}\\data.db", dataFolderPath);
			dataSource = "data source=" + database;
			tableName = "product";
			waiting = true;
			Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

			Console.WriteLine("Starting..");
			// TestConnection();
			createTable();
			if (useRemoteDriver) {
				selenium_driver = new RemoteWebDriver(new Uri(hub_url), DesiredCapabilities.Chrome());
			} else {
				if (useHeadlessDriver) {
					var chromeOptions = new ChromeOptions();
					chromeOptions.AddArgument("--headless");
					selenium_driver = new ChromeDriver(chromeOptions);
				} else {
					selenium_driver = new ChromeDriver();
				}
			}
			selenium_driver.Manage().Timeouts().ImplicitWait = new System.TimeSpan(0, 0, 30);

			selenium_driver.Navigate().GoToUrl(baseURL);
			// selenium_driver.WaitJqueryInActive();
			// selenium_driver.WaitDocumentReadyState(expected_states[0]);
			selenium_driver.WaitDocumentReadyState(expected_states);

			// experimental - process JSON.stringify(timings)
			List<Dictionary<String, String>> result = selenium_driver.Performance(true);
			// List<Dictionary<String, String>> result = selenium_driver.Performance();
			var dic = new Dictionary<string, object>();

			foreach (var row in result) {
				dic["caption"] = "dummy";
				foreach (string key in row.Keys) {
					if (Regex.IsMatch(key, "(name|duration)")) {
						Console.Error.WriteLine(key + " " + row[key]);
						if (key.IndexOf("duration") > -1) {
							dic[key] = (Double)Double.Parse(row[key]);
						} else {
							dic[key] = (String)row[key];
						}
					}
				}
				insert(dic);

				foreach (string key in dic.Keys.ToArray()) {
					dic[key] = null;
				}
				Console.Error.WriteLine("");
			}

			Console.Error.WriteLine("Press <CTRL-C> to quit program");

			// forever = new Object();
			// lock (forever) {
			// waiting = true;
			// }
			while (waiting) {
				// lock (forever)
				//{
				//    System.Threading.Monitor.Wait(forever);
				//}
			}
			Console.WriteLine("Stopped.");

			if (selenium_driver != null)
				try {
					selenium_driver.Close();
				} catch (WebDriverException) {
					// ignore
				}
			selenium_driver.Quit();
		}

		public static bool insert(Dictionary<string, object> dic)
		{
			try {
				using (SQLiteConnection conn = new SQLiteConnection(dataSource)) {
					using (SQLiteCommand cmd = new SQLiteCommand()) {
						cmd.Connection = conn;
						conn.Open();
						SQLiteHelper sh = new SQLiteHelper(cmd);
						sh.Insert(tableName, dic);
						conn.Close();
						return true;
					}
				}
			} catch (Exception ex) {
				Console.Error.WriteLine(ex.ToString());
				return false;
			}
		}

		public static void createTable()
		{
			using (SQLiteConnection conn = new SQLiteConnection(dataSource)) {
				using (SQLiteCommand cmd = new SQLiteCommand()) {
					cmd.Connection = conn;
					conn.Open();
					SQLiteHelper sh = new SQLiteHelper(cmd);
					sh.DropTable(tableName);

					SQLiteTable tb = new SQLiteTable(tableName);
					tb.Columns.Add(new SQLiteColumn("id", true)); // auto increment
					tb.Columns.Add(new SQLiteColumn("caption"));
					tb.Columns.Add(new SQLiteColumn("name"));
					tb.Columns.Add(new SQLiteColumn("duration", ColType.Decimal));
					sh.CreateTable(tb);
					conn.Close();
				}
			}
		}
		// https://stackoverflow.com/questions/203246/how-can-i-keep-a-console-open-until-cancelkeypress-event-is-fired
		// https://msdn.microsoft.com/en-us/library/system.console.cancelkeypress.aspx
		static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			Console.WriteLine("Stopping");
			System.Threading.Thread.Sleep(1);
			e.Cancel = true;
			waiting = false;
			// System.Threading.Monitor.Exit(forever);
		}
	}
}
