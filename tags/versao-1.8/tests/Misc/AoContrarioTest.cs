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
using NUnit.Framework;

using AoContrario;

namespace Tests.Misc {

	[TestFixture]
	public class AoContrarioTest {
	
		[Test]
		public void Converter() {
			Assert.AreEqual("etset selpmis", Conversor.Converter("teste simples"), "#1");
			Assert.AreEqual("Aroga moc Salucsúiam", Conversor.Converter("Agora com Maiúsculas"), "#2");
			Assert.AreEqual("Epilef A. Assel", Conversor.Converter("Felipe A. Lessa"), "#3");
			Assert.AreEqual("Amu ahnirac :)!", Conversor.Converter("Uma carinha :)!"), "#4");
			Assert.AreEqual("a...b...c", Conversor.Converter("a...b...c"), "#5");
			Assert.AreEqual("Adn!IOfqq?!", Conversor.Converter("Nda!QQfoi?!"), "#6");
			Assert.AreEqual("Soremun:123123eheheh", Conversor.Converter("Numeros:123123hehehe"), "#7");
		}
	}
}
