/*  Copyright (C) 2007 Felipe Almeida Lessa
    
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
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.XPath;

using MensagemWeb.Messages;
using MensagemWeb.Logging;

namespace MensagemWeb.Engines {
	public sealed class XmlEngineInterpreter : HttpEngine, IEngine {	
		private ISupported valid;
		private string name;
		private string baseURL;
		private int maxChars;	
		
		private Dictionary<string,object> vars;
		
		private delegate object ActionDelegate();
		
		private XPathNavigator stages;
		private XPathNavigator current;
		private VerificationDelegate callback;
		
		protected override string BaseURL { get { return baseURL; } }
		string IEngine.Name { get { return name; } }
		int IEngine.MaxTotalChars { get { return maxChars; } }
		ISupported IEngine.Valid { get { return valid; } }
				
		
		public static XmlEngineInterpreter[] ParseXml(XPathNavigator nav) {
			XPathNodeIterator iter = nav.Select("//engines/engine");
			XmlEngineInterpreter[] result = new XmlEngineInterpreter[iter.Count];
			int i = 0;
			foreach (XPathNavigator engineNav in iter)
				result[i++] = new XmlEngineInterpreter(engineNav);
			return result;
		}
				
		public XmlEngineInterpreter(XPathNavigator nav) {
			name = GetAttribute(nav, "name"); 
			baseURL = "http://" + GetAttribute(nav, "host") + "/";
			maxChars = Convert.ToInt32(GetAttribute(nav, "chars"));
			
			ParseSupported(nav);
			ParseVars(nav);
			ParseStages(nav);
		}
		
		private string GetAttribute(XPathNavigator nav, string name) {
			return nav.GetAttribute(name, String.Empty);
		}
		
		private string GetAttribute(string name) {
			return current.GetAttribute(name, String.Empty);
		}
		
		private void ParseRange(string range, out int r1, out int r2) {
			if (range.Contains(":")) {
				string[] rs = range.Split(':');
				r1 = Convert.ToInt32(rs[0]);
				r2 = Convert.ToInt32(rs[1]);
			} else {
				r1 = r2 = Convert.ToInt32(range);
			}
		}
		
		private void ParseSupported(XPathNavigator nav) {
			List<ISupported> sups = new List<ISupported>(10);
			foreach (XPathNavigator n in nav.Select("./support/prefix")) {
				int ddd1, ddd2, pre1, pre2;
				ParseRange(GetAttribute(n, "ddd"), out ddd1, out ddd2);
				ParseRange(GetAttribute(n, "number"), out pre1, out pre2);
				
				ISupported sup;	
				try {
					sup = new SupportedRange(ddd1, ddd2, pre1, pre2);
				} catch (ArgumentOutOfRangeException) {
					sup = new SupportedXRange(ddd1, ddd2, pre1, pre2);
				}
				sups.Add(sup);
			}
			valid = new SupportedList(sups.ToArray());
		}
		
		private void ParseVars(XPathNavigator nav) {
			vars = new Dictionary<string,object>(10);
			foreach (XPathNavigator n in nav.Select("./vars/var")) {
				string name = GetAttribute(n, "name");
				string type = GetAttribute(n, "type");
				switch (type) {
					case "string":
						vars[name] = String.Empty;
						break;
					case "dict":
						vars[name] = new Dictionary<object,object>();
						break;
					default:
						throw new NotImplementedException("var of type \"" + type + "\"");
				}
			}
			vars["result"] = String.Empty;
		}
		
		void IEngine.Clear() {
			base.Clear();
			current = null;
			Dictionary<string,object> newVars = new Dictionary<string,object>(vars.Count);
			foreach (KeyValuePair<string,object> kvp in vars) {
				string key = kvp.Key;
				if (kvp.Value is string)
					newVars[key] = String.Empty;
				else if (kvp.Value is IDictionary)
					newVars[key] = new Dictionary<object,object>();
				else
					newVars[key] = null;
			}
		}
		
		private void ParseStages(XPathNavigator nav) {
			foreach (XPathNavigator n in nav.Select("./stages")) {
				stages = n;
				stages.MoveToChild(XPathNodeType.Element);
				break;
			}
		}
		
		public override string ToString() {
			return "[" + name + "]";
		}
		
		
		
		
		EngineResult IEngine.SendMessage(Message msg, VerificationDelegate callback) {
			current = stages.Clone();
			vars["to_ddd"] = msg.Destination.DDD.ToString();
			vars["to_number"] = msg.Destination.Number.ToString();
			vars["from_name"] = Util.ToSecureASCII(msg.FromName);
			vars["from_ddd"] = msg.FromDDD;
			vars["from_number"] = msg.FromNumber;
			vars["message"] = Util.ToSecureASCII(msg.Contents);
			this.callback = callback;
			
			object ret = null;
			while (ret == null || !(ret is EngineResult))
				ret = RunCurrent();
			return ret as EngineResult;
		}
		
		EngineResult IEngine.SendCode(string code) {
			vars["code"] = code.Trim();
			object ret = null;
			while (current != null && (ret == null || !(ret is EngineResult)))
				ret = RunCurrent();
			if (ret is EngineResult)
				return ret as EngineResult;
			else
				return EngineResult.Unknown;
		}
		
		private object RunCurrent() {
			if (current == null)
				throw new Exception("Engine mal-construído.");
			if (aborted)
				return EngineResult.UserCancel;
			
			/*switch (current.Name) {
			"dict-clear" = this.ActionDictClear;
			"dict-update" = this.ActionDictUpdate;
			"exit" = this.ActionExit;
			"image" = this.ActionImage;
			"retrive" = this.ActionRetrive;
			"regexp-replace" = this.ActionRegexpReplace;
			"string-contains" = this.ActionStringContains;
			"string-equals" = this.ActionStringEquals;
			"switch" = this.ActionSwitch;
			}*/
			string method = "Action";
			foreach (string s in current.Name.Split('-')) method += Char.ToUpper(s[0]) + s.Substring(1);
			//Logger.Log(this,method);
			object ret = typeof(XmlEngineInterpreter).GetMethod(method).Invoke(this, new object[] {});
			//string str = Convert.ToString(ret).Trim().Split('\n')[0];
			//Logger.Log(this, "{0} --> {1}", current.Name, str.Substring(0, Math.Min(100, str.Length)));
			if (!current.MoveToNext(XPathNodeType.Element))
				current = null;
			return ret;
		}
		
		private object ParseValue(string val) {
			if (val.Length < 2 || val[0] != '$')
				return val;
			val = val.Substring(1);
			if (!val.Contains("["))
				return vars[val];
			int start = val.IndexOf('[');
			int stop = val.IndexOf(']');
			IDictionary dict = (IDictionary) vars[val.Substring(0, start)];
			start += 1;
			object key = ParseValue(val.Substring(start, stop-start));
			try {
				return dict[key];
			} catch (KeyNotFoundException) {
				return "";
			}
		}
		
		private string ParseString(string val) {
			return (string) ParseValue(val);
		}
		
		private IDictionary ParseDict(string val) {
			return (IDictionary) ParseValue(val);
		}
		
		private Dictionary<string,string> ParseEntries(XPathNodeIterator iter) {
			Dictionary<string,string> ret = new Dictionary<string,string>(iter.Count);
			foreach (XPathNavigator nav in iter)
				switch (nav.Name) {
					case "entry":
						string name = ParseString(GetAttribute(nav, "name"));
						ret[name] = ParseString(GetAttribute(nav, "value"));
						break;
					case "dict-entries":
						foreach (DictionaryEntry kvp in ParseDict(GetAttribute(nav, "var")))
							ret[(string) kvp.Key] = (string) kvp.Value;
						break;
					default:
						throw new NotImplementedException("ParseEntries at \"" + nav.Name + "\"");
				}
			return ret;
		}
		
		private Response ParseResponse() {
			Dictionary<string,string> entries = ParseEntries(current.SelectChildren(XPathNodeType.Element));
			if (entries.Count == 0) entries = null;			
			string referrer = ParseString(GetAttribute("referrer"));
			if (referrer.Length == 0) referrer = lastURL;			
			string address = ParseString(GetAttribute("href"));			
			string method = ParseString(GetAttribute("method"));
			switch (method) {
				case "post":
					return Post(address, referrer, entries);
				case "get":
					return Get(address, referrer, entries);
				default:
					throw new NotImplementedException("Retrive method \"" + method + "\"");
			}
		}
		
		public object ActionAssign() {
			string var = GetAttribute("var");
			if (var.Length < 2 || var[0] != '$')
				throw new Exception("Only vars receive assignments.");
			string value_ = GetAttribute("value");
			object ret;
			if (value_.Length == 0 && current.HasChildren)
				ret = ActionBlock();
			else
				ret = ParseValue(value_);
			vars[var.Substring(1)] = ret;
			return ret;
		}
		
		public bool? ActionDictClear() {
			ParseDict(GetAttribute("var")).Clear();
			return true;
		}
		
		public bool? ActionDictUpdate() {
			object key = ParseValue(GetAttribute("key"));
			object value_ = ParseValue(GetAttribute("value")); 
			ParseDict(GetAttribute("var"))[key] = value_;
			return true;
		}
		
		public EngineResult ActionExit() {
			string message = ParseString(GetAttribute("message"));
			if (message.Length == 0) message = null;
			
			string ret = ParseString(GetAttribute("return"));
			switch (ret) {
				case "ok":
					return new EngineResult(EngineResultType.Success, message);
				case "busy":
					return new EngineResult(EngineResultType.ServerMaintenence, message);
				//case "":
				//	return new EngineResult(EngineResultType.UserCancel, message);
				case "invalid code":
					return new EngineResult(EngineResultType.WrongPassword, message);
				case "not supported":
					return new EngineResult(EngineResultType.PhoneNotEnabled, message);
				//case "":
				//	return new EngineResult(EngineResultType.MaxTryExceeded, message);
				case "limits exceeded":
					return new EngineResult(EngineResultType.LimitsExceeded, message);
				case "unknown":
					return new EngineResult(EngineResultType.Unknown, message);
				default:
					throw new NotImplementedException("Unknown return \"" + ret + "\"");
			}
		}
		
		public EngineResult ActionImage() {
			using (Response r = ParseResponse())
				r.CallVerification(callback);
			callback = null;
			return EngineResult.Success;
		}
		
		public object ActionLog() {
			string attr = GetAttribute("var");
			Logger.Log(this, "{0} = {1}", attr, ParseValue(attr));
			return null;
		}
		
		public object ActionBlock() {
			XPathNavigator saved_current = current.Clone();
			try {
				current.MoveToChild(XPathNodeType.Element);
				object ret = null;
				while (current != null)
					ret = RunCurrent();
				return ret;
			} finally {
				current = saved_current;
			}			
		}
		
		public bool? ActionAnd() {
			XPathNavigator saved_current = current.Clone();
			try {
				current.MoveToChild(XPathNodeType.Element);
				while (current != null)
					if ((bool?) RunCurrent() ?? false == false)
						return false;
				return true;
			} finally {
				current = saved_current;
			}			
		}
		
		public bool? ActionOr() {
			XPathNavigator saved_current = current.Clone();
			try {
				current.MoveToChild(XPathNodeType.Element);
				while (current != null)
					if ((bool?) RunCurrent() ?? false == true)
						return true;
				return false;
			} finally {
				current = saved_current;
			}			
		}
		
		public bool? ActionNot() {
			XPathNavigator saved_current = current.Clone();
			try {
				current.MoveToChild(XPathNodeType.Element);
				return !((bool?) RunCurrent() ?? false);
			} finally {
				current = saved_current;
			}			
		}
		
		public string ActionRetrive() {
			string result;
			using (Response r = ParseResponse())
				result = r.Text;
			vars["result"] = result;
			return result;
		}
		
		public bool? ActionRegexpMatch() {
			Regex r = new Regex(ParseString(GetAttribute("regexp")), RegexOptions.CultureInvariant);
			string input = ParseString(GetAttribute("input"));
			string group = ParseString(GetAttribute("group"));
			string output_var = GetAttribute("output");
			
			if (output_var[0] == '$') output_var = output_var.Substring(1);
			else throw new Exception("output of regexps should be vars");
			
			Group g = r.Match(input).Groups[group];
			vars[output_var] = g.Value;
			return g.Success;
		}
		
		public string ActionRegexpReplace() {
			Regex r = new Regex(ParseString(GetAttribute("regexp")), 
			                    RegexOptions.Singleline | RegexOptions.CultureInvariant);
			string input = ParseString(GetAttribute("input"));
			string repl = ParseString(GetAttribute("replacement"));
			string output_var = GetAttribute("output");
			
			if (output_var[0] == '$') output_var = output_var.Substring(1);
			else throw new Exception("output of regexps should be vars");
			
			string output = r.Replace(input, repl);
			vars[output_var] = output;
			return output;
		}
		
		public string ActionStringConcat() {
			XPathNavigator saved_current = current.Clone();
			try {
				current.MoveToChild(XPathNodeType.Element);
				System.Text.StringBuilder builder = new System.Text.StringBuilder();
				while (current != null)
					builder.Append((string) RunCurrent());
				return builder.ToString();
			} finally {
				current = saved_current;
			}			
		}
		
		public bool? ActionStringContains() {
			string str = ParseString(GetAttribute("string"));
			string contained = ParseString(GetAttribute("contained"));
			return str.Contains(contained);
		}
		
		public bool? ActionStringEquals() {
			string str = ParseString(GetAttribute("string"));
			string other = ParseString(GetAttribute("other"));
			return str.ToLowerInvariant() == other.ToLowerInvariant();
		}
		
		public bool? ActionStringStartsWith() {
			string str = ParseString(GetAttribute("string"));
			string other = ParseString(GetAttribute("start"));
			return str.StartsWith(other);
		}
		
		public string ActionStringUpper() {
			string str = ParseString(GetAttribute("string"));
			return str.ToUpper();
		}
		
		public object ActionSwitch() {
			XPathNavigator saved_current = current.Clone();
			try {
				foreach (XPathNavigator nav in current.SelectChildren(XPathNodeType.Element)) {
					current = nav.Clone();
					current.MoveToChild(XPathNodeType.Element);
					if (nav.Name == "default" || true == ((bool?) RunCurrent() ?? false)) {
						object ret = null;
						while (current != null)
							ret = RunCurrent();
						return ret;
					}
				}
				return null;
			} finally {
				current = saved_current;
			}
		}
		
		public object ActionSgmlExtractTags() {
			XPathNavigator saved_current = current.Clone();
			try {
				XPathNavigator new_current = current;
				new_current.MoveToChild(XPathNodeType.Element);
				object ret = null;
				Sgml.ExtractTags(ParseString(GetAttribute(saved_current, "value")),
					delegate (string name, Dictionary<string,string> attrs) {
						current = new_current.Clone();
						vars["tag_name"] = name;
						vars["tag_attr"] = attrs;
						while (current != null)
							ret = RunCurrent();
					});
				return ret;
			} finally {
				current = saved_current;
			}	
		}
		
		public object ActionValue() {
			return ParseValue(GetAttribute("var"));
		}
		
		public object ActionWhile() {
			XPathNavigator saved_current = current.Clone();
			try {
				XPathNavigator new_current = current;
				new_current.MoveToChild(XPathNodeType.Element);
				current = new_current.Clone();
				object ret = null;
				for (; (bool?) RunCurrent() ?? false == true; current = new_current.Clone())
					while (current != null)
						ret = RunCurrent();
				return ret;
			} finally {
				current = saved_current;
			}			
		}
	}
	
	
	
	
	
	
	internal static class Sgml {
		public delegate void TagDelegate(string name, Dictionary<string,string> attrs);
		public static void ExtractTags(string text, TagDelegate callback) {
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
		} /* extractTags */
	}
}






