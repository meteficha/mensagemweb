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
using System.Xml;
using System.Net;

using MensagemWeb;

namespace MensagemWeb.Config {
	public class ProxyConfig : IConfigurable {
		public static bool UseProxy = true;
		public static bool UseSystemProxy = true;
		public static string Host = String.Empty;
		public static int Port = 8080;
		public static string Username = String.Empty;
		public static string Password = String.Empty;
		public string Section { get { return "proxy"; } }
		
		public static WebProxy GetProxy() {
			if (UseProxy) {
				if (UseSystemProxy) {
					return WebProxy.GetDefaultProxy();
				} else {
					try {
						WebProxy proxy = new WebProxy(Host, Port);
						if (Username != null && Username.Length > 0
						          && Password != null && Password.Length > 0)
							proxy.Credentials = new NetworkCredential(Username, Password);
						return proxy;
					} catch (Exception e) {
						MensagemWeb.Logging.Logger.Log(typeof(ProxyConfig), e.ToString());
						return null;
					}
				}
			} else {
				return new WebProxy();
			}
		}
				
		public void SaveConfiguration(XmlWriter writer) {
			Util.WriteProperty(writer, "UseProxy", XmlConvert.ToString(UseProxy));
			Util.WriteProperty(writer, "UseSystemProxy", XmlConvert.ToString(UseSystemProxy));
			Util.WriteProperty(writer, "Host", XmlConvert.EncodeName(Host));
			Util.WriteProperty(writer, "Port", XmlConvert.ToString(Port));
			Util.WriteProperty(writer, "Username", XmlConvert.EncodeName(Username));
			Util.WriteProperty(writer, "Password", XmlConvert.EncodeName(Password));
		}
		
		public void LoadConfiguration(XmlReader reader) {
			while (reader.Read()) {
				if (reader.NodeType == XmlNodeType.Element) {
					string name = reader.Name;
					string inner = "";
					if (reader.Read() && reader.NodeType == XmlNodeType.Text) {
						inner = reader.Value;
						reader.Read(); // -> EndElement
					}
					switch (name) {
						case "UseProxy":
							UseProxy = XmlConvert.ToBoolean(inner);
							break;
						case "UseSystemProxy":
							UseSystemProxy = XmlConvert.ToBoolean(inner);
							break;
						case "Host":
							Host = XmlConvert.DecodeName(inner);
							break;
						case "Port":
							Port = XmlConvert.ToInt32(inner);
							break;
						case "Username":
							Username = XmlConvert.DecodeName(inner);
							break;
						case "Password":
							Password = XmlConvert.DecodeName(inner);
							break;
					}
				} else if (reader.NodeType == XmlNodeType.EndElement &&
						   reader.Name == Section)
					return;
			}
		}
		
		public void DefaultConfiguration() {
			// Do not do anything
		}
	}
}