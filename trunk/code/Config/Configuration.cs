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
using System.IO;
using System.Xml;

using MensagemWeb.Logging;

namespace MensagemWeb.Config {
	// Interface that classes that have to save/load their configurations need implement.
	public interface IConfigurable {
		string Section { get; }
		void SaveConfiguration(XmlWriter writer);
		void LoadConfiguration(XmlReader reader);
		void DefaultConfiguration();
	}
	
	
	
	// This is the class responsable of saving and loading
	// every configuration of MensagemWeb starting on version 1.3.
	public static class Configuration {
		private static bool reading = false;
		
		private static string configFileCache = null;
		public static string ConfigFilePath {
			get {
				if (configFileCache == null) {
				 	string directory = Environment.GetFolderPath(
						Environment.SpecialFolder.ApplicationData);
					if (!Directory.Exists(directory))
						Directory.CreateDirectory(directory);
					configFileCache = Path.Combine(directory, "MensagemWebConfig.xml");
					Logger.Log(typeof(Configuration), "Configuration file path: {0}",
								configFileCache);
				}
				return configFileCache;
			}
		}
		
		private static readonly IConfigurable[] Connections = 
			new IConfigurable[] {
				new PhoneBookConfig(), 
				new MainWindowConfig(),
				new ProxyConfig(),
				new UpdateConfig(),
				new QueueConfig()
			};
		
		private static bool held = false;
		private static bool postponed = false;
		
		
		public static void Load() {
			Logger.Log(typeof(Configuration), "Loading configuration...");
			reading = true;
			try {
				IConfigurable[] connections = (IConfigurable[]) Connections.Clone();
				try {
					using (StreamReader streamReader = File.OpenText(ConfigFilePath)) {
						// Open the file
						XmlTextReader reader = new XmlTextReader(streamReader);
						
						// Find the root tag
						while (reader.Read()) {
							if (reader.NodeType == XmlNodeType.Element && 
									reader.Name == "configuration")
								break;
						}
						
						// Now loop inside it
						reader.Read();
						while (true) {
							if (reader.NodeType == XmlNodeType.Element)
								try {
									// Read this section
									ReadSection(reader, ref connections);
								} catch (Exception e) {
									Logger.Log(typeof(Configuration), 
										"Caught an exception while loading " +
										"the configuration:\n{0}\n\nTrying to " +
										"continue loading the other sections.", e);
								}
							else if (reader.NodeType == XmlNodeType.EndElement &&
									reader.Name == "configuration")
								// We found the end
								break;
							else if (!reader.Read())
								break;
						}
						
						// We're finished now =D!
					} 
				} catch (Exception e) {
					// Try the default configurations
					Logger.Log(typeof(Configuration), "Could not load the configuration file" +
						" (see exception below), using default values\n{0}", e);
				}
				
				// Check if we need to use the default values for something
				foreach (IConfigurable conn in connections)
					if (conn != null) {
						Logger.Log(typeof(Configuration),
							"Using the default configuration for {0}", conn.GetType());
						conn.DefaultConfiguration();
					}
			} finally {
				reading = false;
			}
			Logger.Log(typeof(Configuration), "Finished loading configuration.");
		}
		
		
		
		private static void ReadSection(XmlReader reader, ref IConfigurable[] connections) {
			// Get the name of the section and the corresponding connection
			string sectionName = reader.Name.ToLower();
			IConfigurable conn = null;
			int i = 0;
			for (; i < connections.Length; i++)
				if (connections[i] != null && connections[i].Section.ToLower() == sectionName) {
					conn = connections[i];
					break;
				}
			
			// Check if this version knows about this section
			if (conn == null) {
				Logger.Log(typeof(Configuration), "Section \"" + sectionName + "\" unknown...");
			} else {
				conn.LoadConfiguration(reader);
				connections[i] = null;
			}
			
			// Find the end element just to be sure
			while (reader.NodeType != XmlNodeType.EndElement || reader.Name.ToLower() != sectionName) {
				if (!reader.Read()) return;
			}
		}
		
		
		
		public static bool Save() {
			// Check if we're reading
			if (reading == true) {
				Logger.Log(typeof(Configuration), "We're reading, not saving configuration...");
				return false;
			}
			
			// Check if we're being postponed
			if (held == true) {
				Logger.Log(typeof(Configuration), "We're in hold, not saving configuration...");
				postponed = true;
				return true;
			}
			
			// Log our action
			Logger.Log(typeof(Configuration), "Saving configuration...");
			
			// Open the file
			XmlTextWriter writer = new XmlTextWriter(ConfigFilePath, System.Text.Encoding.UTF8);
			writer.Namespaces = false;
			writer.Formatting = Formatting.None;
			try {
				// Root tag start
				writer.WriteStartElement("configuration");
				
				// Loop through each IConfigurable
				foreach (IConfigurable conn in Connections) {
					try {
						// Create their tag
						writer.WriteStartElement(conn.Section);
						
						// Write a space to prevent something like <tag />
						// that will disrupt our loading code
						writer.WriteString("\n");
						
						try {
							// Call them
							conn.SaveConfiguration(writer);
						} finally {
							// Close their tag
							writer.WriteEndElement();
						}
					} catch (Exception e) {
						Logger.Log(typeof(Configuration), "Caught an exception " +
							"while saving the configuration:\n{0}\n\n" +
							"Trying to continue saving the other sections.", e);
					}
				}
				
				// Root tag end
				writer.WriteEndElement();
			} finally {
				// Close the file
				writer.Close();
			}
			
			// Log our action
			Logger.Log(typeof(Configuration), "Configuration saved!");
			
			return true;
		}
		
		
		
		public static void Hold() {
			postponed = false;
			held = true;
		}
		
		
		
		public static void Thew() {
			if (held) {
				held = false;
				if (postponed) {
					Save();
				}
			}
		}
	}
}
