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
using System.Text;

using MensagemWeb.Messages;
using MensagemWeb.Logging;

namespace MensagemWeb.Engines {
	public sealed class ClaroEngine : HttpEngine, IEngine {
		private static readonly ISupported valid = new SupportedList(new ISupported[] 
			{new SupportedRange(61, 69, 91, 95), new SupportedRange(71, 79, 81, 81),
			 new SupportedRange(61, 69, 91, 95), new SupportedRange(81, 89, 91, 94),
			 new SupportedRange(41, 49, 88, 88), new SupportedRange(21, 28, 91, 94),
			 new SupportedRange(51, 55, 91, 94), new SupportedRange(11, 11, 76, 76),
			 new SupportedRange(11, 19, 91, 94), new SupportedRange(31, 38, 84, 84)});
		ISupported IEngine.Valid { get { return valid; } }
		string IEngine.Name { get { return "Claro"; } }
		int IEngine.MaxTotalChars { get { return 146; } }
		
		private const string homeURL = "http://www.claroideias.com.br/portal/site/CIdeias/";
		protected override string BaseURL { 
			get { return "http://clarotorpedoweb.claro.com.br/"; } 
		}
		
		private Dictionary<string, string> postData;
		private string imageSource, codeTag;
		
		void IEngine.Clear() {
			base.Clear();
            imageSource = codeTag = String.Empty;
		}
		
		
		
		
		
		
		
		
		
		private static StringBuilder RemoveHtmlComments(string text) {
			int text_Length = text.Length;
			StringBuilder builder = new StringBuilder(text_Length);
			bool comment = false;
			for (int i = 0; i < text_Length; i++) {
				char text_i = text[i];
				if (!comment) {
					if (text_i != '<')
						builder.Append(text_i);
					else if (text.Substring(i, 4) == "<!--")
						comment = true;
					else
						builder.Append('<');
				} else if (text_i == '-' && text.Substring(i, 3) == "-->") {
					i += 2;
					comment = false;
				}
			}
			return builder;
		}
		
		private delegate void TagDelegate(string name, Dictionary<string,string> attrs);
		private static void ExtractTags(string text, TagDelegate callback) {
			// We could use something like NParsec, but it won't be necessary for now
			bool inTag, inName, inQuote, inValue;
			char quote = '\0';
			string name, propName, propValue;
			Dictionary<string,string> attrs = null;
			
			inTag = inName = inQuote = inValue = false;
			name = propName = propValue = null;
			foreach (char c in text) {
				if (!inTag) {
					if (c == '<') {
						inTag = inName = true;
						inQuote = inValue = false;
						name = propName = propValue = String.Empty;
						attrs = new Dictionary<string,string>(10);
					}
				} else {
					if (!inQuote && (c == '/' || c == '>')) {
						callback(name.ToLower(), attrs);
						inTag = false;
					} else if (inName) {
						if (Char.IsWhiteSpace(c)) 
							inName = false;
						else 
							name += c;
					} else if (inQuote) {
						if (c == quote) {
							attrs[propName.ToLower()] = propValue;
							propName = propValue = String.Empty;
							inQuote = false;
						} else if (c != '\r' && c != '\n')
							propValue += c;
					} else if (inValue) {
						if (c == '\'' || c == '"') {
							inQuote = true;
							inValue = false;
							quote = c;
						}
					} else {
						if (c == '=')
							inValue = true;
						else if (Char.IsLetterOrDigit(c))
							propName += c;
					}
				}
			}
		}
		
		public void Parse(string text) {
			TagDelegate callback = delegate (string name, Dictionary<string,string> attrs) {
				if (name == "img") {
					string possible = null;
					if (attrs.TryGetValue("src", out possible) && possible.StartsWith("/ClaroTorpedoWeb/"))
						imageSource = possible;
				} else if (name == "input") {
					string type = null;
					if (attrs.TryGetValue("type", out type) && type.ToLower() == "text")
						codeTag = attrs["name"];
					else {
						try {
							postData[attrs["name"]] = attrs["value"];
						} catch {
							// Don't bother
						}
					}
				}
			};
			
			ExtractTags(RemoveHtmlComments(text).ToString(), callback);
		}
		
		EngineResult IEngine.SendMessage(Message msg, VerificationDelegate callback) {
			postData = new Dictionary<string, string>();
			postData["ddd_para"] = msg.Destination.DDD.ToString();
			postData["telefone_para"] = msg.Destination.Number.ToString();
			postData["nome_de"] = Util.ToSecureASCII(msg.FromName);
			postData["ddd_de"] = msg.FromDDD;
			postData["telefone_de"] = msg.FromNumber;
			postData["msg"] = Util.ToSecureASCII(msg.Contents);
			
			using (Response response = Post("/ClaroTorpedoWeb/pwdForm.jsp", homeURL, postData)) {
				string text = response.Text;
				if (text.Contains("Sistema temporariamente indispon"))
					return EngineResult.ServerMaintenence;
				Parse(text);
			}
			
			using (Response response = Get(imageSource))
				response.CallVerification(callback);
			
			return EngineResult.Success;
		}
		
		
		
		
		
		
		
		
		
		private string FindCall(string head, string tail, string html) {
			int start = html.IndexOf(head);
			if (start < 0) return null;
			start += head.Length;
			int end = html.IndexOf(tail, start);
			if (end < start) return null;
			return html.Substring(start, end-start);
		}
		
		private string RetornoIdMessage(string html) {
			// parent.retornoIdMessage('ID:P<383945.1147338399876.0>');
			return FindCall("parent.retornoIdMessage('", "');", html);
		}
		
		private int? Retorno(string html) {
			// parent.retorno(20);
			string str = FindCall("parent.retorno(", ");", html);
			if (str != null)
				try {
					return Int32.Parse(str);
				} catch (FormatException) { /* Go ahead */ }
			return null;
		}
		
		EngineResult IEngine.SendCode(string code) {
			code = code.Trim().Replace(" ", String.Empty);
			if (code.Length != 4)
				return EngineResult.WrongPassword;
				
			postData[codeTag] = code.ToUpper();
			
			bool found = false;
			while (!found) {
				string html;
				using (Response response = Get("/ClaroTorpedoWeb/ValidDeliverer", BaseURL + "pwdForm.jsp", postData))
					html = response.Text;
				
				string id = RetornoIdMessage(html);
				if (id != null) {
					postData.Clear();
					postData["idmessage"] = id;
					continue;
				}
				
				// Parse the code the server returned
				try {
					int number = Retorno(html) ?? Convert.ToInt32(html);
					switch (number) {
						case 0:
							return EngineResult.Success;
	                    case 5:
							return EngineResult.WrongPassword;
						case 10:
							return EngineResult.PhoneNotEnabled;
					    case 26:
					    case 27:
					    case 28:
					    case 29:
					        return EngineResult.LimitsExceeded;
					    case 1111:
					    	// Message being sent, try again later
							System.Threading.Thread.Sleep(250);
							continue;
	                 }
					found = true;
				} catch (Exception e) {
					// Ignore any exceptions
	            	Logger.Log(this, e.ToString());
	            	break;
				}
			} // while (!found)
			
			return EngineResult.Unknown;
		}
	}
}
