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
	public sealed class CtbcEngine : HttpEngine, IEngine {
		private static readonly ISupported valid = new SupportedList(new ISupported[]
			{new SupportedXRange(16, 16, 9965, 9969), new SupportedXRange(16, 16, 9971, 9972),
			 new SupportedXRange(16, 16, 9978, 9979), new SupportedXRange(16, 16, 9989, 9989),
			 new SupportedXRange(16, 16, 9995, 9999), new SupportedXRange(17, 17, 9979, 9979),
			 new SupportedXRange(34, 34, 9661, 9665), new SupportedXRange(34, 34, 996, 997),
			 new SupportedXRange(34, 34, 999, 999),   new SupportedXRange(35, 35, 9991, 9991),
			 new SupportedXRange(37, 37, 997, 997),   new SupportedXRange(64, 64, 9661, 9661),
			 new SupportedXRange(64, 64, 9966, 9966), new SupportedXRange(64, 64, 9992, 9992),
			 new SupportedXRange(64, 64, 9999, 9999), new SupportedXRange(67, 67, 9966, 9966)});
		
		ISupported IEngine.Valid { get { return valid; } }
		string IEngine.Name { get { return "CTBC"; } }
		int IEngine.MaxTotalChars { get { return 149; } }
		protected override string BaseURL { get { return "http://www2.ctbcnet.com.br/webcel/"; } }
		
		
		private IDictionary<string, string> postData;
		private bool stage1 = false;
		private string imageSource;
		
		void IEngine.Clear() {
			base.Clear();
			stage1 = false;
		}
		
		EngineResult IEngine.SendMessage(Message msg, VerificationDelegate callback) {
			if (!stage1) {
				postData = new Dictionary<string, string>(6);
				postData["pre1"] = msg.Destination.DDD.ToString();
				postData["min"] = msg.Destination.Number.ToString();
				postData["sender"] = Util.ToSecureASCII(msg.FromName);
				if (msg.FromPhone != null)
					postData["sender"] += " " + msg.FromPhone.ToString();
				postData["msg"] = Util.ToSecureASCII(msg.Contents);
				
				using (Response response = Post("enviarnovo.php", BaseURL, postData)) {
					string html = response.Text;
					int start = html.IndexOf("geraimagem.php") + "geraimagem.php".Length;
					int stop = html.IndexOf("\"", start);
					imageSource = "geraimagem.php" + html.Substring(start, stop - start);
				
					stage1 = true;
				}
			} else
				Logger.Log(this, "Skipping stage1");
			
			string url = lastURL;
			using (Response response = Get(imageSource))
				response.CallVerification(callback);
			lastURL = url;
			
			return EngineResult.Success;
		}
		
		EngineResult IEngine.SendCode(string code) {
			postData["txtCode"] = code.Trim();
			postData["ddd"] = postData["pre1"];
			postData.Remove("pre1");
			
			using (Response response = Post("enviar2.php", postData)) {
				string html = response.Text;
				if (html.Contains("Mensagem enviada com sucesso"))
					return EngineResult.Success;
				else if (html.Contains("digo da figura informado est"))
					return EngineResult.WrongPassword;
				else if (html.Contains("o WEBCel ativado"))
					return EngineResult.PhoneNotEnabled;
			}
				
			return EngineResult.Unknown;
		}
	}
}