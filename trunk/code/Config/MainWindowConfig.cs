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

#if !LITE
using System.Collections.Generic;
using MensagemWeb.Phones;
using MensagemWeb.Windows;
#else
using MensagemWeb.Logging;
#endif

namespace MensagemWeb.Config {
	public class MainWindowConfig : IConfigurable {
#if !LITE
		// These are the attributes MainWindow will look for
		public static bool Loaded = false;
		public static List<string> Destinations = new List<string>();
		public static string FromPhoneDDD = String.Empty;
		public static string FromPhoneNumber = String.Empty;
		public static string FromName = String.Empty;
		public static string DestPhoneStr = String.Empty;
		public static string Contents = String.Empty;
		public static int Width = -1;
		public static int Height = -1;
		public static bool Sent = true;
		
		// This attribute MainWindow sets before being destroyed
		public static IEnumerable<Destination> savedDests = null;
#endif
		
		string IConfigurable.Section { get { return "mainwindow"; } }
		
		void IConfigurable.SaveConfiguration(XmlWriter writer) {
#if !LITE
			MainWindow mw = MainWindow.This;
			
			Util.WriteProperty(writer, "FromPhoneDDD", XmlConvert.EncodeName(mw.FromPhoneDDD));
			Util.WriteProperty(writer, "FromPhoneNumber",XmlConvert.EncodeName(mw.FromPhoneNumber));
			Util.WriteProperty(writer, "FromName", XmlConvert.EncodeName(mw.FromName));
			Util.WriteProperty(writer, "Contents", XmlConvert.EncodeName(mw.Contents));
			Util.WriteProperty(writer, "Width", XmlConvert.ToString(mw.Allocation.Width));
			Util.WriteProperty(writer, "Height", XmlConvert.ToString(mw.Allocation.Height));
			
			if (savedDests == null)
				savedDests = mw.Destinations;
			if (savedDests != null) {
				foreach (Destination dest in savedDests)
					if (dest != null)
						Util.WriteProperty(writer, "Destination", XmlConvert.EncodeName(dest.Name));
				savedDests = null;
			}
#else
			Logger.Log(this, "This is LITE version, MainWindowConfig disabled.");
#endif
		}
		
		void IConfigurable.LoadConfiguration(XmlReader reader) {
#if !LITE
			// Read the values
			reader.Read();
			string section = (this as IConfigurable).Section; 
			while (true) {
				if (reader.NodeType == XmlNodeType.Element) {
					string name = reader.Name;
					string inner = reader.ReadInnerXml();
					switch (name) {
						case "FromPhoneDDD":
							FromPhoneDDD = XmlConvert.DecodeName(inner); break;
						case "FromPhoneNumber":
							FromPhoneNumber = XmlConvert.DecodeName(inner); break;
						case "FromName":
							FromName = XmlConvert.DecodeName(inner); break;
						case "DestPhoneStr":
							DestPhoneStr = XmlConvert.DecodeName(inner); break;
						case "Destination":
							Destinations.Add(XmlConvert.DecodeName(inner)); break;
						case "Contents":
							Contents = XmlConvert.DecodeName(inner); break;
						case "Width":
							Width = XmlConvert.ToInt32(inner); break;
						case "Height":
							Height = XmlConvert.ToInt32(inner); break;
					}
				} else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == section)
					break;
				else
					reader.Read();
			}
			
			// We read them
			Loaded = true;
#else
			Logger.Log(this, "This is LITE version, MainWindowConfig disabled.");
#endif
		}
		
		void IConfigurable.DefaultConfiguration() {
			// Do not do anything
		}
	}
}