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
 
#if !LITE
 
using System;
using Gtk;

using MensagemWeb.Windows;

namespace MensagemWeb.Messages {
	public sealed class MultipleMsgsDialog : Dialog {
		private static DialogFlags dialog_flags = DialogFlags.DestroyWithParent |
			DialogFlags.NoSeparator;
		private static object[] button_data = new object[] {Stock.Cancel, ResponseType.No,
			"_Enviar", ResponseType.Yes};		
		
		
		public MultipleMsgsDialog(Message[] msgsToSend)
				: base("", MainWindow.This, dialog_flags, button_data)
		{
			if (msgsToSend == null)
				throw new ArgumentNullException("msgsToSend");
			if (msgsToSend.Length < 2)
				throw new ArgumentException("msgsToSend");
				
			foreach (Widget w in this.ActionArea.AllChildren) {
				Button b = w as Button;
				if (b != null && b.Label == (button_data[2] as string)) {
					Util.SetProperty(b, "Image", new Image(Stock.GoForward, IconSize.Button));
					break;
				}
			}
			
			ActionArea.BorderWidth = 12;
			this.VBox.BorderWidth = 0;
			
			DefaultResponse = ResponseType.Yes;
			Modal = true;
			Resizable = true;
			
			HBox hbox = new HBox();
			VBox.Add(hbox);
			hbox.BorderWidth = 12;
			hbox.Spacing = 12;
			
			Image img = new Image(Stock.DialogQuestion, IconSize.Dialog);
			img.Yalign = 0;
			hbox.PackStart(img, false, false, 0);
			
			VBox vbox = new VBox();
			hbox.Add(vbox);
			vbox.Spacing = 12;
			
			Label titleLabel = new Label("");
			titleLabel.Markup = "<span size=\"large\" weight=\"bold\">Enviar mais de uma mensagem?</span>";
			titleLabel.Xalign = 0.0f;
			vbox.PackStart(titleLabel, false, true, 0);
			
			Label explLabel = new Label("Seu texto é muito longo para ser enviado\nem uma " +
			                            "mensagem apenas, por isso ele\nserá dividido em " +
			                            Util.Number(msgsToSend.Length, false) + " mensagens:");
			explLabel.Xalign = 0.0f;
			explLabel.Wrap = false;
			vbox.PackStart(explLabel, false, true, 0);
			
			
			Widget list = CreateList(msgsToSend);
			vbox.PackStart(list, true, true, 0);
			
			Gdk.Geometry geom = new Gdk.Geometry();
			geom.MinWidth = 150;
			geom.MinHeight = 150;
			this.SetGeometryHints(list, geom, Gdk.WindowHints.MinSize);
			
			hbox.ShowAll();
		}
		
		
		private Widget CreateList(Message[] msgs) {
			ListStore store = new ListStore(typeof(string), typeof(string));
			int index = 0;
			foreach (Message msg in msgs) {
				store.AppendValues("<span size=\"xx-large\" weight=\"bold\">" + ++index + "</span>",
				                   Util.Split(msg.Contents, 40));
			}
		
			TreeView treeview = new TreeView();			
			treeview.Model = store;
			treeview.Selection.Mode = SelectionMode.None;
			treeview.Reorderable = false;
			treeview.HeadersVisible = false;
			treeview.RulesHint = true;
			treeview.SearchColumn = 1; // Model's column
			
			TreeViewColumn col = new TreeViewColumn();
			col.Title = "Mensagem";
			CellRendererText cell = new CellRendererText();
			col.PackStart(cell, false);
			col.AddAttribute(cell, "markup", 0);
			cell = new CellRendererText();
			col.PackStart(cell, true);
			col.AddAttribute(cell, "text", 1);
			treeview.AppendColumn(col);
			
			ScrolledWindow sw = new ScrolledWindow();
			sw.BorderWidth = 0;
			sw.ShadowType = ShadowType.In;
			sw.Add(treeview);
			
			return sw;
		}
	}
}

#endif