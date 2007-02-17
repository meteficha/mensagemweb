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

using MensagemWeb.Phones;
using MensagemWeb.Logging;

namespace MensagemWeb.Engines {
	public static class EngineCatalog {
		private static readonly IEngine[] engines = new IEngine[] 
			{new BrasilTelecomEngine(), new ClaroEngine(), new OiEngine(), new NovoVivoEngine(),
			 new TelemigEngine(), new AmazoniaCelularEngine(), new CtbcEngine(), new NextelEngine(),
			 new TimEngine()};
		private static Dictionary<Phone, IEngine> engineCache = new Dictionary<Phone, IEngine>(50);
		
		
		public static IEngine ForPhone(Phone number) {
			try {
				return engineCache[number];
			} catch (KeyNotFoundException) {
				foreach (IEngine engine in engines)
					if (engine.Valid.IsSupported(number)) {
						engineCache[number] = engine;
						return engine;
					}
					
				Logger.Log(typeof(Phone), "Could not recognize number {0}.", number);
				engineCache[number] = null;
				return null;
			}
		}
		
		// XXX: Shouldn't this be ICollection`1 or something?
		public static IEngine[] Engines {
			get { return engines; }
		}
		
		public static IEngine ByName(string name) {
			if (name != null && name.Length > 0)
				foreach (IEngine engine in engines)
					if (engine.Name.Equals(name))
						return engine;
			return null;
		}
	}
}