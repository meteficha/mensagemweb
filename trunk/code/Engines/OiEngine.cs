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
	// Inspired on code from Flávio R. Sampaio.
	// Updated with code from jSMS (www.jsms.com.br)
	public sealed class OiEngine : HttpEngine, IEngine {
		private static readonly ISupported valid = new SupportedList(new ISupported[]
			{new SupportedRange(21, 28, 86, 89), new SupportedRange(31, 38, 86, 89),
			 new SupportedRange(71, 79, 86, 89), new SupportedRange(81, 89, 86, 89),
			 new SupportedRange(91, 99, 86, 89)});
		ISupported IEngine.Valid { get { return valid; } }
		string IEngine.Name { get { return "Oi"; } }
		int IEngine.MaxTotalChars { get { return 151; } }
		
		protected override string BaseURL { get { return "http://torpedo.oiloja.com.br/"; } }
		private const string homeURL = "http://www.oiloja.com.br/index.jsp";
		
		private Dictionary<string, string> postData = new Dictionary<string, string>();
		
		void IEngine.Clear() {
			base.Clear();
		}
		
		EngineResult IEngine.SendMessage(Message msg, VerificationDelegate callback) {
			using (Response response = Get("captcha.jsp", BaseURL + "wb/POi/POi_oi_torpedo_mundooi"))
				response.CallVerification(callback);
			
			// Save the message data for the SendCode method
			postData.Clear();
			postData["para_ddd"] = msg.Destination.DDD.ToString();
			postData["para_numero"] = msg.Destination.Number.ToString();
			postData["de_nome"] = Util.ToSecureASCII(msg.FromName);
			postData["de_ddd"] = msg.FromDDD;
			postData["de_numero"] = msg.FromNumber;
			postData["mensagem"] = Util.ToSecureASCII(msg.Contents);
			
			return EngineResult.Success;
		}
		
		EngineResult IEngine.SendCode(string code) {
			code = code.Trim().Replace(" ", String.Empty);
			if (code.Length != 4)
				return EngineResult.WrongPassword;
			
			postData["captcha"] = Util.ToSecureASCII(code);
			using (Response response = Post("wb/POi/POi_oi_torpedo_mundooi", homeURL, postData)) {
				string text = response.Text;
				if (text.Contains("window.alert(\"Torpedo enviado com sucesso!\")"))
					return EngineResult.Success;
				else if (text.Contains("digo informado n"))
					return EngineResult.WrongPassword;
			}			
			return EngineResult.Unknown;
		}
	}
}
