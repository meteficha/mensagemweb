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

namespace MensagemWeb.Engines {
	// Simple abstract class from where the engine containers are subclassed (for convinience)
	public abstract class EngineContainer : IEngine {
		protected EngineContainer(IEngine engine) {
			if (engine == null)
				throw new ArgumentNullException("engine");
			this.Engine = engine;
		}
		
		// The contained engine
		public readonly IEngine Engine;
		
		// Forwarded by default
		string IEngine.Name { get { return Engine.Name; } }
		int IEngine.MaxTotalChars { get { return Engine.MaxTotalChars; } }
		void IEngine.Clear() { Engine.Clear(); }
		ISupported IEngine.Valid { get { return Engine.Valid; } }
		void IEngine.Abort() { Engine.Abort(); }
		bool IEngine.Aborted { get { return Engine.Aborted; } }
		
		// To be implemented
		EngineResult IEngine.SendMessage(Message message, VerificationDelegate callback) {
			throw new NotImplementedException();
		}
		EngineResult IEngine.SendCode(string code) {
			throw new NotImplementedException();
		}
		
		public override string ToString() { return Engine.Name; }
	}
}