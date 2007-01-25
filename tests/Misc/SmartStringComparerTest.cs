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
using NUnit.Framework;

using MensagemWeb;

namespace Tests.Misc {

	[TestFixture]
	public class SmartStringComparerTest {
	
		[Test]
		public void CompareTo() {
			System.Collections.Generic.IComparer<string> comp = SmartStringComparer.This;
			string[] sorted = new string[] {
				"aaa",
				"aaA",
				"aAa",
				"aAA",
				"Aaa",
				"AAA",
				"aba",
				"aBa",
				"acA",
				"acb",
				"bbb",
				"bbB",
				"bcb"};
			
			for (int i = 0; i < sorted.Length; i++) {
				string strI = sorted[i];
				for (int j = 0; j < i; j++) {
					string strJ = sorted[j];
					Assert.IsTrue(comp.Compare(strI, strJ) > 0, "#1 {0} {1}", strI, strJ);
					Assert.IsTrue(comp.Compare(strJ, strI) < 0, "#2 {0} {1}", strI, strJ);
				}
				Assert.IsTrue(comp.Compare(strI, strI) == 0, "#3 {0}", strI);
				for (int j = i+1; j < sorted.Length; j++) {
					string strJ = sorted[j];
					Assert.IsTrue(comp.Compare(strI, strJ) < 0, "#4 {0} {1}", strI, strJ);
					Assert.IsTrue(comp.Compare(strJ, strI) > 0, "#5 {0} {1}", strI, strJ);
				}
			}
		}
		
	}
}
