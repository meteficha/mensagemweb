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
	public class SupportedXRangeTest {
	
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Ctor_InvalidDDD1() {
			new SupportedXRange(0, 61, 900, 901);
		}
	
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Ctor_InvalidDDD2() {
			new SupportedXRange(61, 100, 900, 901);
		}
	
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Ctor_InvalidDDD3() {
			new SupportedXRange(0, 100, 900, 901);
		}
	
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Ctor_InvalidDDD4() {
			new SupportedXRange(62, 61, 900, 901);
		}
	
	
	
	
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Ctor_InvalidPrefix1() {
			new SupportedXRange(61, 61, 0, 901);
		}
	
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Ctor_InvalidPrefix2() {
			new SupportedXRange(61, 61, 901, 0);
		}
	
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Ctor_InvalidPrefix3() {
			new SupportedXRange(61, 61, 80, 91);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Ctor_InvalidPrefix5() {
			new SupportedXRange(61, 61, 100000000, 100000000);
		}
		
		
	
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Ctor_InvalidPrefix6() {
			new SupportedXRange(61, 61, 1000, 10000);
		}
	
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Ctor_InvalidPrefix7() {
			new SupportedXRange(61, 61, 10000, 100000);
		}
	
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Ctor_InvalidPrefix8() {
			new SupportedXRange(61, 61, 100000, 1000000);
		}
	
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Ctor_InvalidPrefix9() {
			new SupportedXRange(61, 61, 1000000, 10000000);
		}
		
	
	
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Ctor_InvalidPrefix10() {
			new SupportedXRange(61, 61, 900, 800);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Ctor_InvalidPrefix11() {
			new SupportedXRange(61, 61, 9000, 8000);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Ctor_InvalidPrefix12() {
			new SupportedXRange(61, 61, 90000, 80000);
		}
	
	
	
		[Test]
		public void IsSupported_3digits() {
			ISupported sup = new SupportedXRange(40, 60, 888, 889);
			Assert.IsFalse(sup.IsSupported(new Phone(38, 88884444)), "#0");
			for (int ddd=40; ddd <= 60; ddd++)
				if (Phone.ValidDDD(ddd)) {
					for (int prefix=8870; prefix < 8880; prefix++)
						Assert.IsFalse(sup.IsSupported(new Phone(ddd, prefix * 10000)), "#1 {0}", prefix);
					for (int prefix=8880; prefix <= 8899; prefix++)
						Assert.IsTrue(sup.IsSupported(new Phone(ddd, prefix * 10000)), "#2 {0}", prefix);
					for (int prefix=8900; prefix < 8910; prefix++)
						Assert.IsFalse(sup.IsSupported(new Phone(ddd, prefix * 10000)), "#3 {0}", prefix);
				}
			Assert.IsFalse(sup.IsSupported(new Phone(61, 88884444)), "#4");
		}
	
	
		[Test]
		public void IsSupported_7digits() {
			ISupported sup = new SupportedXRange(40, 60, 1234567, 1234570);
			Assert.IsFalse(sup.IsSupported(new Phone(38, 12345678)), "#0");
			for (int ddd=40; ddd <= 60; ddd++)
				if (Phone.ValidDDD(ddd)) {
					for (int prefix=1234560; prefix < 1234567; prefix++)
						Assert.IsFalse(sup.IsSupported(new Phone(ddd, prefix * 10)), "#1 {0}", prefix);
					for (int prefix=1234567; prefix <= 1234570; prefix++)
						Assert.IsTrue(sup.IsSupported(new Phone(ddd, prefix * 10)), "#2 {0}", prefix);
					for (int prefix=1234571; prefix < 1234573; prefix++)
						Assert.IsFalse(sup.IsSupported(new Phone(ddd, prefix * 10)), "#3 {0}", prefix);
				}
			Assert.IsFalse(sup.IsSupported(new Phone(61, 12345678)), "#4");
		}
	}
}
