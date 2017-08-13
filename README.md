### Chrome Page Element Performance Collector

Application demonstrates collecting details of the web navigation from the Chrome browser.
The results are stored into the SQLite database allowing measuring the Page performance at the individual Page element level.

By default the Chrome browser is run in [headless](http://executeautomation.com/blog/running-chrome-in-headless-mode-with-selenium-c/) mode.
Build copies the recent `chromedriver.exe` to application `bin` directory.
Make sure to upgrade your Chrome browser to build 60 or later.
To make browser window visible, change `useHeadlessDriver` to `false`

### Writing Tests

Include `Program.cs` into your project and merge `nuget.config` with yours. There are two extension methods : `WaitDocumentReadyState` and `Performance`
You may need to place `chromedriver.exe` in the `$PATH`

### Note
This application is not browser-agnostic, and is fully functional for Chrome only:  Chrome has `performance.getEntries`, while barebones Firefox only has `performance.timing` - with no further details.
For better results with Firefox, one needs to install [Firebug](https://getfirebug.com/releases/) and [netExport](https://getfirebug.com/releases/netexport/) add-ons into the profile the test is run. It is unknown if PhantomJS supports the same - currenty a stub is used.

The following Javascript is run to collect performance results while the page is being loaded:

    (
    window.performance ||
    window.mozPerformance ||
    window.msPerformance ||
    window.webkitPerformance
    ).getEntries()

The script is invoked when browser reports certain `document.readyState`.

![data.db](https://github.com/sergueik/chrome_page_performance_sqlite/blob/master/screenshots/data.png)


The following headers are selected to go to the SQLite database `data.db`:

 * id
 * url
 * duration

The [browsermob-proxy](https://github.com/lightbody/browsermob-proxy) offers similar functionality for Java - see e.g. [http://amormoeba.blogspot.com/2014/02/how-to-use-browser-mob-proxy.html][http://amormoeba.blogspot.com/2014/02/how-to-use-browser-mob-proxy.html]

### Author
[Serguei Kouzmine](kouzmine_serguei@yahoo.com)
