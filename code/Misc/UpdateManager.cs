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
using System.Reflection;
using System.Net;
using System.Xml;

using MensagemWeb.Config;
using MensagemWeb.Logging;

namespace MensagemWeb {
	// Represents a version. (it's not named is not "Version" to avoid clashing with System.Version)
	public sealed class Update : IComparable<Update> {
		public readonly Version Version;
		public readonly DateTime Released;
		public readonly string Changes;
		
		public Update(string version, string released, string changes) :
			this(new Version(version), 
				 XmlConvert.ToDateTime(released, XmlDateTimeSerializationMode.Utc), 
				 changes) { }
		
		public Update(Version version, DateTime released, string changes) {
			Version = version;
			Released = released;
			Changes = changes;
		}
		
		int IComparable<Update>.CompareTo(Update other) {
			if (other == null)
				return 1;
			else
				return this.Version.CompareTo(other.Version);
		}
		
		public override string ToString() {
			return String.Format("=== MensagemWeb {0} ({1}) ===\n{2}", Version, Released, Changes);
		}
	}
	
	
	
	public static class UpdateManager {
		private const string address = 
			"http://mensagemweb.codigolivre.org.br/mensagemweb.xml";
		private static DateTime lastUpdate = DateTime.MinValue;
		private static DateTime lastAutomaticCheck = DateTime.MinValue;
		private static Update[] updates = null;
		private static bool updating = false;
		
		public static Version CurrentVersion = Assembly.GetCallingAssembly().GetName().Version;
		
		public static DateTime LastUpdate { get { return lastUpdate; } }
		
		public static DateTime LastAutomaticCheck { 
			get { return lastAutomaticCheck; }
			set { lastAutomaticCheck = value; }
		}
		
		// This array is sorted with older versions first.
		public static Update[] Updates { get { return updates; } }
		
		
		public static bool NewVersions { get {
			CheckForUpdates();
			if (updates != null) {
				Update lastUpdate = updates[updates.Length-1];
				return (lastUpdate.Version > CurrentVersion);
			} else
				return false;
		} }
		
		
		public static void UpdateFromConfig(UpdateConfig config) {
			StringReader reader = new StringReader(config.Xml);
			XmlTextReader xml = new XmlTextReader(reader);
			Parse(xml);
			lastUpdate = config.lastUpdate.Value;
			lastAutomaticCheck = config.lastAutomaticCheck.Value;
		}
		
		public static void CheckForUpdates() {
			if (updating)
				return;
			updating = true;
			try {
				// Do not check again if we checked some time ago
				if (DateTime.Now.Subtract(lastUpdate).TotalHours < 12)
					return;
				
				// Get and parse the XML
				updates = null;
				WebRequest request = WebRequest.Create(address);
				request.Timeout = 10000;
				request.Proxy = MensagemWeb.Config.ProxyConfig.GetProxy();
				request.Method = "GET";
				using (WebResponse response = request.GetResponse())
					using (Stream stream = response.GetResponseStream())
						Parse(new XmlTextReader(stream));
				
				// Save the last time we did this
				lastUpdate = DateTime.Now;
			} catch (Exception e) {
				// Log any error
				Logger.Log(typeof(UpdateManager), e.ToString());
				updates = null;
			} finally {
				// Call our friends
				updating = false;
			}
		}
		
		private static void Parse(XmlReader xml) {
			// Prepare our temporary array
			List<Update> array = new List<Update>(16);
			
			// Find the root tag
			while (xml.Read()) {
				if (xml.NodeType == XmlNodeType.Element &&
						xml.Name == "mensagemweb")
					break;
			}	
			
			// Find all version tags
			while (true) {
				if (xml.NodeType == XmlNodeType.Element &&
						xml.Name == "version") {
					string version = xml["number"];
					string released = xml["released"];
					string changes = xml.ReadInnerXml();
					array.Add(new Update(version, released, changes));
				} else if (xml.NodeType == XmlNodeType.EndElement &&
						xml.Name == "mensagemweb")
					break;
				else if (!xml.Read())
					break;
			}
			
			// Create the final array
			array.Sort();
			updates = array.ToArray();
		}
	}
}
