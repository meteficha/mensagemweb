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

using MensagemWeb.Messages;

namespace Tests.Messages {

	[TestFixture]
	public class MessageTest {
//		[Test]
//		[ExpectedException(typeof(ArgumentOutOfRangeException))]
//		public void CtorEx1() {
//			new Phone(9, 12345678);
//		}
//	
//		[Test]
//		[ExpectedException(typeof(ArgumentOutOfRangeException))]
//		public void CtorEx2() {
//			new Phone(100, 12345678);
//		}
//	
//		[Test]
//		[ExpectedException(typeof(ArgumentOutOfRangeException))]
//		public void CtorEx3() {
//			new Phone(61, 9999999); /* 7 digits */
//		}
//	
//		[Test]
//		[ExpectedException(typeof(ArgumentOutOfRangeException))]
//		public void CtorEx4() {
//			new Phone(61, 100000000); /* 9 digits */
//		}
//	
//		[Test]
//		[ExpectedException(typeof(ArgumentException))]
//		public void CtorEx5() {
//			new Phone(39, 12345678);
//		}
//		
//		[Test]
//		public void Fields() {
//			Phone p = new Phone(61, 12345678);
//			Assert.AreEqual(61, p.DDD, "#1");
//			Assert.AreEqual(12345678, p.Number, "#2");
//			Assert.AreEqual(12, p.Prefix, "#3");
//		}
//		
//		// TODO: Test ValidDDD somehow
//		
//		[Test]
//		public void CtorFromStrings() {
//			Phone p = new Phone("61", "12345678");
//			Assert.AreEqual(61, p.DDD);
//			Assert.AreEqual(12345678, p.Number);
//			Assert.AreEqual(12, p.Prefix);
//		}
//		
//		[Test]
//		[ExpectedException(typeof(ArgumentOutOfRangeException))]
//		public void CtorFromStringsEx1() {
//			new Phone("100", "12345678");
//		}
//	
//		[Test]
//		[ExpectedException(typeof(ArgumentOutOfRangeException))]
//		public void CtorFromStringsEx2() {
//			new Phone("61", "9999999"); /* 7 digits */
//		}
//		
//		[Test]
//		[ExpectedException(typeof(ArgumentNullException))]
//		public void CtorFromStringsEx3() {
//			new Phone(null, "12345678");
//		}
//		
//		[Test]
//		[ExpectedException(typeof(ArgumentNullException))]
//		public void CtorFromStringsEx4() {
//			new Phone("61", null);
//		}
//		
//		[Test]
//		public void TestToString() {
//			Assert.AreEqual("(61) 1234-5678", new Phone(61, 12345678).ToString());
//		}
//		
//		[Test]
//		public void IComparable() {
//			Phone[] list = new Phone[] {
//				new Phone(38, 22223333),
//				new Phone(38, 44445555),
//				new Phone(61, 11112222),
//				new Phone(61, 55556666)};
//			foreach (Phone p in list)
//				Assert.AreEqual(0, (p as IComparable<Phone>).CompareTo(p), "#1 {0}", p);
//			for (int i = 0; i < list.Length - 1; i++)
//				Assert.IsTrue((list[i] as IComparable<Phone>).CompareTo(list[i+1]) < 0, "#2 {0}", i);
//		}
//		
//		[Test]
//		public void Equality() {
//			Phone[] list = new Phone[] {
//				new Phone(38, 22223333),
//				new Phone(38, 44445555),
//				new Phone(61, 11112222),
//				new Phone(61, 55556666)};
//			foreach (Phone p in list) {
//				Phone o = new Phone(p.DDD, p.Number);
//				Assert.IsTrue(p == o, "#1 {0}/{1}", p, o);
//				Assert.IsFalse(p != o, "#2 {0}/{1}", p, o);
//				Assert.IsTrue(p.Equals(o), "#3 {0}/{1}", p, o);
//				Assert.AreEqual(p.GetHashCode(), o.GetHashCode(), "#4 {0}/{1}", p, o);
//			}
//		}
	}
}
