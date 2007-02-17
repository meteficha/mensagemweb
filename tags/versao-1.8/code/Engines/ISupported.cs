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
using MensagemWeb.Messages;

namespace MensagemWeb.Engines {
	// Nice idea taken from jSMS. Thanks!
	public interface ISupported {
		bool IsSupported(Phone phone);
	}
	
	// O(1)
	public class SupportedRange : ISupported {
		private int firstDDD, lastDDD, firstPrefix, lastPrefix;
		public SupportedRange(int firstDDD, int lastDDD, int firstPrefix, int lastPrefix) {
			if (firstDDD < 10 || firstDDD > 99)
				throw new ArgumentOutOfRangeException("firstDDD");
			if (lastDDD < 10 || lastDDD > 99)
				throw new ArgumentOutOfRangeException("lastDDD");
			if (firstDDD > lastDDD)
				throw new ArgumentException("firstDDD > lastDDD");
				
			if (firstPrefix < 10 || firstPrefix > 99)
				throw new ArgumentOutOfRangeException("firstPrefix");
			if (lastPrefix < 10 || lastPrefix > 99)
				throw new ArgumentOutOfRangeException("lastPrefix");
			if (firstPrefix > lastPrefix)
				throw new ArgumentException("firstPrefix > lastPrefix");
				
			this.firstDDD = firstDDD;
			this.lastDDD = lastDDD;
			this.firstPrefix = firstPrefix;
			this.lastPrefix = lastPrefix;
		}
		
		bool ISupported.IsSupported(Phone phone) {
			int ddd = phone.DDD;
			int prefix = phone.Prefix;
			return (prefix >= firstPrefix && prefix <= lastPrefix &&
					ddd >= firstDDD && ddd <= lastDDD);
		}
	}
	
	
	// O(lg n) where n is the number of prefix combinations.
	public class SupportedXRange : ISupported {
		private string[] prefixes; // Must be sorted
		private int firstDDD, lastDDD, prefixLength;
		public SupportedXRange(int firstDDD, int lastDDD, int firstPrefix, int lastPrefix) {
			if (firstDDD < 10 || firstDDD > 99)
				throw new ArgumentOutOfRangeException("firstDDD");
			if (lastDDD < 10 || lastDDD > 99)
				throw new ArgumentOutOfRangeException("lastDDD");
			if (firstDDD > lastDDD)
				throw new ArgumentException("firstDDD > lastDDD");
				
			if (firstPrefix < 100 || lastPrefix < 100)
				throw new ArgumentOutOfRangeException("For 2-digit prefixes use SupportedRange.");
			if (firstPrefix >= 100000000)
				throw new ArgumentOutOfRangeException("firstPrefix");
			if (lastPrefix >= 100000000)
				throw new ArgumentOutOfRangeException("lastPrefix");
			if (firstPrefix > lastPrefix)
				throw new ArgumentException("firstPrefix > lastPrefix");
			
			if (lastPrefix.ToString().Length != firstPrefix.ToString().Length)
				throw new ArgumentException("All prefixes must have the same length");
			
			this.firstDDD = firstDDD;
			this.lastDDD = lastDDD;
			
			int diff = lastPrefix - firstPrefix;
			this.prefixes = new string[diff + 1];
			for (int i = 0; i <= diff; i++)
				prefixes[i] = Convert.ToString(i + firstPrefix);
				
			this.prefixLength = prefixes[0].Length;
		}
		
		bool ISupported.IsSupported(Phone phone) {
			int ddd = phone.DDD;
			if (ddd >= firstDDD && ddd <= lastDDD) {
				string pre = phone.Number.ToString().Substring(0, prefixLength);
				if (Array.BinarySearch<string>(prefixes, pre) >= 0)
					return true;
			}
			return false;
		}
	}
	
	
	// O(n) where n is list length
	public class SupportedList : ISupported {
		private IEnumerable<ISupported> list;
		public SupportedList(IEnumerable<ISupported> list) {
			if (list == null)
				throw new ArgumentNullException("list");
			if (!list.GetEnumerator().MoveNext())
				throw new ArgumentException("list should have at least one ISupported");
				
			this.list = list;
		}
		
		bool ISupported.IsSupported(Phone phone) {
			foreach (ISupported sup in list)
				if (sup.IsSupported(phone))
					return true;
			return false;
		}
	}
}