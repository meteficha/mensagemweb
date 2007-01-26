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

using MensagemWeb.Engines;

namespace MensagemWeb.Phones {
	/// <summary>
	/// Container for the data that each name have in the phone book.
	/// </summary>
	public sealed class PhoneContainer : IComparable<PhoneContainer> {
		public readonly Phone Phone;
		public readonly IEngine Engine;
		
		public IEngine RealEngine { 
			get {
				if (Engine != null)
					return Engine;
				else
					return Phone.Engine;
			}
		}
		
		public PhoneContainer(string ddd, string number, string engine) :
			this(new Phone(ddd, number), EngineCatalog.ByName(engine)) { }
		
		public PhoneContainer(Phone phone, IEngine engine) {
			Phone = phone;
			Engine = engine;
		}
		
		public int CompareTo(PhoneContainer ot) {
			int i = (Phone as IComparable<Phone>).CompareTo(ot.Phone);
			if (i != 0)
				return i;
			else {
				if (Engine.GetType() == ot.Engine.GetType())
					return 0;
				else
					return Engine.ToString().CompareTo(ot.Engine.ToString());
			}
		}
		
		public override string ToString() {
			if (Engine == null)
				return Phone.ToString();
			else
				return String.Format("{0} (forçando {1})", Phone, Engine.Name);
		}
	}
}