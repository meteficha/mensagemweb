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
using Gtk;

using MensagemWeb.Logging;

namespace MensagemWeb.Windows {
	public class UpdateWindow : Window {
		private static UpdateWindow this_;
		public static UpdateWindow This {
			get {
				if (this_ == null)
					this_ = new UpdateWindow();
				return this_; 
			}
		}
		
		private RichTextBuffer newTB, oldTB;
		private Notebook notebook = new Notebook();
		private const string updating = "\tProcurando no site do MensagemWeb por novas versões, " +
										"por favor aguarde um momento...";
		
		
		private UpdateWindow()
				: base("Atualizações")
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
			this.KeyReleaseEvent += delegate (object o, KeyReleaseEventArgs args) {
				if (args.Event.Key == Gdk.Key.Escape)
					this.Hide();
			};
			
			// Main VBox
			VBox mainBox = new VBox();
			mainBox.Spacing = 12;
			mainBox.BorderWidth = 12;
			this.Add(mainBox);
			
			// The notebook
			mainBox.PackStart(notebook, true, true, 0);
			Gdk.Geometry geom = new Gdk.Geometry();
			geom.MinWidth = 570; geom.MinHeight = 400;
			this.SetGeometryHints(notebook, geom, Gdk.WindowHints.MinSize);
			
				notebook.AppendPage(RichTextBuffer.NewTextView(out newTB), 
					new Label("Versões _novas"));
				notebook.AppendPage(RichTextBuffer.NewTextView(out oldTB), 
					new Label("Versões _antigas"));
				
			// Buttons
			ButtonBox buttonBox = new HButtonBox();
			buttonBox.Spacing = 6;
			buttonBox.Layout = ButtonBoxStyle.End;
			mainBox.PackStart(buttonBox, false, true, 0);
			
				Button websiteButton = new Button("Ir para nosso site");
				websiteButton.Clicked += delegate {
					Util.OpenLink("http://mensagemweb.codigolivre.org.br/");
				};
				buttonBox.Add(websiteButton);
				
				Button closeButton = new Button(Stock.Close);
				closeButton.Clicked += delegate {
					this.Hide();
				};
				buttonBox.Add(closeButton);
		}
		
		
		public void ShowThis(bool AutomaticUpdates) {
			// Present the window if needed
			if (!AutomaticUpdates) {
				newTB.Clear();
				newTB.AddBoldText(updating);
				oldTB.Clear();
				oldTB.AddBoldText(updating);
				
				this.ShowAll();
				this.Present();
			} else
				Logger.Log(this, "Starting automatic check...");
			
			// Do everything in background
			Thread t = new Thread(delegate () {
				UpdateManager.CheckForUpdates();
				
				Gtk.Application.Invoke(delegate {
					// Update the interface on Gtk's thread
					if (AutomaticUpdates) {
						if (UpdateManager.Updates != null)
							UpdateManager.LastAutomaticCheck = DateTime.Now;
						if (UpdateManager.NewVersions) {
							Logger.Log(this, "Newer versions avaiable, showing myself.");
							this.ShowAll();
							this.Present();
						} else
							Logger.Log(this, "Nothing new was found.");
					}
					UpdateBuffers();
					notebook.CurrentPage = 0;
				});
			});
			t.Name = "UpdateWindow thread";
			t.Start();
		}
		
		private void UpdateBuffers() {
			Update[] updates = UpdateManager.Updates;
			newTB.Clear();
			oldTB.Clear();
			
			if (updates == null) {
				// TODO Do something better here
				newTB.AddTitle("Erro desconhecido");
				newTB.AddText("Não foi possível acessar a lista de atualizações.");
				return;
			} 
			
			Version ourVersion = UpdateManager.CurrentVersion;
			int lastIndex = updates.Length - 1;
			int splitIndex = lastIndex;
			for (; splitIndex >= 0; splitIndex--) {
				if (updates[splitIndex].Version <= ourVersion)
					break;
			}
			
			if (splitIndex == lastIndex) {
				// No new versions
				newTB.AddTitle("Nenhuma versão nova disponível");
				newTB.AddText("\tParabéns! Você já possui a versão mais nova " +
					"do MensagemWeb em seu computador!");
				oldTB.AddTitle("Versões antigas");
				Array.Reverse(updates);
				Fill(oldTB, updates);
			} else {
				// There are some new, some older
				Update[] newer = new Update[lastIndex - splitIndex];
				Array.Copy(updates, splitIndex + 1, newer, 0, newer.Length);
				Update[] older = new Update[splitIndex + 1];
				Array.Copy(updates, 0, older, 0, older.Length);
				Array.Reverse(older);
				
				newTB.AddTitle("Existe uma nova versão disponível!");
				newTB.AddText("\tVocê atualmente está usando a versão ");
				newTB.AddBoldText(VersionToString(ourVersion));
				newTB.AddText(", sendo que a mais nova é a versão ");
				newTB.AddBoldText(newer[newer.Length-1].Version.ToString());
				newTB.AddText(". Aconselhamos que você vá assim que possível ao site ");
				newTB.AddMensagemWebSite();
				newTB.AddText(" e pegue a versão mais nova do MensagemWeb.\n\t" +
					"Veja abaixo o que há de novo:\n");
				Fill(newTB, newer);
				
				oldTB.AddTitle("Versões antigas");
				Fill(oldTB, older);
			}
		}
		
		private void Fill(RichTextBuffer buffer, Update[] updates) {
			foreach (Update update in updates) {
				buffer.AddSubtitle(String.Format("MensagemWeb {0} ({1})", 
					update.Version, update.Released.ToShortDateString()));
				foreach (string line in update.Changes.Split('\n')) {
					string trimmed = line.Trim();
					if (trimmed[0] == '*')
						buffer.AddItem(trimmed.Substring(1).Trim());
					else
						buffer.AddText("\t" + trimmed);
					buffer.AddText("\n");
				}
			}
		}
		
		private string VersionToString(Version version) {
			string result = version.ToString().TrimEnd('0', '.');
			if (!result.Contains("."))
				result += ".0";
			return result;
		}
	}
}
