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

namespace MensagemWeb.Phones {
	// Holds a name and its corresponding PhoneContainer.
	public class Destination {
		public readonly string Name;
		
		public PhoneContainer Container { 
			get { 
				try {
					return PhoneBook.Get(Name);
				} catch (KeyNotFoundException) {
					return PhoneContainer.Null;
				}
			}
		}
		
		private Destination(string name) {
			if (name == null)
				throw new ArgumentNullException("name");
			if (name.Length == 0)
				throw new ArgumentException("name");
			Name = name;
		}
		
		private static IDictionary<string, Destination> destinations = new Dictionary<string, Destination>();
		public static Destination GetDestination(string name) {
			Destination result;
			if (!destinations.TryGetValue(name, out result)) {
				result = new Destination(name);
				destinations[name] = result;
			}
			return result;
		}
		
		public override string ToString() {
			return String.Format("{0} [{1}]", Name, Container);
		}
		
		public static PhoneContainer[] ToPhoneContainer(Destination[] source) {
			PhoneContainer[] result = new PhoneContainer[source.Length];
			for (int i = 0; i < source.Length; i++)
				if (source[i] == null)
					result[i] = null;
				else
					result[i] = source[i].Container;
			return result;
		}
	}
}