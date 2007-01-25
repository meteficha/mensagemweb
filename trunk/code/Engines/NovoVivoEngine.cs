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
	public sealed class NovoVivoEngine : HttpEngine, IEngine {
		private static readonly ISupported valid = new SupportedList(new ISupported[]
			{new SupportedRange(11, 15, 95, 99), new SupportedRange(11, 71, 74, 74),
			 new SupportedRange(18, 19, 95, 99), new SupportedRange(21, 28, 95, 99),
			 new SupportedRange(41, 49, 91, 92), new SupportedRange(51, 55, 95, 99),
			 new SupportedRange(61, 69, 96, 99), new SupportedRange(71, 79, 96, 99),
			 new SupportedRange(91, 99, 91, 94)});
		ISupported IEngine.Valid { get { return valid; } }
		string IEngine.Name { get { return "Vivo"; } }
		int IEngine.MaxTotalChars { get { return 75; } }
		
		private const string homeURL = "http://www.vivo.com.br/portal/home.php";
		protected override string BaseURL {
			get { return "http://www.portal-sva.vivo.com.br/torpedo/"; }
		}
		
		private static readonly EngineResult VivoPhoneNotEnabled = 
			new EngineResult(EngineResultType.PhoneNotEnabled, "Observe que a Vivo não permite o " +
				"envio de mensagens para celulares dos estados de Espírito Santo, Paraná, Rio " + 
				"de Janeiro, Santa Catarina e São Paulo.");
		
		private IDictionary<string, string> postData;
		private bool stage1 = false;
		
		void IEngine.Clear() {
			base.Clear();
			stage1 = false;
		}
		
		EngineResult IEngine.SendMessage(Message msg, VerificationDelegate callback) {
			if (!stage1) {
				postData = new Dictionary<string, string>();
				postData["operadora"] = msg.Destination.DDD.ToString();
				postData["numTelefone"] = msg.Destination.Number.ToString();
				postData["nome"] = Util.ToSecureASCII(msg.FromName);
				postData["operadoraContato"] = msg.FromDDD;
				postData["numTelefoneContato"] = msg.FromNumber;
				postData["mensagem"] = Util.ToSecureASCII(msg.Contents);
				
				using (Response response = Get("TorpedoForm.do", homeURL, postData)) {
					if (response.Text.Contains("rio invalido") && response.Text.Contains("Destinat"))
						return VivoPhoneNotEnabled;
					stage1 = true;
				}
			} else
				Logger.Log(this, "Skipping stage1");
			
			string url = lastURL;
			using (Response response = Get("imagenumber?a=d"))
				response.CallVerification(callback);
			lastURL = url;
			
			return EngineResult.Success;
		}
		
		EngineResult IEngine.SendCode(string code) {
			postData["password"] = code.ToUpper();
			using (Response response = Post("SendSMS.do", postData)) {
				string text = response.Text;
				if (text.Contains("Torpedo enviado com sucesso!"))
					return EngineResult.Success;
				else if (text.Contains("A palavra informada deve ser igual a imagem gerada no for"))
					return EngineResult.WrongPassword;
				else if (text.Contains("Destinat"))
					return VivoPhoneNotEnabled;
				else if (text.Contains("o encontra-se indisponivel no momento. Tente Mais tarde."))
					return EngineResult.ServerMaintenence;
			}				
			return EngineResult.Unknown;
		}
	}
}
