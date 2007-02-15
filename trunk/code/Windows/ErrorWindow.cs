/*  Copyright (C) 2007 Felipe Almeida Lessa
    
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
using System.Text;

namespace MensagemWeb.Windows {	
	public sealed class ErrorWindow : Gtk.Dialog {
#pragma warning disable 649
		private Gtk.TextView textview;
#pragma warning restore 649

		public ErrorWindow(Gtk.Window parent, Exception e) {
			Stetic.Gui.Build(this, typeof(MensagemWeb.Windows.ErrorWindow));
			
			TransientFor = parent;
			
			StringBuilder text = new StringBuilder(1000);
			text.Append(" == EXCEÇÃO QUE FOI LANÇADA ==\n");
			text.Append(e == null ? "null" : e.ToString());
			text.Append("\n\n\n\n == INFORMAÇÕES DO LOG ==\n");
			try {
				foreach (string str in MensagemWeb.Logging.Logger.GetLogs()) {
					text.Append(str);
					text.Append("\n\n");
				}
			} catch (Exception e2) {
				text.Append(" --> Erro:\n");
				text.Append(e2.ToString());
			}
			
			Gtk.TextBuffer buffer = textview.Buffer;
			buffer.Text = text.ToString();
			buffer.SelectRange(buffer.StartIter, buffer.EndIter);
			textview.ScrollToIter(buffer.StartIter, 0.0, true, 0.0, 0.0);
		}
	}
}
