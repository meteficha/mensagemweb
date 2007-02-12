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
using System.Xml;

using MensagemWeb;

namespace MensagemWeb.Config {
	public class QueueConfig : IConfigurable {
		public static bool AutoClose = true;
		public static int Width = 1, Height = 1;
		
		public string Section { get { return "queue"; } }
				
		public void SaveConfiguration(XmlWriter writer) {
			Util.WriteProperty(writer, "AutoClose", XmlConvert.ToString(AutoClose));
			Util.WriteProperty(writer, "Width", XmlConvert.ToString(Width));
			Util.WriteProperty(writer, "Height", XmlConvert.ToString(Height));
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
						case "AutoClose":
							AutoClose = XmlConvert.ToBoolean(inner); break;
						case "Width":
							Width = XmlConvert.ToInt32(inner); break;
						case "Height":
							Height = XmlConvert.ToInt32(inner); break;
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