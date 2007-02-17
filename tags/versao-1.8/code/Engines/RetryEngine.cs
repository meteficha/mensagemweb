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

using MensagemWeb.Messages;
using MensagemWeb.Logging;

namespace MensagemWeb.Engines {
	public sealed class RetryEngine: EngineContainer, IEngine {
		// Default limits: 45 seconds max and 3s of wait
		public RetryEngine(IEngine engine) : this(engine, 45, 3) { }
		public RetryEngine(IEngine engine, int retryTime, int wait)
				: base(engine)
		{
			this.retryTime = new TimeSpan(0, 0, retryTime);
			this.wait = new TimeSpan(0, 0, wait);
		}
		
		private TimeSpan retryTime;
		private TimeSpan wait;
			
					
		EngineResult IEngine.SendMessage(Message msg, VerificationDelegate callback) {
			DateTime start = DateTime.Now;
			int errors = 0;
			while (DateTime.Now.Subtract(start) <= retryTime && !Engine.Aborted) {
				try {
					return Engine.SendMessage(msg, callback);
				} catch (Exception e) {
					errors ++;
					Logger.Log(this, "RetryEngine caught an error ({0}) of type {1}" +
						" when running SendMessage on {2}.\n{3}", errors, e.GetType(), Engine, e);
					
					if (!Engine.Aborted) {
						if (errors % 3 == 0)
							Engine.Clear();
						System.Threading.Thread.Sleep(wait);
					}
				}
			}
			
			if (Engine.Aborted)
				return EngineResult.UserCancel;
			else
				return EngineResult.MaxTryExceeded;
		}
		
		
		EngineResult IEngine.SendCode(string code) {
			DateTime start = DateTime.Now;
			int errors = 0;
			while (DateTime.Now.Subtract(start) <= retryTime && !Engine.Aborted) {
				try {
					return Engine.SendCode(code);
				} catch (Exception e) {
					errors ++;
					Logger.Log(this, "RetryEngine caught an error ({0}) of type {1}" +
						" when running SendCode on {2}.\n{3}", errors, e.GetType(), Engine, e);
					
					if (!Engine.Aborted)
						System.Threading.Thread.Sleep(wait);                                                                               
				}
			}
			if (Engine.Aborted)
				return EngineResult.UserCancel;
			else
				return EngineResult.MaxTryExceeded;
		}
	}
}
