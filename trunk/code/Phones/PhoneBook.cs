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

using MensagemWeb.Config;
using MensagemWeb.Engines;
using MensagemWeb.Logging;

namespace MensagemWeb.Phones {	
	public static class PhoneBook {	
		public static event EventHandler Updated;
		
		
		private static SortedList<string,PhoneContainer> Container;
		private static bool held = false;
		private static bool postponed = false;
		
		static PhoneBook() {
			Container = new SortedList<string,PhoneContainer>(50, SmartStringComparer.This);
		}
		
		
		// Loads the phone book from the old file.
		public static void OldLoad() {
			Logger.Log(typeof(PhoneBook), "OldLoad() is deprecated.");
			Container.Clear();
			string path = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
					"MensagemWeb.phoneBook");
			try {
				using (StreamReader reader = File.OpenText(path)) {
					string version = reader.ReadLine();
					bool version2;
					if (version == "v1")
						version2 = false;
					else if (version == "v2")
						version2 = true;
					else
						throw new NotImplementedException("Can't parse version \"" +
							version + "\" files.");
							
					string name, ddd, number, engine = null;
					while (true) {
						name = reader.ReadLine();
						if (version2)
							engine = reader.ReadLine();
						ddd = reader.ReadLine();
						number = reader.ReadLine();
						
						if (number == null)
							break;
						
						Add(name, ddd, number, engine);
					}
				}
			} catch (Exception e) { 
				// Don't care about it
				Logger.Log(typeof(PhoneBook), e.ToString());
			}
		}
		
		
		
		private static void Save() {
			if (held) {
				postponed = true;
			} else {
				Configuration.Save();	
				if (Updated != null)
					Updated(typeof(PhoneBook), null);
			}
		}
		
		
		public static void Hold() {
			postponed = false;
			held = true;
		}
		
		public static void Thew() {
			if (held) {
				held = false;
				if (postponed)
					Save();
			}
		}
		
		
		
		public static void Add(string name, Phone number, IEngine engine) {
			if (name == null || number == null)
				throw new ArgumentNullException("Name or number is null.");
			if (name.Length < 1 || name.Contains("\n"))
				throw new ArgumentException("name");
			Container.Add(name, new PhoneContainer(number, engine));
			Save();
		}
		
		public static void Add(string name, string ddd, string number, string engine) {
			Container.Add(name, new PhoneContainer(ddd, number, engine));
			Save();
		}
		
		
		public static bool Contains(string name) {
			return Container.ContainsKey(name);
		}
		
		// We can't use [] because we're static =( 
		public static PhoneContainer Get(string name) {
			return Container[name];
		} 
		
		public static PhoneContainer TryGet(string name) {
			return TryGet(name, null);
		} 
		
		public static PhoneContainer TryGet(string name, PhoneContainer default_) {
			PhoneContainer result;
			if (Container.TryGetValue(name, out result))
				return result;
			else
				return default_;
		} 
		
		// WARNING: O(n)!
		public static string FindName(Phone number) {
			foreach (KeyValuePair<string, PhoneContainer> kvp in Container) { 
				if (kvp.Value.Phone == number)
					return kvp.Key;
			}
			return null;
		}
		
		
		public static void Remove(string name) {
			Container.Remove(name);
			Save();
		}
		
		
		// Sorted by name
		public static IEnumerable<KeyValuePair<string, PhoneContainer>> NamesPhones {
			get { return Container; }
		}
		
		// Sorted by name
		public static IEnumerable<string> Names { 
			get { return Container.Keys; } 
		}
		
		
		public static int? DefaultDDD {
			get {
				int dddFound = -1;
				foreach (PhoneContainer cont in (IEnumerable<PhoneContainer>) Container.Values) {
					int ddd = cont.Phone.DDD;
					if (dddFound == -1)
						dddFound = ddd;
					else if (ddd != dddFound)
						return null;
				}
				if (dddFound == -1)
					return null;
				else
					return dddFound;
			}
		}
		
		public static int Count { 
			get { return Container.Count; } 
		}
	}
}