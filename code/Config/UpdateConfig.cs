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
using System.Xml;

using MensagemWeb;

namespace MensagemWeb.Config {
	public class UpdateConfig : IConfigurable {
		// This is the attribute UpdateManager will look for
		public DateTime? lastUpdate = null;
		public DateTime? lastAutomaticCheck = null;
		public string Xml = null;
		
		public string Section { get { return "updates"; } }
		
		public void SaveConfiguration(XmlWriter writer) {
			if (UpdateManager.Updates == null)
				return;
			
			Util.WriteProperty(writer, "lastUpdate", 
				XmlConvert.ToString(UpdateManager.LastUpdate, XmlDateTimeSerializationMode.Utc));
			Util.WriteProperty(writer, "lastAutomaticCheck",
								XmlConvert.ToString(UpdateManager.LastAutomaticCheck, 
													XmlDateTimeSerializationMode.Utc));
			
			writer.WriteStartElement("mensagemweb");
			Dictionary<string, string> attrs = new Dictionary<string, string>();
			foreach (Update update in UpdateManager.Updates) {
				attrs.Clear();
				attrs["number"] = update.Version.ToString();
				attrs["released"] = XmlConvert.ToString(update.Released, 
														XmlDateTimeSerializationMode.Utc);
				Util.WriteProperty(writer, "version", update.Changes, attrs);
			}
			writer.WriteEndElement();
		}
		
		public void LoadConfiguration(XmlReader reader) {
			// Read the lastUpdate
			while (true) {
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "lastUpdate")
					break;
				else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == Section)
					return;
				else if (!reader.Read())
					return;
			}
			reader.Read();
			lastUpdate = XmlConvert.ToDateTime(reader.Value, XmlDateTimeSerializationMode.Utc);
			
			// Read the lastAutomaticCheck
			while (true) {
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "lastAutomaticCheck")
					break;
				else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == Section)
					return;
				else if (!reader.Read())
					return;
			}
			reader.Read();
			lastAutomaticCheck = XmlConvert.ToDateTime(reader.Value, XmlDateTimeSerializationMode.Utc);
			
			// Read the updates
			while (true) {
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "mensagemweb")
					break;
				else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == Section)
					return;
				else if (!reader.Read())
					return;
			}
			Xml = String.Format("<mensagemweb>{0}</mensagemweb>", reader.ReadInnerXml());
			
			// Move them to UpdateManager
			UpdateManager.UpdateFromConfig(this);
			
			// Clear everything
			Xml = null;
			lastUpdate = null;
			lastAutomaticCheck = null;
		}
		
		public void DefaultConfiguration() {
			// Do not do anything
		}
	}
}