### Info

The __Chrome Page Element Performance Collector__ application demonstrates collecting details of the web navigation from the Chrome browser.

The project was originally developed before the [Navigation Timing API](https://w3c.github.io/perf-timing-primer/) has become the W2C standard published in October 2017,
and support of [Navigational Timing](https://developer.mozilla.org/en-US/docs/Web/API/Navigation_timing_API)
and [User Timing API](https://developer.mozilla.org/en-US/docs/Web/API/User_Timing_API) by Firefox browser was documented,
and the subject was explained in detail by several vendors  e.g. [Teleric](https://developer.telerik.com/featured/introduction-navigation-timing-api/) or [GoDaddy]().

The results are saved into the SQLite database allowing statistical evaluation of the Page performance at the *Page element* level.

By default Chrome browser runs in [headless](http://executeautomation.com/blog/running-chrome-in-headless-mode-with-selenium-c/) mode.
Buildfile copies `chromedriver.exe` driver dependency to application's `bin` directory.
Make sure to upgrade your Chrome browser to build 60 or later.
To make browser window visible, change `useHeadlessDriver` to `false`

### Writing Tests

Include `Program.cs` into your project and merge `nuget.config` with yours. It will make available the extension methods `WaitDocumentReadyState`, `WaitJqueryInActive`` and `Performance`:
```c#
selenium_driver.WaitDocumentReadyState("complete");
selenium_driver.WaitJqueryInActive();
List<Dictionary<String, String>> result = selenium_driver.Performance();
```
You may need to place `chromedriver.exe` in the `$PATH`

### Note
This application is not browser-agnostic and is only fully functional for Chrome:  Chrome has `performance.getEntries`, while barebones Firefox only has `performance.timing` - with no further details.

See also the [Java port of this project](https://github.com/sergueik/chrome_page_performance_sqlite_java).

For better results with Firefox, one needs to install [Firebug](https://getfirebug.com/releases/) and [netExport](https://getfirebug.com/releases/netexport/) add-ons into the profile the test is run with. It is unknown if [PhantomJS](https://phantomjs.org) supports the same - currenty a stub is used.

The following Javascript [snippet](https://github.com/sergueik/chrome_page_performance_sqlite/blob/master/Data/timing.min.js) is injected into page 
and is run to collect performance results while the page is being loaded
A shorter version is:
```javascript
    (
    window.performance ||
    window.mozPerformance ||
    window.msPerformance ||
    window.webkitPerformance
    ).getEntries()
```
Alternaively the more advanced `timing.js` script


```javascript
(function(window) {
    "use strict";
    window.timing = window.timing || {
        getTimes: function(opts) {
            var performance = window.performance || window.webkitPerformance || window.msPerformance || window.mozPerformance;
            if (performance === undefined) {
                return false
            }
            var timing = performance.timing;
            var api = {};
            opts = opts || {};
            if (timing) {
                if (opts && !opts.simple) {
                    for (var k in timing) {
                        if (isNumeric(timing[k])) {
                            api[k] = parseFloat(timing[k])
                        }
                    }
                }
                if (api.firstPaint === undefined) {
                    var firstPaint = 0;
                    if (window.chrome && window.chrome.loadTimes) {
                        firstPaint = window.chrome.loadTimes().firstPaintTime * 1e3;
                        api.firstPaintTime = firstPaint - window.chrome.loadTimes().startLoadTime * 1e3
                    } else if (typeof window.performance.timing.msFirstPaint === "number") {
                        firstPaint = window.performance.timing.msFirstPaint;
                        api.firstPaintTime = firstPaint - window.performance.timing.navigationStart
                    }
                    if (opts && !opts.simple) {
                        api.firstPaint = firstPaint
                    }
                }
                api.loadTime = timing.loadEventEnd - timing.fetchStart;
                api.domReadyTime = timing.domComplete - timing.domInteractive;
                api.readyStart = timing.fetchStart - timing.navigationStart;
                api.redirectTime = timing.redirectEnd - timing.redirectStart;
                api.appcacheTime = timing.domainLookupStart - timing.fetchStart;
                api.unloadEventTime = timing.unloadEventEnd - timing.unloadEventStart;
                api.lookupDomainTime = timing.domainLookupEnd - timing.domainLookupStart;
                api.connectTime = timing.connectEnd - timing.connectStart;
                api.requestTime = timing.responseEnd - timing.requestStart;
                api.initDomTreeTime = timing.domInteractive - timing.responseEnd;
                api.loadEventTime = timing.loadEventEnd - timing.loadEventStart
            }
            return api
        },
        printTable: function(opts) {
            var table = {};
            var data = this.getTimes(opts) || {};
            Object.keys(data).sort().forEach(function(k) {
                table[k] = {
                    ms: data[k],
                    s: +(data[k] / 1e3).toFixed(2)
                }
            });
            console.table(table)
        },
        printSimpleTable: function() {
            this.printTable({
                simple: true
            })
        }
    };

    function isNumeric(n) {
        return !isNaN(parseFloat(n)) && isFinite(n)
    }
    if (typeof module !== "undefined" && module.exports) {
        module.exports = window.timing
    }
})(typeof window !== "undefined" ? window : {});
```

 from [Addy Osmani's repository](https://github.com/addyosmani/timing.js/blob/master/timing.js
) is called, and the results are deserialized into a `List<Dictionary<String, String>>` either implicitly via deserializaion 
```c#
List<Dictionary<String, String>> result = new List<Dictionary<string, string>>();
String rawData = driver.Execute<String>(performanceNetworkScript);
jsonNetworkTimingData = JsonConvert.DeserializeObject(rawData, typeof(List<NetworkTiming>)) as List<NetworkTiming>;
```
The NetwotkTiming [class](https://github.com/sergueik/chrome_page_performance_sqlite/blob/master/Extensions/NetworkTiming.cs) is a data type to load Chrome idl entries.

Another option is to directly parse of the `JSON.stringify()` returned value

```c#
IEnumerator<JToken> jsonDataTokenEnumerator = JArray.Parse(rawData).GetEnumerator();
  while (jsonDataTokenEnumerator.MoveNext()) {
    Dictionary<String, Object> dic = jsonDataTokenEnumerator.Current.ToObject(typeof(Dictionary<String,Object>)) as Dictionary<String, Object>;
							} catch (JsonReaderException) { // ignore
```

The script is invoked when browser reports certain `document.readyState` events that can be used to assume that page was fully loaded.

![data.db](https://github.com/sergueik/chrome_page_performance_sqlite/blob/master/screenshots/data.png)


The following headers are selected to go to the SQLite database `data.db`:

 * `id`
 * `url`
 * `duration`

### Work in progress

  * Refresh support to the [Firefox](http://kaaes.github.io/timing/) and [Internet EXplorer](https://msdn.microsoft.com/en-us/library/hh673552(v=vs.85).aspx).
  * Provide ElasticSearch bulk load via [ElasticSearch index API for Java](https://www.elastic.co/guide/en/elasticsearch/client/java-api/current/java-docs-index.html) and [ElasticSearch index API for C#](https://www.codeproject.com/Articles/1033116/A-Beginners-Tutorial-for-Understanding-and-Imple).

### See Also
  * [Chrome DevTools API](https://chromedevtools.github.io/devtools-protocol/tot/Performance/#method-getMetrics), also covered in [sergueik/selenium_cdp](https://github.com/sergueik/selenium_cdp/search?q=performance) and [cdp-webdriver](https://github.com/sergueik/cdp_webdriver/search?q=performance) 
  * [blog](http://www.amitrawat.tech/post/capturing-websocket-messages-using-selenium/) on filtering regular WebDriver Performance logs
  * [browsermob-proxy](https://github.com/lightbody/browsermob-proxy) offers similar functionality for Java - see e.g. [http://amormoeba.blogspot.com/2014/02/how-to-use-browser-mob-proxy.html][http://amormoeba.blogspot.com/2014/02/how-to-use-browser-mob-proxy.html]
  * [GoDaddy page timing API](https://github.com/godaddy/timings) and [client API](https://github.com/godaddy/timings-client-py)
  * [Web Performance Timing API Chrome extention](https://github.com/GregM/performance-timing-google-chrome-extension), also published as Chrome extention __# nllipdabkglnhmanndddgcihbcmjpfej__
  * [CSV/Excel File Parser - A Revisit](https://www.codeproject.com/Articles/1238464/CSV-Excel-File-Parser-A-Revisit)
  * [Measuring Page Load Speed with Navigation Timing](https://www.html5rocks.com/en/tutorials/webperformance/basics/)
  * [Internet Explorer Developer Tools User Interface Reference](https://msdn.microsoft.com/en-us/library/dd565626(v=vs.85).aspx)
  * [visualization](https://github.com/kaaes/timing) of __Navigation Timing object__ (in Javascript)
### License
This project is licensed under the terms of the MIT license.

### Changelog

  * version 1.1.x.x of the assembly is compiled with .net 4.5
  * version 1.0.x.x is compiled with .net 4.0

### Author
[Serguei Kouzmine](kouzmine_serguei@yahoo.com)
