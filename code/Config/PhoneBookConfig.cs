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

using MensagemWeb.Phones;

namespace MensagemWeb.Config {
	public class PhoneBookConfig : IConfigurable {
		public string Section { get { return "phonebook"; } }
		
		public void SaveConfiguration(XmlWriter writer) {
			foreach (KeyValuePair<string, PhoneContainer> kvp in PhoneBook.NamesPhones) {
				writer.WriteStartElement("contact");
					writer.WriteStartElement("name");
					writer.WriteString(XmlConvert.EncodeName(kvp.Key));
					writer.WriteEndElement();
					
					writer.WriteStartElement("ddd");
					writer.WriteString(kvp.Value.Phone.DDD.ToString());
					writer.WriteEndElement();
					
					writer.WriteStartElement("number");
					writer.WriteString(kvp.Value.Phone.Number.ToString());
					writer.WriteEndElement();
					
					if (kvp.Value.Engine != null) {
						writer.WriteStartElement("engine");
						writer.WriteString(kvp.Value.Engine.Name);
						writer.WriteEndElement();
					}
				writer.WriteEndElement();
			}
		}
		
		public void LoadConfiguration(XmlReader reader) {
			PhoneBook.Hold();
			try {
				while (reader.Read()) {
					switch (reader.NodeType) {
						case XmlNodeType.Element:
							if (reader.Name == "contact") LoadContact(reader);
							break;
						case XmlNodeType.EndElement:
							if (reader.Name == Section) return;
							break;
					}
				}
			} finally {
				PhoneBook.Thew();
			}
		}
		
		private void LoadContact(XmlReader reader) {
			string name, ddd, number, engine;
			name = ddd = number = engine = null;
			bool inside = true;
			while (inside && reader.Read()) {
				switch (reader.NodeType) {
					case XmlNodeType.Element:
						string xmlname = reader.Name;
						string inner = "";
						if (reader.Read() && reader.NodeType == XmlNodeType.Text) {
							inner = reader.Value;
							reader.Read(); // -> EndElement
						}
						switch (xmlname) {
							case "name":
								name = XmlConvert.DecodeName(inner); break;
							case "ddd":
								ddd = inner; break;
							case "number":
								number = inner; break;
							case "engine":
								engine = inner; break;
						}
						break;
					case XmlNodeType.EndElement:
						if (reader.Name == "contact")
							inside = false;
						else if (reader.Name == Section)
							return;
						break;
				}
			}
			PhoneBook.Add(name, ddd, number, engine);
		}
		
		public void DefaultConfiguration() {
			// Try to load from that other file
			PhoneBook.OldLoad();
		}
	}
}