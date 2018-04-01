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
		public List<string> serverTiming { get; set; }
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
		override
		public String ToString(){
			return  "name: " + this.name + "\nduration:" + this.duration;
		}
	}
}