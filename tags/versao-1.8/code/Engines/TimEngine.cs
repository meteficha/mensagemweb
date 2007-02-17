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

using MensagemWeb.Messages;
using MensagemWeb.Logging;

namespace MensagemWeb.Engines {
	public sealed class TimEngine : IEngine {
		private static readonly ISupported valid = new SupportedList(new ISupported[]
			{new SupportedRange(11, 19, 81, 85), new SupportedRange(21, 28, 81, 83),
			 new SupportedRange(31, 38, 91, 94), new SupportedRange(41, 49, 95, 99), 
			 new SupportedRange(51, 55, 81, 83), new SupportedRange(61, 69, 81, 83), 
			 new SupportedRange(71, 79, 91, 94), new SupportedRange(81, 89, 95, 99),
			 new SupportedRange(91, 99, 81, 83)});
			 
		ISupported IEngine.Valid { get { return valid; } }
		string IEngine.Name { get { return "TIM (não suportada)"; } }
		int IEngine.MaxTotalChars { get { return 156; } }
		
		private bool aborted = false;
		
		bool IEngine.Aborted { get { return aborted; } }
		void IEngine.Abort() { aborted = true; }
		void IEngine.Clear() { aborted = false; }
		
		private static readonly EngineResult TimNotSupported = 
			new EngineResult(EngineResultType.PhoneNotEnabled, "A TIM não permite que " +
				"sejam enviadas mensagens de graça para seus números, assim o MensagemWeb " + 
				"não pode enviar sua mensagem.");
		
		EngineResult IEngine.SendMessage(Message msg, VerificationDelegate callback) {
			return TimNotSupported;
		}
		
		EngineResult IEngine.SendCode(string code) {
			throw new NotImplementedException("Shouldn't get here.");
		}
	}
}
