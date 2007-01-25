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
using System.Threading;

using MensagemWeb.Messages;
using MensagemWeb.Logging;

namespace MensagemWeb.Engines {
	public sealed class WaitEngine: EngineContainer, IEngine {
		// Default: 5 secs between messages
		public WaitEngine(IEngine engine) : this(engine, 5) { }
		public WaitEngine(IEngine engine, int waitTime) 
				: base(engine)
		{
			this.waitTime = new TimeSpan(0, 0, waitTime);
		}
		
		private TimeSpan waitTime;
		private DateTime lastMsg = DateTime.MinValue;		
				
		EngineResult IEngine.SendMessage(Message msg, VerificationDelegate callback) {
			// Enforces the time
			TimeSpan wait = waitTime - DateTime.Now.Subtract(lastMsg);
			if (wait.TotalSeconds > 0) {
				Logger.Log(this, "WaitEngine is forcing to wait {0}", wait);
				Thread.Sleep(wait);
			}
			return Engine.SendMessage(msg, callback);
		}
		
		EngineResult IEngine.SendCode(string code) {
			// Saves the time
			try {
				return Engine.SendCode(code);
			} finally {
				lastMsg = DateTime.Now;
			}
		}
	}
}
