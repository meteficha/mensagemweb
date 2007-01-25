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

using MensagemWeb.Engines;

namespace MensagemWeb.Phones {
	public class Phone : IComparable<Phone> {		
		// Taken from http://www.embratel.com.br/Embratel02/files/secao/06/13/365/codigos_ddd.zip
		// Keep this sorted for BinarySearch!
		private static readonly int[] validDDDs = new int[] {
			11, 12, 13, 14, 15, 16, 17, 18, 19, 21, 22, 24, 27, 28, 31, 32, 33,
			34, 35, 37, 38, 41, 42, 43, 44, 45, 46, 47, 48, 49, 51, 53, 54, 55, 
			61, 62, 63, 64, 65, 66, 67, 68, 69, 71, 73, 74, 75, 77, 79, 81, 82, 
			83, 84, 85, 86, 87, 88, 89, 91, 92, 93, 94, 95, 96, 97, 98, 99};
		
		public readonly int DDD;
		public readonly int Number;
		public readonly int Prefix;
		
		public IEngine Engine { 
			get { return EngineCatalog.ForPhone(this); }
		}
		
		public Phone(int ddd, int number) {
			if (ddd < 10 || ddd > 99)
				throw new ArgumentOutOfRangeException("ddd");
			if (!ValidDDD(ddd))
				throw new ArgumentException("This DDD is not known.");
			if (number < 10000000 || number > 99999999)
				throw new ArgumentOutOfRangeException("number");
				
			DDD = ddd; 
			Number = number;
			Prefix = Convert.ToInt32(Math.Floor(number / 10.0e5));
		}
		
		
		public Phone(string ddd, string number)
			: this(Int32.Parse(ddd), Int32.Parse(number)) 
		{
			// Nothing special
		}
		
		public static bool ValidDDD(int ddd) {
			return (Array.BinarySearch<int>(validDDDs, ddd) >= 0);
		}
		
		
		public override string ToString() {
			string numberAsString = Number.ToString();
			string firstPart = numberAsString.Substring(0, 4);
			string secondPart = numberAsString.Substring(4);
			return String.Format("({0}) {1}-{2}", DDD, firstPart, secondPart);
		}
		
		public override int GetHashCode() {
			// Hardly the same person will have on its PhoneBook two different
			// Phone's with the same number and different DDDs.
			// This is sufficient and fast.
			return Number.GetHashCode();
		}
		
		public override bool Equals(object other) {
			Phone ot = other as Phone;
			if (ot == null) return false;
			return (this.Number == ot.Number && this.DDD == ot.DDD);
		}
		
		public static bool operator ==(Phone x, Phone y) {
			if (x as object == null) {
				if (y as object == null)
					return true;
				else
					return false;
			} else if (y as object == null)
				return false;
				
			return (x.Number == y.Number) && (x.DDD == y.DDD);
		}
		
		public static bool operator !=(Phone x, Phone y) {
			return ! (x == y);
		}
		
		int IComparable<Phone>.CompareTo(Phone other) {
			int comp = this.DDD.CompareTo(other.DDD);
			if (comp == 0)
				return this.Number.CompareTo(other.Number);
			else
				return comp;
		}
	}
}