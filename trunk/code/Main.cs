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
using System.Reflection;
using System.Threading;

using MensagemWeb.Config;
using MensagemWeb.Logging;
using MensagemWeb.Windows;

namespace MensagemWeb {
	public static class MainClass {
		private class ExitException : Exception {
			public ExitException()
				: base("\n\n\nThis exception is harmless, we're just exiting...\n\n")
			{ }
			
			public override string ToString() { return String.Empty; }
		}
		
		
		[STAThread]
		public static void Main (string[] args) {
			Thread.CurrentThread.Name = "MensagemWeb (main)";
			Logger.Log(typeof(MainClass), Assembly.GetCallingAssembly().FullName);
			Configuration.Load();
			
			try {
				Gtk.Application.Init();
								
				Gtk.Window.DefaultIconList = new Gdk.Pixbuf[] {
					new Gdk.Pixbuf(null, "icone48.png"),
					new Gdk.Pixbuf(null, "icone16.png"),
					new Gdk.Pixbuf(null, "icone64.png")};
				MainWindow.This.Present();
				
				Gtk.Application.Run();
			} catch (ExitException) {
				throw;
			} catch (Exception e) {
				// This is a general exception catcher that catches everything that the
				// real code couldn't handle or expect. In other word, this is our last resort
				// to any bugs that happen to throw an exception.
				
				// First log
				Logger.Log(typeof(MainClass), e.ToString());
				
				// Then try to show something (remember, Gtk# may be screwed)
				ErrorWindow w = new ErrorWindow(MainWindow.This, e);
				w.Run();
				w.Destroy();
				
				// Don't save anything, just exit
				throw;
			}
			
			Exit();
		}
		
		private static volatile bool exiting = false;
		public static void Exit() {
			if (exiting)
				return;
			else
				exiting = true;
				
			Configuration.Save();
			Logger.Log(typeof(MainClass), "MensagemWeb exiting...");
			
			// XXX Quit is causing random bugs on windows
			throw new ExitException();
//			Gtk.Application.Quit();
		}
	}
}
