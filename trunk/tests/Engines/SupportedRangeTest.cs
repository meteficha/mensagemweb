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
	public class SupportedRangeTest {
	
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Ctor_InvalidDDD1() {
			new SupportedRange(0, 61, 90, 91);
		}
	
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Ctor_InvalidDDD2() {
			new SupportedRange(61, 100, 90, 91);
		}
	
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Ctor_InvalidDDD3() {
			new SupportedRange(0, 100, 90, 91);
		}
	
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Ctor_InvalidDDD4() {
			new SupportedRange(62, 61, 90, 91);
		}
	
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Ctor_InvalidPrefix1() {
			new SupportedRange(61, 61, 0, 91);
		}
	
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Ctor_InvalidPrefix2() {
			new SupportedRange(61, 61, 90, 100);
		}
	
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Ctor_InvalidPrefix3() {
			new SupportedRange(61, 61, 100, 91);
		}
	
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Ctor_InvalidPrefix4() {
			new SupportedRange(61, 61, -10, 1000);
		}
	
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Ctor_InvalidPrefix5() {
			new SupportedRange(61, 61, 90, 80);
		}
	
		[Test]
		public void IsSupported() {
			ISupported sup = new SupportedRange(40, 60, 80, 90);
			for (int ddd=40; ddd <= 60; ddd++)
				if (Phone.ValidDDD(ddd))
					for (int prefix=80; prefix <= 90; prefix++)
						Assert.IsTrue(sup.IsSupported(new Phone(ddd, prefix * 1000000)));
		}
	}
}
