using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Microsoft.Activities.UnitTesting;
using System.Data.SQLite;
using SQLite.Utils;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
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

		private static string databaseName = "performance.db";

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
		#pragma warning disable 169
		private static Object forever;
		#pragma warning restore 169
		private static Boolean waiting = false;
		private static Boolean useRemoteDriver = false;

		public static Boolean UseRemoteDriver {
			get { return useRemoteDriver; }
			set { useRemoteDriver = value; }
		}
		private static Boolean useHeadlessDriver = false;

		public static Boolean UseHeadlessDriver {
			get { return useHeadlessDriver; }
			set { useHeadlessDriver = value; }
		}

		[TestInitialize]
		public void Initialize() {
			database = String.Format(@"{0}\{1}", dataFolderPath, databaseName);
			dataSource = "data source=" + database;
			tableName = "product";
			createTable();
			if (useRemoteDriver) {
				var requested = new Dictionary<string,object>();
				requested.Add("browserName","chrome");
				requested.Add("version",string.Empty);
				requested.Add("platform", "windows");
				requested.Add("javaScript",true);
				#pragma warning disable 618
				// https://stackoverflow.com/questions/70366741/desiredcapabilities-is-inaccessible-due-to-its-protection-level
				// https://support.smartbear.com/crossbrowsertesting/docs/automated-testing/frameworks/selenium/about/use-of-desiredcapabilities-has-been-deprecated.html
				
				
				var chromeOptions = new ChromeOptions();

				chromeOptions.PlatformName = "Windows";
				chromeOptions.BrowserVersion = "109";
				// chromeOptions.AddAdditionalCapability("javaScript", true, true);

				selenium_driver = new RemoteWebDriver(new Uri(hub_url), chromeOptions);

			//	var desiredCapabilities =
			//		new DesiredCapabilities(requested);
				/*
				 	SharpDevelopdoes not recognize VS 7 syntax
					new DesiredCapabilities(
						new Dictionary<string,object>(){
							["browserName","chrome"],
							["version",string.Empty],
							["platform", "windows"],
							["javaScript",true]
						});
				*/
				// selenium_driver = new RemoteWebDriver(new Uri(hub_url), desiredCapabilities);
				#pragma warning restore 618
			} else {
				if (useHeadlessDriver) {
					var chromeOptions = new ChromeOptions();
					chromeOptions.AddArgument("--headless");
					chromeOptions.AddArgument("--window-size=1200x800");
					chromeOptions.AddArgument("--disable-gpu");
					selenium_driver = new ChromeDriver(chromeOptions);
				} else {
					selenium_driver = new ChromeDriver();
					/*
					#region Edge-specific
					try {
						// location for MicrosoftWebDriver.exe
						string serverPath = System.IO.Directory.GetCurrentDirectory();
						EdgeOptions edgeOptions = new EdgeOptions();
						System.Environment.SetEnvironmentVariable("webdriver.edge.driver", String.Format(@"{0}\MicrosoftWebDriver.exe", serverPath));
						edgeOptions.PageLoadStrategy = (PageLoadStrategy)EdgePageLoadStrategy.Eager;
						selenium_driver = new EdgeDriver(serverPath, edgeOptions);
					} catch (Exception e) { 
						Console.WriteLine(e.Message); 
					} finally {
					} 
					#endregion
					*/
				}
			}
			// Set page load timeout to 5 seconds
			selenium_driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(5);
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
			String performanceScript = GetScriptContent(@"Data\timing.min.js");
			List<Dictionary<String, String>> result = selenium_driver.Performance(performanceScript);
			// experimental - process JSON.stringify(timings)
			// result = selenium_driver.Performance(true);
			result = selenium_driver.Performance(performanceScript, false);
			var dic = new Dictionary<string, object>();
			var urls = new List<string>();

			foreach (var row in result) {
				dic["caption"] = "dummy";
				foreach (string key in row.Keys) {
					if (Regex.IsMatch(key, "(name|duration)")) {
						Console.Error.WriteLine(key + " " + row[key]);
						// Type of conditional expression cannot be determined because there is no implicit conversion between 'double' and 'string' (CS0173)
						dic[key] = (key.Equals("duration")) ? 
							(Object)((Double)Double.Parse(row[key])):
							(Object)((String)row[key]);
					}
				}
				String url = dic["name"].ToString();
				if (!urls.Contains(url)) {
					urls.Add(url);
					insert(dic);
				} else {
					Console.Error.WriteLine("Skipping duplicate url: " + url);
				}
			}

			foreach (string key in dic.Keys.ToArray()) {
					dic[key] = null;
			}
			Console.Error.WriteLine("");
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
				Dictionary<string,object> requested = new Dictionary<string,object>();
				requested.Add("browserName","chrome");
				requested.Add("version",string.Empty);
				requested.Add("platform", "windows");
				requested.Add("javaScript",true);
				#pragma warning disable 618
				var chromeOptions = new ChromeOptions();

				chromeOptions.PlatformName = "Windows";
				chromeOptions.BrowserVersion = "109";
				// chromeOptions.AddAdditionalCapability("javaScript", true, true);

				selenium_driver = new RemoteWebDriver(new Uri(hub_url), chromeOptions);


				
				// var desiredCapabilities =
				//	new DesiredCapabilities(requested);
				/*
				 	SharpDevelopdoes not recognize VS 7 syntax
					new DesiredCapabilities(
						new Dictionary<string,object>(){
							["browserName","chrome"],
							["version",string.Empty],
							["platform", "windows"],
							["javaScript",true]
						});
				*/
				
				// selenium_driver = new RemoteWebDriver(new Uri(hub_url), desiredCapabilities);
				#pragma warning restore 618
				// 'OpenQA.Selenium.Remote.DesiredCapabilities' does not contain a definition for 'Chrome' (CS0117)
				// selenium_driver = new RemoteWebDriver(new Uri(hub_url), DesiredCapabilities.Chrome());
			} else {
				if (useHeadlessDriver) {
					var chromeOptions = new ChromeOptions();
					chromeOptions.AddArgument("--headless");
					chromeOptions.AddArgument("--window-size=1200x800");
					chromeOptions.AddArgument("--disable-gpu");
					selenium_driver = new ChromeDriver(chromeOptions);
				} else {
					selenium_driver = new ChromeDriver();
					/*
					#region Edge-specific
					try {
						// location for MicrosoftWebDriver.exe
						string serverPath = System.IO.Directory.GetCurrentDirectory();
						EdgeOptions edgeOptions = new EdgeOptions();
						System.Environment.SetEnvironmentVariable("webdriver.edge.driver", String.Format(@"{0}\MicrosoftWebDriver.exe", serverPath));
						edgeOptions.PageLoadStrategy = (PageLoadStrategy)EdgePageLoadStrategy.Eager;
						selenium_driver = new EdgeDriver(serverPath, edgeOptions);
					} catch (Exception e) { 
						Console.WriteLine(e.Message); 
					} finally {
					} 
					#endregion
					*/
				}	
			}
			selenium_driver.Manage().Timeouts().ImplicitWait = new System.TimeSpan(0, 0, 30);

			selenium_driver.Navigate().GoToUrl(baseURL);
			// selenium_driver.WaitJqueryInActive();
			// selenium_driver.WaitDocumentReadyState(expected_states[0]);
			selenium_driver.WaitDocumentReadyState(expected_states);

			// experimental - process JSON.stringify(timings)
			String performanceScript = GetScriptContent(@"Data\timing.min.js");
			List<Dictionary<String, String>> result = selenium_driver.Performance(performanceScript, true);
			// List<Dictionary<String, String>> result = selenium_driver.Performance();
			var dic = new Dictionary<string, object>();
			var urls = new List<string>();

			foreach (var row in result) {
				dic["caption"] = "dummy";
				foreach (string key in row.Keys) {
					if (Regex.IsMatch(key, "(name|duration)")) {
						// NOTE: duplicate urls will be dropped
						Console.Error.WriteLine(key + " " + row[key]);						
						// Type of conditional expression cannot be determined because there is no implicit conversion between 'double' and 'string' (CS0173)
						dic[key] = (key.Equals("duration")) ? 
							(Object)((Double)Double.Parse(row[key])):
							(Object)((String)row[key]);
						/*
							if (key.IndexOf("duration") > -1) {
								dic[key] = (Double)Double.Parse(row[key]);
							} else {
								dic[key] = (String)row[key];
							}
						*/
					}
				}
				String url = dic["name"].ToString();
				if (!urls.Contains(url)) {
					urls.Add(url);
					insert(dic);
				} else {
					Console.Error.WriteLine("Skipping duplicate url: " + url);
				}

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

		protected static String GetScriptContent(String scriptName) {
			String path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), scriptName);
			String[] lines = File.ReadAllLines(path);
			return String.Join("\n", lines);
		}
		
		public static void createTable() {
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
		static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e) {
			Console.WriteLine("Stopping");
			System.Threading.Thread.Sleep(1);
			e.Cancel = true;
			waiting = false;
			// System.Threading.Monitor.Exit(forever);
		}
	}
}