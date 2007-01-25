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

using MensagemWeb.Engines;
using MensagemWeb.Phones;

namespace Tests.Engines {

	[TestFixture]
	public class SupportedListTest {
	
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void CtorEx1() {
			new SupportedList(null);
		}
	
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CtorEx2() {
			new SupportedList(new ISupported[] {});
		}
	

		


		private class IsSupportedTest : ISupported {
			public bool Called = false;
			public bool Support = false;
			
			public IsSupportedTest(bool support) {
				Support = support;
			}
			
			bool ISupported.IsSupported(Phone p) {
				Called = true;
				return Support;
			}
		}
		
		[Test]
		public void IsSupported() {
			IsSupportedTest[] list = new IsSupportedTest[] {
				new IsSupportedTest(false),
				new IsSupportedTest(false),
				new IsSupportedTest(false),
				new IsSupportedTest(false),
				new IsSupportedTest(false)};
			ISupported sup = new SupportedList(list);
			Phone p = new Phone(61, 11114444);
			Assert.IsFalse(sup.IsSupported(p), "#1");
			foreach (IsSupportedTest t in list) {
				Assert.IsTrue(t.Called, "#2");
				t.Called = false;
			}
			
			list[1].Support = true;
			Assert.IsTrue(sup.IsSupported(p), "#3");
			Assert.IsTrue(list[1].Called, "#4");
		}
	}
}
