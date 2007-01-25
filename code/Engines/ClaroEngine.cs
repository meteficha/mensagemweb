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
	public sealed class ClaroEngine : HttpEngine, IEngine {
		private static readonly ISupported valid = new SupportedList(new ISupported[] 
			{new SupportedRange(61, 69, 91, 95), new SupportedRange(71, 79, 81, 81),
			 new SupportedRange(61, 69, 91, 95), new SupportedRange(81, 89, 91, 94),
			 new SupportedRange(41, 49, 88, 88), new SupportedRange(21, 28, 91, 94),
			 new SupportedRange(51, 55, 91, 94), new SupportedRange(11, 11, 76, 76),
			 new SupportedRange(11, 19, 91, 94), new SupportedRange(31, 38, 84, 84)});
		ISupported IEngine.Valid { get { return valid; } }
		string IEngine.Name { get { return "Claro"; } }
		int IEngine.MaxTotalChars { get { return 146; } }
		
		private const string homeURL = "http://www.claroideias.com.br/portal/site/CIdeias/";
		protected override string BaseURL { 
			get { return "http://clarotorpedoweb.claro.com.br/ClaroTorpedoWeb/"; } 
		}
		
		private Dictionary<string, string> postData;
		private string imageSource;
		private bool stage1 = false;
		
		void IEngine.Clear() {
			base.Clear();
            imageSource = String.Empty;
			stage1 = false;
		}
		
		EngineResult IEngine.SendMessage(Message msg, VerificationDelegate callback) {
			if (!stage1) {
				postData = new Dictionary<string, string>();
				postData["ddd_para"] = msg.Destination.DDD.ToString();
				postData["telefone_para"] = msg.Destination.Number.ToString();
				postData["nome_de"] = Util.ToSecureASCII(msg.FromName);
				postData["ddd_de"] = msg.FromDDD;
				postData["telefone_de"] = msg.FromNumber;
				postData["msg"] = Util.ToSecureASCII(msg.Contents);
				
				using (Response response = Post("pwdForm.jsp", homeURL, postData)) {
					string text = response.Text;
					if (text.Contains("Sistema temporariamente indispon"))
						return EngineResult.ServerMaintenence;
					
					//Example: src='imageCode/g1gX2f82LW4SYS0UPPReg16LUNdKMS5KS6bSL32SV4T7MSNVY9aQY' 
					int start = text.IndexOf("src='imageCode/") + "src='".Length;
					int end = text.IndexOf("'", start);
					this.imageSource = text.Substring(start, end-start);
					
					stage1 = true;
				}
			} else
				Logger.Log(this, "Skipping stage1"); 
			
			using (Response response = Get(imageSource))
				response.CallVerification(callback);
			
			return EngineResult.Success;
		}
		
		
		
		private string FindCall(string head, string tail, string html) {
			int start = html.IndexOf(head);
			if (start < 0) return null;
			start += head.Length;
			int end = html.IndexOf(tail, start);
			if (end < start) return null;
			return html.Substring(start, end-start);
		}
		
		private string RetornoIdMessage(string html) {
			// parent.retornoIdMessage('ID:P<383945.1147338399876.0>');
			return FindCall("parent.retornoIdMessage('", "');", html);
		}
		
		private int? Retorno(string html) {
			// parent.retorno(20);
			string str = FindCall("parent.retorno(", ");", html);
			if (str != null)
				try {
					return Int32.Parse(str);
				} catch (FormatException) { /* Go ahead */ }
			return null;
		}
		
		EngineResult IEngine.SendCode(string code) {
			code = code.Trim().Replace(" ", String.Empty);
			if (code.Length != 4)
				return EngineResult.WrongPassword;
				
			postData["Image2.x"] = String.Empty;
			postData["Image2.y"] = String.Empty;
			postData["tx_senha"] = code.ToUpper();
			
			bool found = false;
			while (!found) {
				string html;
				using (Response response = Get("ValidDeliverer", BaseURL + "pwdForm.jsp", postData))
					html = response.Text;
				
				string id = RetornoIdMessage(html);
				if (id != null) {
					postData.Clear();
					postData["idmessage"] = id;
					continue;
				}
				
				// Parse the code the server returned
				try {
					int number = Retorno(html) ?? Convert.ToInt32(html);
					switch (number) {
						case 0:
							return EngineResult.Success;
	                    case 5:
							return EngineResult.WrongPassword;
						case 10:
							return EngineResult.PhoneNotEnabled;
					    case 26:
					    case 27:
					    case 28:
					    case 29:
					        return EngineResult.LimitsExceeded;
					    case 1111:
					    	// Message being sent, try again later
							System.Threading.Thread.Sleep(250);
							continue;
	                 }
					found = true;
				} catch (Exception e) {
					// Ignore any exceptions
	            	Logger.Log(this, e.ToString());
	            	break;
				}
			} // while (!found)
			
			return EngineResult.Unknown;
		}
	}
}
