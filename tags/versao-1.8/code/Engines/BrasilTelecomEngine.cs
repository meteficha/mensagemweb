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
	public sealed class BrasilTelecomEngine : HttpEngine, IEngine {
		private static readonly ISupported valid = new SupportedList(new ISupported[]
			{new SupportedRange(41, 49, 84, 84), new SupportedRange(51, 55, 84, 84),
			 new SupportedRange(61, 69, 84, 84)});
		ISupported IEngine.Valid { get { return valid; } }
		string IEngine.Name { get { return "BrasilTelecom"; } }
		int IEngine.MaxTotalChars { get { return 140; } }
		
		void IEngine.Clear() {
			base.Clear();
		}
		
		protected override string BaseURL { get { return "http://gsm.brasiltelecom.com.br/gsm/"; } }
		private const string homeURL = "http://www.brasiltelecom.com.br/home/Index.do";
		
		
		EngineResult IEngine.SendMessage(Message msg, VerificationDelegate callback) {
			Response response;
			
			Dictionary<string, string> postData = new Dictionary<string, string>(6);
			postData["iEstado"] = "00000000000000000000000000000000000000RS";
			postData["iCanal"] = "1";
			using (response = Post("site/home/mostrarHome.do", homeURL, postData)) { }
			
			postData.Clear();
			postData["sms.dddDestino"] = msg.Destination.DDD.ToString();
			postData["sms.numeroDestino"] = msg.Destination.Number.ToString();
			postData["sms.remetente"] = Util.ToSecureASCII(msg.FromName);
			postData["sms.dddOrigem"] = msg.FromDDD;
			postData["sms.numeroOrigem"] = msg.FromNumber;
			postData["sms.mensagem"] = Util.ToSecureASCII(msg.Contents);
			
			using (response = Post("site/home/sendSmsVertical.do", postData)) { }
			
			string url = lastURL;
			using (response = Get("secureImage?position=012345"))
				response.CallVerification(callback);
			lastURL = url;
			
			return EngineResult.Success;
		}
		
		EngineResult IEngine.SendCode(string code) {
			code = code.Trim().Replace(" ", String.Empty);
			if (code.Length != 6)
				return EngineResult.WrongPassword;
			
			Dictionary<string, string> data = new Dictionary<string, string>();
			data["secure"] = Util.ToSecureASCII(code.Replace(" ", String.Empty));
			using (Response response = Post("site/home/sendSmsVertical.do", data)) {
				string text = response.Text;
				if (text.Contains("Sua mensagem foi enviada com sucesso"))
					return EngineResult.Success;
				else if (text.Contains("vel enviar sua mensagem, alguns dos dados podem estar inv"))
					return EngineResult.PhoneNotEnabled;
				else if (text.Contains("Digite no campo abaixo a seq"))
					return EngineResult.WrongPassword;
			}
			
			return EngineResult.Unknown;
		}
	}
}
