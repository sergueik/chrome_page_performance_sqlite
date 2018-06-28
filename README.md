### Info

The __Chrome Page Element Performance Collector__ application demonstrates collecting details of the web navigation from the Chrome browser.

The project was originally developed before the [Navigation Timing API](https://w3c.github.io/perf-timing-primer/) has become the W2C standard published in October 2017,
and support of [Navigational Timing](https://developer.mozilla.org/en-US/docs/Web/API/Navigation_timing_API)
and [User Timing API](https://developer.mozilla.org/en-US/docs/Web/API/User_Timing_API) by Firefox browser was documented,
and the subject was explained in detail by several vendors  e.g. [Teleric](https://developer.telerik.com/featured/introduction-navigation-timing-api/) or [GoDaddy]().

The results are saved into the SQLite database allowing statistical evaluation of the Page performance at the *Page element* level.

By default Chrome browser is run in [headless](http://executeautomation.com/blog/running-chrome-in-headless-mode-with-selenium-c/) mode.
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
This application is not yet browser-agnostic, and is only fully functional for Chrome:  Chrome has `performance.getEntries`, while barebones Firefox only has `performance.timing` - with no further details.

See also the [Java port of this project](https://github.com/sergueik/chrome_page_performance_sqlite_java).

For better results with Firefox, one needs to install [Firebug](https://getfirebug.com/releases/) and [netExport](https://getfirebug.com/releases/netexport/) add-ons into the profile the test is run. It is unknown if PhantomJS supports the same - currenty a stub is used.

The following Javascript is run to collect performance results while the page is being loaded:
```javascript
    (
    window.performance ||
    window.mozPerformance ||
    window.msPerformance ||
    window.webkitPerformance
    ).getEntries()
```
Alternaively the more advanced `timing.js` script from [Addy Osmani's repository](https://github.com/addyosmani/timing.js/blob/master/timing.js
) is called, and the results are deserialized into a `List<Dictionary<String, String>>` either implicitly by cast of directly parsing of the `JSON.stringify()` returned value.

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

  * [browsermob-proxy](https://github.com/lightbody/browsermob-proxy) offers similar functionality for Java - see e.g. [http://amormoeba.blogspot.com/2014/02/how-to-use-browser-mob-proxy.html][http://amormoeba.blogspot.com/2014/02/how-to-use-browser-mob-proxy.html]
  * [GoDaddy page timing API](https://github.com/godaddy/timings) and [client API](https://github.com/godaddy/timings-client-py)
  * [Web Performance Timing API Chrome extention](https://github.com/GregM/performance-timing-google-chrome-extension), also published as Chrome extention # nllipdabkglnhmanndddgcihbcmjpfej
  * [CSV/Excel File Parser - A Revisit](https://www.codeproject.com/Articles/1238464/CSV-Excel-File-Parser-A-Revisitx`)
  * [Measuring Page Load Speed with Navigation Timing](https://www.html5rocks.com/en/tutorials/webperformance/basics/)
  * [Internet Explorer Developer Tools User Interface Reference] (https://msdn.microsoft.com/en-us/library/dd565626(v=vs.85).aspx)

### License
This project is licensed under the terms of the MIT license.

### Changelog

  * version 1.1.x.x of the assembly is compiled with .net 4.5
  * version 1.0.x.x is compiled with .net 4.0

### Author
[Serguei Kouzmine](kouzmine_serguei@yahoo.com)
