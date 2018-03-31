using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Microsoft.Activities.UnitTesting;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace WebTester
{
	public static class Extensions
	{
		private static int cnt = 0;
		private static Regex regex;
		private static MatchCollection matches;
		private static string oldScript = @"
var ua = window.navigator.userAgent;
if (ua.match(/PhantomJS/)) {
    return [{}];
} else {
    var performance =
        window.performance ||
        window.mozPerformance ||
        window.msPerformance ||
        window.webkitPerformance || {};

    if (ua.match(/Chrome/)) {
	/* provides page element-granula inforation */
        var network = performance.getEntries() || {};
        return network;
    } else {
	    /* Firefox and Internet Explorer provide load times of the whole page only */
        var timings = performance.timing || {};
        return [timings];
    }
}
";
		// see also: https://github.com/addyosmani/timing.js/blob/master/timing.js
		// for timings.loadTime,timings.domReadyTime  etc.
		private static string libraryScript = @"
            	
(function(window) {
  'use strict';
  window.timing = window.timing || {
    getTimes: function(opt) {
      var performance = window.performance ||
        window.webkitPerformance || window.msPerformance ||
        window.mozPerformance;

      if (performance === undefined) {
        return '';
      }
      var timings = performance.timing || {};
      // NOTE: legacy conversion
      if (opt && opt['stringify']) {
        return JSON.stringify(timings);
      } else {
        return timings;
      }

    },

    getNetwork: function(opt) {
      var network = performance.getEntries() || {};
      if (opt && opt['stringify']) {
        return JSON.stringify(network);
      } else {
        return network;
      }
    }
  }
})(typeof window !== 'undefined' ? window : {});";


		private static string performanceTimerScript = String.Format(
			                                               "{0}\nreturn window.timing.getTimes();", libraryScript);

		private static string performanceNetworkScript = String.Format(
			                                                 "{0}\nreturn window.timing.getNetwork({1});", libraryScript, "{stringify:true}");
		private static string performanceNetworkScriptNoStringify = String.Format(
		
			                                                            "{0}\nreturn window.timing.getNetwork();", libraryScript);
		/*				private static Boolean stringify;

		public static Boolean Stringify {
			get { return stringify; }
			set { stringify = value; }
		}
*/
		public static List<String> SplitDataRows(string payload, string matchPattern, int maxRows = 0)
		{
			List<String> result = new List<String>();
			String[] resultArray = { };
			regex = new Regex(matchPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
			matches = regex.Matches(payload);
			if (matches != null) {
				int rowNum = 0;
				resultArray = Regex.Split(payload, matchPattern);
				foreach (string arrItem in resultArray) {
					if (maxRows != 0 && rowNum++ > maxRows) {
						break;
					}
					result.Add(arrItem);
				}
			}
			return result;
		}

		public static T Execute<T>(this IWebDriver driver, string script)
		{
			return (T)((IJavaScriptExecutor)driver).ExecuteScript(script);
		}

		public static List<Dictionary<String, String>> Performance(this IWebDriver driver, Boolean stringify = false)
		{
			// NOTE: this code is highly browser-specific:
			// Chrome has performance.getEntries
			// FF only has performance.timing
			// PhantomJS does not have anything
			// System.InvalidOperationException: {"errorMessage":"undefined is not a constructor..

			String script = oldScript;
			List<Dictionary<String, String>> result = new List<Dictionary<string, string>>();
			var row = new Dictionary<String, String>();
			var dic = new Dictionary<String, Object>();
			if (stringify) { 
				script = performanceNetworkScript;
				// TODO: parse json
				// with old script,
				// System.InvalidCastException: Unable to cast object of type 'System.Collections.ObjectModel.ReadOnlyCollection`1[System.Object]' to type 'System.String'.

				String rawData = "";
				try {
					script = performanceNetworkScript;
					rawData = driver.Execute<String>(script);
					Console.Error.WriteLine("Raw data: \n{0}", rawData);
				} catch (System.InvalidCastException e) {
					Console.Error.WriteLine("Exception (ignored) :" + e.ToString());
				}
				int maxRows = 5;
				if (rawData.Length > 0) { 
					List<String> rawDataRows = SplitDataRows(rawData, "(?<=\\}) *, *(?=\\{)", maxRows);
					// List<Dictionary<String, String>> result2 = new List<Dictionary<string, string>>();
					Regex columnRegex = new Regex(" *, *", RegexOptions.IgnoreCase | RegexOptions.Compiled);
					foreach (String rawDataRow in rawDataRows) {
					
						String[] columnsArray = Regex.Split(Regex.Replace(rawDataRow, "[\\]{}\\[]", ""), " *, *");
						row = new Dictionary<String, String>();
						foreach (String columnData in columnsArray) {
							// TODO:  use matchers to prevent error with "name" column
							if (!(columnData.Contains("\"name\":"))) {
								String[] entryData = Regex.Split(columnData, " *: *");
								try {
									row.Add(Regex.Replace(entryData[0], "\"", ""), Regex.Replace(entryData[1], "\"", ""));
								} catch (IndexOutOfRangeException e) { 
									Console.WriteLine("Exception (ignored) in the entry: " + columnData);
								}
							}
						}
						result.Add(row);
					}
				}
			} else {
				script = performanceNetworkScriptNoStringify;
				IEnumerable<Object> rawObject = null;
				try {
					rawObject = driver.Execute<IEnumerable<Object>>(script);
				} catch (System.InvalidCastException e) {
					Console.Error.WriteLine("Exception (ignored) :" + e.ToString());
				}
				if (rawObject != null) {
					foreach (var element in (IEnumerable<Object>)rawObject) {
						row = new Dictionary<String, String>();
						dic = (Dictionary<String, Object>)element;
						foreach (object key in dic.Keys) {
							Object val = null;
							if (!dic.TryGetValue(key.ToString(), out val)) {
								val = "";
							}
							row.Add(key.ToString(), val.ToString());
						}
						result.Add(row);
					}
				}
			}
			return result;
		}

		public static void WaitJqueryInActive(this IWebDriver driver, int max_cnt = 10)
		{

			cnt = 0;
			// https://www.linkedin.com/grp/post/961927-6024383820957040643
			string script = @"
if (window.jQuery) {
    // jQuery is loaded
    return jQuery.active;
} else {
    // jQuery is not loaded
    return -1;
}
";
			var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(30.00));
			wait.PollingInterval = TimeSpan.FromSeconds(0.50);
			wait.Until(o => {
				Int64 result = o.Execute<Int64>(script);
				cnt++;
				Console.Error.WriteLine(String.Format("result = {0}", result));
				Console.Error.WriteLine(String.Format("cnt = {0}", cnt));
				cnt++;
				return ((result.Equals(0) || cnt > max_cnt));
			});
		}


		public static void WaitDocumentReadyState(this IWebDriver driver, string expected_state, int max_cnt = 10)
		{
			cnt = 0;
			string script = "return document.readyState";

			var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(30.00));
			wait.PollingInterval = TimeSpan.FromSeconds(0.50);
			wait.Until(o => {
				string result = o.Execute<String>(script);
				Console.Error.WriteLine(String.Format("result = {0}", result));
				Console.Error.WriteLine(String.Format("cnt = {0}", cnt));
				cnt++;
				return ((result.Equals(expected_state) || cnt > max_cnt));
			});
		}

		public static void WaitDocumentReadyState(this IWebDriver driver, string[] expected_states, int max_cnt = 10)
		{
			cnt = 0;
			string script = "return document.readyState";
			Regex state_regex = new Regex(String.Join("", "(?:", String.Join("|", expected_states), ")"),
				                    RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
			var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(30.00));
			wait.PollingInterval = TimeSpan.FromSeconds(0.50);
			wait.Until(o => {
				string result = o.Execute<String>(script);
				Console.Error.WriteLine(String.Format("result = {0}", result));
				cnt++;
				return ((state_regex.IsMatch(result) || cnt > max_cnt));
			});
		}
	}
}
