### Chrome Page Element Performance Collector

This application demonstrates collecting details of the web navigation from the Chrome browser.
It ws started before the [Navigation Timing API](https://w3c.github.io/perf-timing-primer/) has become the W2C standard published in October 2017,
and support of [Navigational Timing](https://developer.mozilla.org/en-US/docs/Web/API/Navigation_timing_API)
and [User Timing API](https://developer.mozilla.org/en-US/docs/Web/API/User_Timing_API) by Firefox browser was documented,
and the subject was explained in detail by several vendors  e.g. [Teleric](https://developer.telerik.com/featured/introduction-navigation-timing-api/) or [GoDaddy]().

The results are stored into the SQLite database allowing measuring the Page performance at the individual Page element level.

By default the Chrome browser is run in [headless](http://executeautomation.com/blog/running-chrome-in-headless-mode-with-selenium-c/) mode.
Build copies the recent `chromedriver.exe` to application `bin` directory.
Make sure to upgrade your Chrome browser to build 60 or later.
To make browser window visible, change `useHeadlessDriver` to `false`

### Writing Tests

Include `Program.cs` into your project and merge `nuget.config` with yours. There are two extension methods : `WaitDocumentReadyState` and `Performance`
You may need to place `chromedriver.exe` in the `$PATH`

### Note
This application is not yet browser-agnostic, and is fully functional for Chrome only:  Chrome has `performance.getEntries`, while barebones Firefox only has `performance.timing` - with no further details.
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
The script is invoked when browser reports certain `document.readyState`.

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
* GoDaddy page timing processing [page timing API](https://github.com/godaddy/timings) and [client API](https://github.com/godaddy/timings-client-py)
* [Web Performance Timing API Chrome extention](https://github.com/GregM/performance-timing-google-chrome-extension), also published as Chrome extention # nllipdabkglnhmanndddgcihbcmjpfej

### Author
[Serguei Kouzmine](kouzmine_serguei@yahoo.com)
