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
using System.IO;
using System.Collections.Generic; 

using MensagemWeb.Phones;
using MensagemWeb.Messages;

namespace MensagemWeb.Engines {
	public delegate void VerificationDelegate(Stream imageStream);

	public interface IEngine {
		string Name { get; }
		int MaxTotalChars { get; }
		ISupported Valid { get; }
		
		EngineResult SendMessage(Message msg, VerificationDelegate callback);
		EngineResult SendCode(string code);
		
		void Clear(); // Call this before sending *any* message 
		void Abort();
		bool Aborted { get; }
	}
	
	public enum EngineResultType {
		Success, 
		ServerMaintenence, 
		UserCancel, 
		WrongPassword, 
		PhoneNotEnabled, 
		MaxTryExceeded,
		LimitsExceeded,
		Unknown
	}
	
	public class EngineResult {
		// Source-level compatibility with most old code that expects an EngineResult enum
		public static readonly EngineResult Success = 
				new EngineResult(EngineResultType.Success);
		public static readonly EngineResult ServerMaintenence = 
				new EngineResult(EngineResultType.ServerMaintenence);
		public static readonly EngineResult UserCancel = 
				new EngineResult(EngineResultType.UserCancel);
		public static readonly EngineResult WrongPassword = 
				new EngineResult(EngineResultType.WrongPassword);
		public static readonly EngineResult PhoneNotEnabled = 
				new EngineResult(EngineResultType.PhoneNotEnabled);
		public static readonly EngineResult MaxTryExceeded = 
				new EngineResult(EngineResultType.MaxTryExceeded);
		public static readonly EngineResult LimitsExceeded = 
				new EngineResult(EngineResultType.LimitsExceeded);
		public static readonly EngineResult Unknown = 
				new EngineResult(EngineResultType.Unknown);
		
		// Properties
		protected EngineResultType type;
		protected string message;
		
		public EngineResultType Type { get { return type; } }
		public string Message { get { return message; } }
		
		// Constructors
		public EngineResult(EngineResultType type) : this(type, null) { }
		public EngineResult(EngineResultType type, string message) {
			this.type = type;
			this.message = message;
		}
		
		// Equality test
		public bool Equals(EngineResult other) {
			if (other == null) return false;
			if (other.Type != type) return false;
			if (message != null && other.message != null && this.message != other.message)
				return false;
			
			return true;
		}
		
		public override bool Equals(object obj) {
			return Equals(obj as EngineResult);
		}
		
		public override int GetHashCode() {
			return type.GetHashCode();
		}
		
		public static bool operator == (EngineResult x, EngineResult y) {
			return EngineResult.Equals(x, y);
		}
		
		public static bool operator != (EngineResult x, EngineResult y) {
			return !EngineResult.Equals(x, y);
		}
	}
}