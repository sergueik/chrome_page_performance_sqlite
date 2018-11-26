using System;
using System.Collections;
using System.Collections.Generic;

namespace WebTester {
	public  class NetworkTiming {
		public string name { get; set; }
		public string entryType { get; set; }
		public double startTime { get; set; }
		public double duration { get; set; }
		public string initiatorType { get; set; }
		public string nextHopProtocol { get; set; }
		public double workerStart { get; set; }
		public double redirectStart { get; set; }
		public double redirectEnd { get; set; }
		public double fetchStart { get; set; }
		public double domainLookupStart { get; set; }
		public double domainLookupEnd { get; set; }
		public double connectStart { get; set; }
		public double connectEnd { get; set; }
		public double secureConnectionStart { get; set; }
		public double requestStart { get; set; }
		public double responseStart { get; set; }
		public double responseEnd { get; set; }
		public int transferSize { get; set; }
		public int encodedBodySize { get; set; }
		public int decodedBodySize { get; set; }
		public double unloadEventStart { get; set; }
		public int unloadEventEnd { get; set; }
		public double domInteractive { get; set; }
		public double domContentLoadedEventStart { get; set; }
		public double domContentLoadedEventEnd { get; set; }
		public double domComplete { get; set; }
		public double loadEventStart { get; set; }
		public double loadEventEnd { get; set; }
		public string type { get; set; }
		public int redirectCount { get; set; }
		public List<ServerTiming> serverTiming { get; set; }
		override
		public String ToString(){
			return  "name: " + this.name + "\nduration:" + this.duration;
		}
	}
}

/*
 {
  "name": "https://n2.mouseflow.com/b.gif?website=f448a37f-24c6-4f70-990f-97aeee
5968c8&session=d357ce4ec215a4179f147f4623d19726&page=112525925c9e7536532c41c2b56
ff64094ae66c6&gz=1",
  "entryType": "resource",
  "startTime": 11090.300000010757,
  "duration": 330.79999999608845,
  "initiatorType": "xmlhttprequest",
  "nextHopProtocol": "h2",
  "workerStart": 0,
  "redirectStart": 0,
  "redirectEnd": 0,
  "fetchStart": 11090.300000010757,
  "domainLookupStart": 0,
  "domainLookupEnd": 0,
  "connectStart": 0,
  "connectEnd": 0,
  "secureConnectionStart": 0,
  "requestStart": 0,
  "responseStart": 0,
  "responseEnd": 11421.100000006845,
  "transferSize": 0,
  "encodedBodySize": 0,
  "decodedBodySize": 0,
  "serverTiming": []
}
 */ 