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
		public string Section { get { return "queue"; } }
				
		public void SaveConfiguration(XmlWriter writer) {
			Util.WriteProperty(writer, "AutoClose", XmlConvert.ToString(AutoClose));
		}
		
		public void LoadConfiguration(XmlReader reader) {
			reader.Read();
			while (true) {
				if (reader.NodeType == XmlNodeType.Element) {
					string name = reader.Name;
					string inner = reader.ReadInnerXml();
					switch (name) {
						case "AutoClose":
							AutoClose = XmlConvert.ToBoolean(inner);
							break;
					}
				} else if (reader.NodeType == XmlNodeType.EndElement &&
						   reader.Name == Section)
					return;
				else
					reader.Read();
			}
		}
		
		public void DefaultConfiguration() {
			// Do not do anything
		}
	}
}