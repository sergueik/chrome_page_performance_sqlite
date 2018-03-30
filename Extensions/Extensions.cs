using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
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

            string oldScript = @"
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
            string libraryScript = @"
            	
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
            string performanceTimerScript = String.Format(
                                                "{0}\nreturn window.timing.getTimes();", libraryScript);

            string performanceNetworkScript = String.Format(
                                                  "{0}\nreturn window.timing.getNetwork({1});", libraryScript, "{stringify:true}");
            string performanceNetworkScriptNoStringify = String.Format(
                                                             "{0}\nreturn window.timing.getNetwork();", libraryScript);
            string script = performanceNetworkScriptNoStringify;
            // script = oldScript;
            List<Dictionary<String, String>> result = new List<Dictionary<string, string>>();
            // raw data example :
            string rawDataExample = @"[{""name"":""https://d.agkn.com/pixel/9235/?che=404249&aid=1D003C736B769D8A-15E566A80D0A9C3B"",""entryType"":""resource"",""startTime"":10767.800000030547,""duration"":798.3999999705702,""initiatorType"":""img"",""nextHopProtocol"":""http/1.1"",""workerStart"":0,""redirectStart"":0,""redirectEnd"":0,""fetchStart"":10767.800000030547,""domainLookupStart"":0,""domainLookupEnd"":0,""connectStart"":0,""connectEnd"":0,""secureConnectionStart"":0,""requestStart"":0,""responseStart"":0,""responseEnd"":11566.200000001118,""transferSize"":0,""encodedBodySize"":0,""decodedBodySize"":0,""serverTiming"":[]},{""name"":""https://www.facebook.com/tr?id=149941242310235&ev=PageView&cd[order_id]=1D003C736B769D8A-15E566A80D0A9C3B"",""entryType"":""resource"",""startTime"":10768.79999996163,""duration"":830.0000000745058,""initiatorType"":""img"",""nextHopProtocol"":""h2"",""workerStart"":0,""redirectStart"":0,""redirectEnd"":0,""fetchStart"":10768.79999996163,""domainLookupStart"":0,""domainLookupEnd"":0,""connectStart"":0,""connectEnd"":0,""secureConnectionStart"":0,""requestStart"":0,""responseStart"":0,""responseEnd"":11598.800000036135,""transferSize"":0,""encodedBodySize"":0,""decodedBodySize"":0,""serverTiming"":[]} ,  {""name"":""https://secure.insightexpressai.com/adServer/adServerESI.aspx?script=false&bannerID=2273552&redir=https://secure.insightexpressai.com/adserver/1pixel.gif&rnd=19388"",""entryType"":""resource"",""startTime"":10769.399999990128,""duration"":984.3999999575317,""initiatorType"":""img"",""nextHopProtocol"":""http/1.1"",""workerStart"":0,""redirectStart"":0,""redirectEnd"":0,""fetchStart"":10769.399999990128,""domainLookupStart"":0,""domainLookupEnd"":0,""connectStart"":0,""connectEnd"":0,""secureConnectionStart"":0,""requestStart"":0,""responseStart"":0,""responseEnd"":11753.79999994766,""transferSize"":0,""encodedBodySize"":0,""decodedBodySize"":0,""serverTiming"":[]}, {""name"":""https://logx.optimizely.com/v1/events"",""entryType"":""resource"",""startTime"":10851.400000043213,""duration"":547.7999999420717,""initiatorType"":""xmlhttprequest"",""nextHopProtocol"":""http/1.1"",""workerStart"":0,""redirectStart"":0,""redirectEnd"":0,""fetchStart"":10851.400000043213,""domainLookupStart"":10851.400000043213,""domainLookupEnd"":10851.400000043213,""connectStart"":10851.400000043213,""connectEnd"":10851.400000043213,""secureConnectionStart"":0,""requestStart"":10915.099999983795,""responseStart"":11010.000000009313,""responseEnd"":11399.199999985285,""transferSize"":339,""encodedBodySize"":0,""decodedBodySize"":0,""serverTiming"":[]}, {""name"":""https://script.crazyegg.com/pages/scripts/0028/5728.js?422902"",""entryType"":""resource"",""startTime"":10974.200000055134,""duration"":629.3999999761581,""initiatorType"":""script"",""nextHopProtocol"":""http/1.1"",""workerStart"":0,""redirectStart"":0,""redirectEnd"":0,""fetchStart"":10974.200000055134,""domainLookupStart"":0,""domainLookupEnd"":0,""connectStart"":0,""connectEnd"":0,""secureConnectionStart"":0,""requestStart"":0,""responseStart"":0,""responseEnd"":11603.600000031292,""transferSize"":0,""encodedBodySize"":0,""decodedBodySize"":0,""serverTiming"":[]},	{""name"":""https://datacloud.tealiumiq.com/tealium_ttd/main/16/i.js?jsonp=utag.ut.tealium_pass_ttdid"",""entryType"":""resource"",""startTime"":10989.500000025146,""duration"":912.2999999672174,""initiatorType"":""script"",""nextHopProtocol"":""http/1.1"",""workerStart"":0,""redirectStart"":0,""redirectEnd"":0,""fetchStart"":10989.500000025146,""domainLookupStart"":0,""domainLookupEnd"":0,""connectStart"":0,""connectEnd"":0,""secureConnectionStart"":0,""requestStart"":0,""responseStart"":0,""responseEnd"":11901.799999992363,""transferSize"":0,""encodedBodySize"":0,""decodedBodySize"":0,""serverTiming"":[]}]";
            // with old script,
            // System.InvalidCastException: Unable to cast object of type 'System.Collections.ObjectModel.ReadOnlyCollection`1[System.Object]' to type 'System.String'.

            /*
            String rawData = "";
            try {
                rawData = driver.Execute<String>(performanceNetworkScript );
            Console.Error.WriteLine("Raw data: \n{0}", rawData);
        } catch (System.InvalidCastException e) {
            Console.Error.WriteLine("Exception (ignored) :" + e.ToString());
        }
        */
            // TODO: parser json
            IEnumerable<Object> rawObject = null;
            try
            {
                rawObject = driver.Execute<IEnumerable<Object>>(script);
            }
            catch (System.InvalidCastException e)
            {
                Console.Error.WriteLine("Exception (ignored) :" + e.ToString());
            }
            if (rawObject != null)
            {
                foreach (var element in (IEnumerable<Object>)rawObject)
                {
                    var row = new Dictionary<String, String>();
                    var dic = (Dictionary<String, Object>)element;
                    foreach (object key in dic.Keys)
                    {
                        Object val = null;
                        if (!dic.TryGetValue(key.ToString(), out val))
                        {
                            val = "";
                        }
                        row.Add(key.ToString(), val.ToString());
                    }
                    result.Add(row);
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
            wait.Until(o =>
            {
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
            wait.Until(o =>
            {
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
            wait.Until(o =>
            {
                string result = o.Execute<String>(script);
                Console.Error.WriteLine(String.Format("result = {0}", result));
                cnt++;
                return ((state_regex.IsMatch(result) || cnt > max_cnt));
            });
        }
    }
}
