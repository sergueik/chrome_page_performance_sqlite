using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Microsoft.Activities.UnitTesting;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.PhantomJS;

namespace WebTester
{
    public static class Extensions
    {
        static int cnt = 0;

        public static T Execute<T>(this IWebDriver driver, string script)
        {
            return (T)((IJavaScriptExecutor)driver).ExecuteScript(script);
        }

        public static List<Dictionary<String, String>> Performance(this IWebDriver driver)
        {
            // NOTE: this code is highly browser-specific: 
            // Chrome has performance.getEntries 
            // FF only has performance.timing 
            // PhantomJS does not have anything
            // System.InvalidOperationException: {"errorMessage":"undefined is not a constructor..

            string performance_script = @"
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
        var network = performance.getEntries() || {};
        return network;
    } else {
        var timings = performance.timing || {};
        return [timings];
    }
}
";
            List<Dictionary<String, String>> result = new List<Dictionary<string, string>>();
            IEnumerable<Object> raw_data = driver.Execute<IEnumerable<Object>>(performance_script);

            foreach (var element in (IEnumerable<Object>)raw_data)
            {
                Dictionary<String, String> row = new Dictionary<String, String>();
                Dictionary<String, Object> dic = (Dictionary<String, Object>)element;
                foreach (object key in dic.Keys)
                {
                    Object val = null;
                    if (!dic.TryGetValue(key.ToString(), out val)) { val = ""; }
                    row.Add(key.ToString(), val.ToString());
                }
                result.Add(row);
            }
            return result;
        }

        public static void WaitDocumentReadyState(this IWebDriver driver, string expected_state, int max_cnt = 10)
        {
            cnt = 0;
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(30.00));
            wait.PollingInterval = TimeSpan.FromSeconds(0.50);
            wait.Until(dummy =>
            {
                string result = driver.Execute<String>("return document.readyState").ToString();
                Console.Error.WriteLine(String.Format("result = {0}", result));
                Console.Error.WriteLine(String.Format("cnt = {0}", cnt));
                cnt++;
                // TODO: match
                return ((result.Equals(expected_state) || cnt > max_cnt));
            });
        }

        public static void WaitDocumentReadyState(this IWebDriver driver, string[] expected_states, int max_cnt = 10)
        {
            cnt = 0;
            Regex state_regex = new Regex(String.Join("", "(?:", String.Join("|", expected_states), ")"),
                                          RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(30.00));
            wait.PollingInterval = TimeSpan.FromSeconds(0.50);
            wait.Until(dummy =>
            {
                string result = driver.Execute<String>("return document.readyState").ToString();
                Console.Error.WriteLine(String.Format("result = {0}", result));
                cnt++;
                return ((state_regex.IsMatch(result) || cnt > max_cnt));
            });
        }
    }
}
