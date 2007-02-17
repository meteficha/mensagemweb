/*  Copyright (C) 2005-2007 Felipe Almeida Lessa
    
    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */
 
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

using MensagemWeb.Messages;
using MensagemWeb.Logging;

namespace MensagemWeb.Engines {
	public abstract class HttpEngine {
		// Contains the first part of the URLs that are going to be accessed. Can be
		// anything, from "" to "http://" and "http://www.xyz.com/abc/def?q=".
		protected abstract string BaseURL { get; }
		
		protected void Clear() {
			this.lastURL = null;
			this.cookies = null;
			this.aborted = false;
		}
		
		
		
		// These I did for you		
		protected string lastURL = null;
		private CookieCollection cookies = null;
		private HttpWebRequest webRequest = null;
		private bool aborted = false, abortable = true;
		
		public void Abort() {
			this.aborted = true;
			try {
				if (this.abortable) {
					this.webRequest.Abort();
					Logger.Log(this, "WebRequest aborted with (apparent) success!");
				} else
					Logger.Log(this, "WebRequest is not abortable.");
			} catch (Exception e) {
				Logger.Log(this, "Failed to abort the WebRequest:\n{0}", e);
			}
		}
		
		public bool Aborted {
			get { return this.aborted; }
		}
		
		
		private HttpWebRequest NewWebRequest(string address) {
			if (this.aborted)
				throw new Exception("Operation aborted.");
			string realAddr = BaseURL + address;
			lastURL = realAddr;
			this.webRequest = HttpWebRequest.Create(realAddr) as HttpWebRequest;
			this.webRequest.CookieContainer = new CookieContainer();
			if (this.cookies != null)
				this.webRequest.CookieContainer.Add(this.cookies);
			this.webRequest.Proxy = MensagemWeb.Config.ProxyConfig.GetProxy();
			this.webRequest.Timeout = 15000; // milliseconds
			this.webRequest.KeepAlive = true;
			this.webRequest.Pipelined = true;
			this.webRequest.AllowAutoRedirect = true;
			this.webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1)";
			this.webRequest.Accept = "text/xml,application/xml,application/xhtml+xml," +
				"text/html,text/plain,image/png,image/gif,*/*";
			return this.webRequest;
		}
		
		
		private void CloseWebResponse(HttpWebResponse response) {
			if (this.cookies == null)
				this.cookies = response.Cookies;
			else
				this.cookies.Add(response.Cookies);
			response.Close();
		}
		
		
		
		private static string PrepareIDictionary(IDictionary<string, string> data) {
			StringBuilder dataStr = new StringBuilder();
			foreach (string key in (IEnumerable<string>) data.Keys) {
				dataStr.Append(key);
				dataStr.Append('=');
				dataStr.Append(data[key]);
				dataStr.Append('&');
			}
			dataStr.Remove(dataStr.Length - 1, 1);
			return dataStr.ToString();
		}	
		
		protected Response Get(string address, string referer, IDictionary<string, string> args) {
			if (args != null) 
				address += "?" + PrepareIDictionary(args);
			HttpWebRequest webRequest = NewWebRequest(address);
			webRequest.Method = "GET";
			if (referer != null)
				webRequest.Referer = referer;
			HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse;
			return new Response(this, webResponse);
		}
		protected Response Get(string address, string referer) {
			return Get(address, referer, null);
		}
		protected Response Get(string address) {
			return Get(address, lastURL, null);
		}
		
		
		
		
		protected Response Post(string address, string referer, IDictionary<string, string> data) {
			// Get the bytes of the request
			byte[] dataBytes = Util.ToBytes(PrepareIDictionary(data).ToString());
			
			// Do the request
			HttpWebRequest webRequest = NewWebRequest(address);
			if (referer != null)
				webRequest.Referer = referer;
			webRequest.Method = "POST";
			webRequest.ContentType = "application/x-www-form-urlencoded";
			webRequest.ContentLength = dataBytes.Length;
			
			// Write the request
			Stream requestStream = webRequest.GetRequestStream();
			requestStream.Write(dataBytes, 0, dataBytes.Length);
			requestStream.Close();
			
			// Return the response
			HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse;
			return new Response(this, webResponse);
		}
		protected Response Post(string address, IDictionary<string, string> data) {
			return Post(address, lastURL, data);
		}
		
		
				// Class that represents a response, should be used with the using pattern. 
		protected sealed class Response : IDisposable {
			private HttpWebResponse resp;
			private HttpEngine parent;
			
			private string asString = null;
			
			internal Response(HttpEngine parent, HttpWebResponse resp) {
				if (resp == null)
					throw new ArgumentNullException("Response shouldn't be null.");
				this.parent = parent;
				this.resp = resp;
			}
			
			~Response() {
				(this as IDisposable).Dispose();
			}
			
			void IDisposable.Dispose() {
				if (resp != null) {
					parent.CloseWebResponse(resp);
					GC.SuppressFinalize(this);
				}
			}
			
			public string Text {
				get {
					if (parent.aborted)
						throw new WebException("Aborted");
					if (asString == null) {
						if (resp == null)
							throw new ArgumentException("Can't take response as text anymore.");
						parent.abortable = false;
						try {
							using (Stream responseStream = resp.GetResponseStream())
								using (StreamReader reader = new StreamReader(responseStream))
									asString = reader.ReadToEnd();
						} finally {
							parent.abortable = true;
						}
					}
					
					return asString;
				}
			}
			
			public void CallVerification(VerificationDelegate callback) {
				if (resp == null)
					throw new ArgumentException("Can't take call verification anymore.");
				parent.abortable = false;
				try {
					if (parent.aborted)
						throw new WebException("Aborted");
					using (Stream stream = resp.GetResponseStream())
						callback(stream);
				} finally {
					parent.abortable = true;
				}
			}
		} // class Response...
	} // class HttpEngine...
} // namespace
