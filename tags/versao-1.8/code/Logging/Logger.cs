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
using System.Threading;

namespace MensagemWeb.Logging {
	public class LoggedEventArgs : EventArgs {
		private object sender;
		private string message;
		
		public object Sender { get { return sender; } }
		public string Message { get { return message; } }
		
		public LoggedEventArgs(object sender_, string message_)
				: base() 
		{
			sender = sender_;
			message = message_;
		}
	}
	
	
	public delegate void LoggedEventHandler(object sender, LoggedEventArgs args);
	
	
	public static class Logger {
		private const int maxLogSize /* in messages */ = 100;
		private static List<string> logs = new List<string>(maxLogSize + 2);
		
		// Activated everytime someone calls Log() on this class. This event always comes
		// from the main thread.
		public static event LoggedEventHandler Logged;
		
		public static void Log(object sender, string message) {
			DateTime time = DateTime.Now;

#if !LITE
			Gtk.Application.Invoke(delegate { 
				Log(sender, message, time); 
			});
#else
			Log(sender, message, time);
#endif
		}
		
		public static void Log(object sender, string message, DateTime time) {
			lock (logs) {
				// Format the message
				string formatted;
				if (sender != null) {
					Type type = sender.GetType();
					if (type == type.GetType())
						formatted = String.Format("*** {0} @ {1} ***\n{2}", 
							sender, time, message);
					else
						formatted = String.Format("*** {0} ({1}) @ {2} ***\n{3}", 
							sender, type, time, message);
				} else
					formatted = String.Format("*** null (?) @ {0} ***\n{1}",
						time, message);
				
				// Print it first
				Console.Error.Write(formatted);
				Console.Error.Write("\n\n");
				Console.Error.Flush();
				
				// And then save it
				logs.Add(formatted);
				
				// Check if we had enough logging already
				CheckLogsCount();
				
				// Call everyone else
				if (Logged != null)
					Logged(typeof(Logger), new LoggedEventArgs(sender, formatted));
			}
		}
		
		
		public static void Log(Object sender, string formatString, params Object[] args) {
			Log(sender, String.Format(formatString, args));
		}
		
		
		public static string[] GetLogs() {
			lock (logs) {
				return logs.ToArray();
			}
		}
		
		public static void AppendLogs(IEnumerable<string> newlogs) {
			lock (logs) {
				logs.InsertRange(0, newlogs);
				CheckLogsCount();
			}
		}
		
		private static void CheckLogsCount() {
			int diff = logs.Count - maxLogSize;
			if (diff > 0)
				logs.RemoveRange(0, diff);
		}
	}
}
