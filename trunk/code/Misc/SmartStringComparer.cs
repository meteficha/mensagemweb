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
using System.Collections.Generic;

namespace MensagemWeb {
	// Used for sorting, compare strings case insensitively. If they seem equal, compare them
	// case sensitively.
	public sealed class SmartStringComparer: IComparer<string> {
		public static readonly SmartStringComparer This = new SmartStringComparer();
		
		private static readonly StringComparer insensitive = StringComparer.CurrentCultureIgnoreCase;
		private static readonly StringComparer sensitive = StringComparer.CurrentCulture;
		
		int IComparer<string>.Compare(string x, string y) {
			int ret = insensitive.Compare(x, y);
			if (ret != 0)
				return ret;
			else
				return sensitive.Compare(x, y);
		}
		
		private SmartStringComparer () { }
	}
}