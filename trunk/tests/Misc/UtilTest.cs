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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using NUnit.Framework;

using MensagemWeb;

namespace Tests.Misc {
	[TestFixture]
	public class UtilTest {
		
		[Test]
		public void Number() {
			Assert.AreEqual("uma", Util.Number(1, false), "#N1");
			Assert.AreEqual("dois", Util.Number(2, true), "#N2");
			Assert.AreEqual("sete", Util.Number(7, false), "#N3");
			Assert.AreEqual("vinte", Util.Number(20, true), "#N4");
			Assert.AreEqual("27", Util.Number(27, true), "#N5");
			Assert.AreEqual("nenhuma", Util.Number(0, false), "#N6");
			Assert.AreEqual("-1", Util.Number(-1, true), "#N7");
		}
		
		[Test]
		public void ToBytes() {
			Assert.AreEqual(new byte[] {97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107},
				Util.ToBytes("abcdefghijk"), "#TB1");
			Assert.AreEqual(new byte[] {65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75},
				Util.ToBytes("ABCDEFGHIJK"), "#TB2");
			Assert.AreEqual(new byte[] {32, 33, 64, 35, 36, 37, 42, 40, 41, 95, 43},
				Util.ToBytes(" !@#$%*()_+"), "#TB3");
			Assert.AreEqual(new byte[] {}, Util.ToBytes(""), "#TB4");
			// TODO: Test Unicode chars
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ToBytes_Null() {
			Util.ToBytes(null);
		}
		
		[Test]
		public void ToSecureASCII() {
			Assert.AreEqual("%61%62%63%64%65%66%67%68%69%6a%6b",
				Util.ToSecureASCII("abcdefghijk"), "#TSA1");
			Assert.AreEqual("%41%42%43%44%45%46%47%48%49%4a%4b",
				Util.ToSecureASCII("ABCDEFGHIJK"), "#TSA2");
			Assert.AreEqual("%20%21%40%23%24%25%2a%28%29%5f%2b",
				Util.ToSecureASCII(" !@#$%*()_+"), "#TSA3");
			Assert.AreEqual("", Util.ToSecureASCII(""), "#TSA4");
			// TODO: Test Unicode chars		
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ToSecureASCII_Null() {
			Util.ToSecureASCII(null);
		}
		
		[Test]
		public void Split() {
			string ret;
			
			// Normal cases
			ret = "A A A A A\nB B B B B\nC C C C C\nD D D D D";
			Assert.AreEqual(ret, Util.Split("A A A A A B B B B B C C C C C D D D D D", 9), "#SN1");
			Assert.AreEqual(ret, Util.Split("A A A A A B B B B B C C C C C D D D D D", 10), "#SN2");
			
			ret = "ABC ABCDE ABCDEF\nABCDEFG ABCDEFGH\nABCDEFGHI\nABCDEFGHIJ";
			Assert.AreEqual(ret, Util.Split("ABC ABCDE ABCDEF ABCDEFG ABCDEFGH ABCDEFGHI ABCDEFGHIJ", 16), "#SN3");
			
			ret = "1234567890\n1\n1234567890";
			Assert.AreEqual(ret, Util.Split("1234567890 1 1234567890", 11), "#SN4");
			
			// Very long words
			ret = "123\n1 1\n1234567890\n1\n1234\n1 1\n1234567901234";
			Assert.AreEqual(ret, Util.Split("123 1 1 1234567890 1 1234 1 1 1234567901234", 3), "#SL1");
			Assert.AreEqual(ret, Util.Split("123 1 1 1234567890 1 1234 1 1 1234567901234", 4), "#SL2");
				
			// Multiple spaces
			ret = "1 1 1 1 1\n123 123\n123";
			Assert.AreEqual(ret, Util.Split("1 1 1 1 1 123 123 123", 9), "#SM1");
			Assert.AreEqual(ret, Util.Split("1            1 1 1 1 123         123 123", 9), "#SM2");
			Assert.AreEqual(ret, Util.Split("1   1   1    1    1    123    123    123", 9), "#SM3");
			Assert.AreEqual(ret, Util.Split("\n1\n1\n1\n1\n1\n123\n123\n123\n", 9), "#SM4");
		}
		
		[Test]
		public void Replace_String() {
			Assert.AreEqual("C&amp;A", Util.Replace("C&A"), "#RS1");
			Assert.AreEqual("&lt;name ret=\"«€»\"/&gt;", Util.Replace("<name ret=\"«€»\"/>"), "#RS2");
			Assert.AreEqual("He &amp;amp; she &amp;gt;&amp;gt; there!",
				Util.Replace("He &amp; she &gt;&gt; there!"), "#RS3");
		}
		
		[Test]
		public void Replace_StringBuilder() {
			StringBuilder builder = new StringBuilder(30);
			
			builder.Append("C&A");
			Assert.AreSame(builder, Util.Replace(builder), "#RSB1A");
			Assert.AreEqual("C&amp;A", builder.ToString(), "#RSB1B");
			
			builder.Remove(0, builder.Length);
			builder.Append("<name ret=\"«€»\"/>");
			Assert.AreSame(builder, Util.Replace(builder), "#RSB2A");
			Assert.AreEqual("&lt;name ret=\"«€»\"/&gt;", builder.ToString(), "#RSB2B");
			
			builder.Remove(0, builder.Length);
			builder.Append("He &amp; she &gt;&gt; hey");
			Assert.AreSame(builder, Util.Replace(builder), "#RSB3A");
			Assert.AreEqual("He &amp;amp; she &amp;gt;&amp;gt; hey", builder.ToString(), "#RSB3B");
		}
		
		[Test]
		public void WriteProperty() {			
			// First write everything
			StringWriter stringWriter = new StringWriter();
			XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
			xmlWriter.WriteStartElement("test");
			
			Util.WriteProperty(xmlWriter, "test1", "");
			Util.WriteProperty(xmlWriter, "test2", "testing... =)");
			
			Dictionary<string,string> dic = new Dictionary<string,string> ();
			dic["param1"] = "value1";
			dic["param2"] = "my value 2";
			Util.WriteProperty(xmlWriter, "test3", "««««", dic);
			
			xmlWriter.WriteEndElement();
			xmlWriter.Close();
			
			// Now test the result
			StringReader stringReader = new StringReader(stringWriter.ToString());
			XmlTextReader xmlReader = new XmlTextReader(stringReader);
			Assert.IsTrue(xmlReader.Read(), "#WP1");
			Assert.AreEqual(XmlNodeType.Element, xmlReader.NodeType, "#WP2");
			Assert.AreEqual("test", xmlReader.Name, "#WP3");
			
			Assert.IsTrue(xmlReader.Read(), "#WP4");
			Assert.AreEqual(XmlNodeType.Element, xmlReader.NodeType, "#WP5");
			Assert.AreEqual("test1", xmlReader.Name, "#WP6");
			Assert.AreEqual("", xmlReader.ReadInnerXml(), "#WP7");
			
			Assert.AreEqual(XmlNodeType.Element, xmlReader.NodeType, "#WP8");
			Assert.AreEqual("test2", xmlReader.Name, "#WP9");
			Assert.AreEqual("testing... =)", xmlReader.ReadInnerXml(), "#WP10");
			
			Assert.AreEqual(XmlNodeType.Element, xmlReader.NodeType, "#WP11");
			Assert.AreEqual("test3", xmlReader.Name, "#WP12");
			Assert.AreEqual("value1", xmlReader["param1"], "#WP13");
			Assert.AreEqual("my value 2", xmlReader["param2"], "#WP14");
			Assert.AreEqual("««««", xmlReader.ReadInnerXml(), "#WP15");
			
			Assert.AreEqual(XmlNodeType.EndElement, xmlReader.NodeType, "#WP16");
			Assert.AreEqual("test", xmlReader.Name, "#WP17");
			Assert.IsFalse(xmlReader.Read());
		}
		
		[Test]
		public void GetSetProperty() {
			object obj;
			
			// Normal
			obj = new StringBuilder();
			Assert.IsTrue(Util.SetProperty(obj, "Capacity", 100), "#GSPN1");
			Assert.IsFalse(Util.SetProperty(obj, "Capacity", 4.4), "#GSPN2");
			Assert.AreEqual(100, Util.GetProperty(obj, "Capacity", -1), "#GSPN3");
			Assert.AreEqual(3.3, Util.GetProperty(obj, "Capacity", 3.3), "#GSPN4");
			Assert.AreEqual(default(double), Util.GetProperty<double>(obj, "Happy"), "#GSPN5");
			
			// Indexed			
			obj = new Dictionary<string,int> ();
			Assert.IsTrue(Util.SetProperty(obj, "Item", 10, new object[] {"oi"}), "#GSPI1");
			Assert.IsFalse(Util.SetProperty(obj, "Item", 11.3, new object[] {"oi"}), "#GSPI2");
			Assert.AreEqual(10, Util.GetProperty(obj, "Item", -1, new object[] {"oi"}), "#GSPI3");
			Assert.AreEqual(7.4, Util.GetProperty(obj, "Item", 7.4, new object[] {"oi"}), "#GSPI4");
			
			obj = "012345";
			Assert.AreEqual('3', Util.GetProperty(obj, "Chars", '#', new object[] {3}), "#GSPI5");
			Assert.AreEqual(10, Util.GetProperty(obj, "Chars", 10, new object[] {3}), "#GSPI6");
			Assert.AreEqual('#', Util.GetProperty(obj, "Chars", '#', new object[] {7}), "#GSPI7");
			Assert.AreEqual('#', Util.GetProperty(obj, "Chars", '#', new object[] {"oi"}), "#GSPI8");
			
			obj = new List<int>(new int[] {0, 1, 2, 3});
			Assert.AreEqual(2, Util.GetProperty(obj, "Item", -1, new object[] {2}), "#GSPI9");
			Assert.AreEqual(-1, Util.GetProperty(obj, "Item", -1, new object[] {5}), "#GSPI10");
			Assert.IsTrue(Util.SetProperty(obj, "Item", 0, new object[] {2}), "#GSPI11");
			Assert.IsFalse(Util.SetProperty(obj, "Item", 0, new object[] {5}), "#GSPI12");
			Assert.IsFalse(Util.SetProperty(obj, "Item", 9.4, new object[] {2}), "#GSPI13");
			Assert.AreEqual(0, Util.GetProperty(obj, "Item", -1, new object[] {2}), "#GSPI14");
		}
	}
}
