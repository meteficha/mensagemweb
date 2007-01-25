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
	public class TelemigEngine : HttpEngine, IEngine {
		private static readonly ISupported valid = new SupportedRange(31, 38, 95, 99);
		ISupported IEngine.Valid { get { return valid; } }
		string IEngine.Name { get { return "Telemig"; } }
		int IEngine.MaxTotalChars { get { return 144; } }
		
		protected override string BaseURL { get { return "http://websms.telemigcelular.com.br/"; } }
		protected virtual string WebsmsURL { get { return "websms.aspx"; } }
		
		private bool stage1 = false;
		private Dictionary<string, string> postData;
		private string imageSource;
		
		void IEngine.Clear() {
			base.Clear();
			stage1 = false;
		}
		
		EngineResult IEngine.SendMessage(Message msg, VerificationDelegate callback) {
			if (!stage1) {				
				using (Response response = Get(WebsmsURL)) {
					string text = response.Text;
					
					postData = new Dictionary<string, string>(10);
					postData["ToDDDNumber"] = msg.Destination.DDD.ToString();
					postData["ToPhoneNumber"] = msg.Destination.Number.ToString();
					postData["FromPhoneNumber"] = Util.ToSecureASCII(msg.FromName);
					if (msg.FromPhone != null)
						postData["FromPhoneNumber"] += " " + msg.FromPhone.ToString(); 
					postData["Message"] = Util.ToSecureASCII(msg.Contents);
					
					postData["servico"] = "3";
					postData["operadora"] = "1";
					postData["btnSend"] = "Enviar";
					
					int start = text.IndexOf("name=\"__VIEWSTATE\" value=\"");
					start += "name=\"__VIEWSTATE\" value=\"".Length;
					int stop = text.IndexOf("\"", start + 1);
					postData["__VIEWSTATE"] = Util.ToSecureASCII(text.Substring(start, stop - start)); 
					
					start = text.IndexOf("getImage.aspx");
					stop = text.IndexOf("\"", start + 1);
					imageSource = text.Substring(start, stop - start);
					
					stage1 = true;
				}
			} else
				Logger.Log(this, "Skipping stage1");
			
			using (Response response = Get(imageSource))
				response.CallVerification(callback);
			
			return EngineResult.Success;
		}
		
		EngineResult IEngine.SendCode(string code) {
			code = code.Trim().Replace(" ", String.Empty);
			if (code.Length != 4)
				return EngineResult.WrongPassword;
			
			postData["ValidationKey"] = code;
			using (Response response = Post(WebsmsURL, BaseURL + WebsmsURL, postData)) {
				string text = response.Text;			
				if (text.Contains("&Message=MessageToSuccess"))
					return EngineResult.Success;
				else if (text.Contains("&Message=MessageToAccessErro"))
					return EngineResult.PhoneNotEnabled;
				else if (text.Contains("&Message=MessageToInvalidKey"))
					return EngineResult.WrongPassword;
			}			
			return EngineResult.Unknown;
		}
	}
}