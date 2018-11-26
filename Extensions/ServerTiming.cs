using System;
using System.Collections;
using System.Collections.Generic;

// https://ma.ttias.be/server-timings-chrome-devtools/
// https://www.w3.org/TR/server-timing/

namespace WebTester {
	public  class ServerTiming {
		public string name { get; set; }
		public string description { get; set; }
		public double duration { get; set; }
		public ServerTiming(String rawData){
			this.name = "";
			this.description = "";
			this.duration = 0.0;
		}
		override
		public String ToString(){
			return  "name: " + this.name + "\nduration:" + this.duration;
		}
	}
}

/*
// Single metric with description and value
// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Server-Timing
Server-Timing: cache;desc="Cache Read";dur=23.2
 */ 
