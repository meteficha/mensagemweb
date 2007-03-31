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
	public sealed class NextelEngine : HttpEngine, IEngine {
		private static readonly ISupported valid = new SupportedList(new ISupported[]
			{new SupportedRange(11, 11, 77, 78), new SupportedRange(12, 12, 78, 78),
			 new SupportedRange(13, 13, 78, 78), new SupportedRange(15, 15, 78, 78),
			 new SupportedRange(19, 19, 78, 78), new SupportedRange(21, 22, 78, 78),
			 new SupportedRange(24, 24, 78, 78), new SupportedRange(31, 31, 78, 78), 
			 new SupportedRange(41, 41, 78, 78), new SupportedRange(61, 62, 78, 78)});
		ISupported IEngine.Valid { get { return valid; } }
		string IEngine.Name { get { return "Nextel"; } }
		int IEngine.MaxTotalChars { get { return 119; } }
		
		protected override string BaseURL { get { return "http://www.nextel.com.br"; } }
		
		private bool stage1 = false;
		private Dictionary<string, string> postData;
		
		void IEngine.Clear() {
			base.Clear();
			stage1 = false;
		}
		
		EngineResult IEngine.SendMessage(Message msg, VerificationDelegate callback) {
			if (!stage1) {				
				using (Response response = Get("/default.aspx?page=SDTorpedoWeb")) {
					string text = response.Text;
					
					int start = text.IndexOf("name=\"__VIEWSTATE\" value=\"");
					start += "name=\"__VIEWSTATE\" value=\"".Length;
					int stop = text.IndexOf("\"", start + 1);
					postData["__VIEWSTATE"] = Util.ToSecureASCII(text.Substring(start, stop - start)); 
					
					postData = new Dictionary<string, string>(15);
					postData["rddNextel:_ctl2:Radpanelbar1value"] = String.Empty;
					postData["rddNextel:_ctl4:ddi"] = "55";
					postData["rddNextel:_ctl4:ddd"] = msg.Destination.DDD.ToString();
					postData["rddNextel:_ctl4:fone"] = msg.Destination.Number.ToString();
					int count = msg.FromName.Length;
					postData["rddNextel:_ctl4:from"] = Util.ToSecureASCII(msg.FromName);
					if (msg.FromPhone != null) {
						string fromPhone = msg.FromPhone.ToString();
						postData["rddNextel:_ctl4:from"] += " " + fromPhone;
						count += 1 + fromPhone.Length;
					}
					postData["rddNextel:_ctl4:subject"] = String.Empty;
					count += msg.Contents.Length;
					postData["rddNextel:_ctl4:message"] = Util.ToSecureASCII(msg.Contents);
					postData["rddNextel:_ctl4:count"] = (119 - count).ToString();
					postData["rddNextel:_ctl4:btnOk.x"] = "24";
					postData["rddNextel:_ctl4:btnOk.y"] = "3";
					
					stage1 = true;
				}
			} else
				Logger.Log(this, "Skipping stage1");
			
			string url = lastURL;
			using (Response response = Get("/Controles/wfGeraImagem.aspx"))
				response.CallVerification(callback);
			lastURL = url;
			
			return EngineResult.Success;
		}
		
		EngineResult IEngine.SendCode(string code) {
			code = code.Trim().Replace(" ", String.Empty);
			//if (code.Length != 4)
			//	return EngineResult.WrongPassword;
			postData["rddNextel:_ctl4:caracteres"] = code;
			using (Response response = Post("/default.aspx?page=SDTorpedoWeb", postData)) {
				string text = response.Text;			
				if (text.Contains("<script>alert('Mensagem enviada com sucesso!')</script>"))
					return EngineResult.Success;
				else if (text.Contains("digo: 5!')</script>"))
					return EngineResult.PhoneNotEnabled;
				else if (text.Contains("digo da imagem corretamente!')</script>"))
					return EngineResult.WrongPassword;
			}
			return EngineResult.Unknown;
		}
	}
}