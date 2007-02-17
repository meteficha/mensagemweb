/*  Copyright (C) 2006-2007 Felipe Almeida Lessa
    
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
using System.Windows.Forms; 

using MensagemWeb.Config;
using MensagemWeb.Logging;

namespace MensagemWeb.Leve {
	public static class MainClass {
		public const string MainThread = "MensagemWeb (Leve)";
		
		[STAThread]
		public static void Main (string[] args) {
			Thread.CurrentThread.Name = MainThread;
			Logger.Log(typeof(MainClass), Assembly.GetCallingAssembly().FullName);
			Configuration.Load();
			
			try {
				Application.Run();
			} catch (Exception e) {
				//// This is a general exception catcher that catches everything that the
				//// real code couldn't handle or expect. In other word, this is our last resort
				//// to any bugs that happen to throw an exception.
				
				// First log
				Logger.Log(typeof(MainClass), e.ToString());
				
				// Then try to show something (remember, Gtk# may be screwed)
				MessageBox.Show("Ocorreu algum erro inesperado\n\n" +
				"Isso significa que existe algum problema interno no MensagemWeb que ainda\n" + 
				"não foi identificado, então nós gostaríamos que você procurasse ajuda\n" +
				"em nossos fóruns (visite nosso site em mensagemweb.codigolivre.org.br)\n" +
				"para que possamos resolvê-lo e evitar novos transtornos." +
								"\n\nInformações detalhadas" +
								" (se possível, copie e cole no seu pedido de ajuda):\n" + 
								e.ToString(),
								
								"Ocorreu algum erro inesperado", MessageBoxButtons.OK,
								MessageBoxIcon.Error);
				
				// Don't save anything, just exit
				throw e;
			}
			
			Exit();
		}
		
		public static void Exit() {
			// We don't save any configuration
			//Configuration.Save();
			
			// Log our stop time
			Logger.Log(typeof(MainClass), "MensagemLeve exiting...");
			
			// Quit
			Application.Exit();
		}
	}
}
