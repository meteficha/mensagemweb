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

using MensagemWeb.Messages;
using MensagemWeb.Logging;

namespace MensagemWeb.Engines {
	// Based on code from jSMS (http://www.jsms.com.br/index.php). Thanks!
	public sealed class AmazoniaCelularEngine: TelemigEngine, IEngine {
		private static readonly ISupported valid = new SupportedRange(91, 99, 95, 99);
		ISupported IEngine.Valid { get { return valid; } }
		string IEngine.Name { get { return "Amazônia Celular"; } }
		
		// This is the important thing!!!
		protected override string WebsmsURL { get { return "websms.aspx?carrierId=2"; } }
	}
}