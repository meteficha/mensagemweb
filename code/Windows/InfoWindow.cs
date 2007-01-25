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
using Gtk;

using MensagemWeb.Phones;
using MensagemWeb.Logging;

namespace MensagemWeb.Windows {
	public class InfoWindow : Window {
		private static InfoWindow this_;
		public static InfoWindow This {
			get {
				if (this_ == null)
					this_ = new InfoWindow();
				return this_; 
			}
		}
		
		private ListStore logsStore = new ListStore(typeof(string), typeof(string), typeof(string));
		private TreeView logsTreeView = new TreeView();
		private TextBuffer logsTextBuffer;
		
		
		
		private InfoWindow()
				: base("Informações de sistema")
		{
			// Some properties
			this.TypeHint = Gdk.WindowTypeHint.Normal;
			this.TransientFor = MainWindow.This;
			this.DestroyWithParent = true;
			this.WindowPosition = WindowPosition.CenterOnParent;
			this.Resizable = true;
			
			// Just hide us when the window is closed
			this.DeleteEvent += delegate (object sender, DeleteEventArgs a) {
				this.Hide();
				a.RetVal = true;
			};
			
			// We need to stop listening to log events when we're destroyed
			// to avoid calling Gtk objects that don't exist.
			this.DestroyEvent += delegate { Logger.Logged -= LogHandler; };
			
			// Main VBox
			VBox mainBox = new VBox();
			mainBox.Spacing = 12;
			mainBox.BorderWidth = 12;
			this.Add(mainBox);
			
			// The notebook
			Notebook notebook = new Notebook();
			mainBox.PackStart(notebook, true, true, 0);
			
				// First page -- miscelaneous information
				ListStore infoStore = new ListStore(typeof(string), typeof(string));
				TreeView infoTV = new TreeView();
				infoTV.Model = infoStore;
				infoTV.HeadersVisible = false;
				infoTV.AppendColumn("Propriedade", new CellRendererText(), "markup", 0);
				infoTV.AppendColumn("Valor", new CellRendererText(), "text", 1);
				ScrolledWindow infoSW = new ScrolledWindow();
				infoSW.ShadowType = ShadowType.In;
				infoSW.Add(infoTV);
				Gdk.Geometry geom = new Gdk.Geometry();
				geom.MinWidth = 333; geom.MinHeight = 180;
				this.SetGeometryHints(infoSW, geom, Gdk.WindowHints.MinSize);
				
				infoStore.AppendValues("<b>Nome completo</b>", 
					GetType().Assembly.FullName);
				infoStore.AppendValues("<b>Sistema operacional</b>", 
					Environment.OSVersion.ToString());
				infoStore.AppendValues("<b>Versão da máquina virtual</b>", 
					Environment.Version.ToString());
				infoStore.AppendValues("<b><i>Site</i> do MensagemWeb</b>",
					"http://mensagemweb.codigolivre.org.br/");
					
				notebook.AppendPage(infoSW, new Label("_Informações"));
				
				// Second page -- logs
				logsTreeView.CursorChanged += delegate (object sender, EventArgs args) {
					TreeIter iter;
					if (logsTreeView.Selection.GetSelected(out iter))
						logsTextBuffer.Text = (string) logsStore.GetValue(iter, 2);
					else
						logsTextBuffer.Text = String.Empty;
				};
				logsTreeView.RulesHint = true;
				logsTreeView.Model = logsStore;
				logsTreeView.HeadersVisible = true;
				logsTreeView.AppendColumn("Tempo", new CellRendererText(), "text", 1);
				logsTreeView.AppendColumn("Remetente", new CellRendererText(), "text", 0);
				ScrolledWindow logsTreeViewSW = new ScrolledWindow();
				logsTreeViewSW.ShadowType = ShadowType.In;
				logsTreeViewSW.Add(logsTreeView);
				
				foreach (string log in Logger.GetLogs()) {
					LogHandler(null, new LoggedEventArgs(null, log));
				}
				Logger.Logged += LogHandler;
				
				TextView logsTextView = new TextView();
				logsTextView.WrapMode = WrapMode.WordChar;
				logsTextView.Editable = false;
				logsTextBuffer = logsTextView.Buffer;
				ScrolledWindow logsTextViewSW = new ScrolledWindow();
				logsTextViewSW.ShadowType = ShadowType.In;
				logsTextViewSW.Add(logsTextView);
				
				VPaned logsPanel = new VPaned();
				logsPanel.Pack1(logsTreeViewSW, true, false);
				logsPanel.Pack2(logsTextViewSW, true, false);
				notebook.AppendPage(logsPanel, new Label("_Mensagens de sistema"));
				
			// Buttons
			ButtonBox buttonBox = new HButtonBox();
			buttonBox.Spacing = 6;
			buttonBox.Layout = ButtonBoxStyle.End;
			mainBox.PackStart(buttonBox, false, true, 0);
			
				Button closeButton = new Button(Stock.Close);
				closeButton.Clicked += delegate {
					this.Hide();
				};
				buttonBox.Add(closeButton);
		}
		
		
		private void LogHandler(object sender, LoggedEventArgs args) {
			string title = args.Message.Split('\n')[0];
			title = title.Substring(4, title.Length - 8);
			string[] values = title.Split('@');
			TreeIter iter = logsStore.AppendValues(values[0].Trim(), 
				values[1].Trim(), args.Message);
			if (sender != null)
				logsTreeView.SetCursor(logsStore.GetPath(iter), null, false);
		}
		
		public void ShowThis() {
			this.ShowAll();
			this.Present();
		}
	}
}
