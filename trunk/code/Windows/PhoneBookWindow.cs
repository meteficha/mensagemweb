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
using System.IO;
using Gtk;

using MensagemWeb.Phones;
using MensagemWeb.Logging;

namespace MensagemWeb.Windows {
	public class PhoneBookWindow : Window {
		private static PhoneBookWindow this_;
		public static PhoneBookWindow This {
			get {
				if (this_ == null)
					this_ = new PhoneBookWindow();
				return this_; 
			}
		}
		
		// Tooltips
		private Tooltips tooltips = new Tooltips();
		private void SetTip(Widget wd, string text) {tooltips.SetTip(wd, text, text);}
		
		// Widgets
		private TreeView treeView = new TreeView();
		private ListStore treeStore = new ListStore(typeof(string), typeof(string), 
			typeof(string), typeof(string));
		private Button editButton, removeButton, exportButton;
				
				
		
		// This affects/reflects the current selection
		private IList<string> Destinations {
			get {
				TreePath[] selected = treeView.Selection.GetSelectedRows();
				string[] destinations = new string[selected.Length];
				TreeIter iter;
				for (int i = 0; i < selected.Length; i++) {
					if (treeStore.GetIter(out iter, selected[i])) {
						destinations[i] = treeStore.GetValue(iter, 0) as string;
					} else {
						Logger.Log(this, "Strange. {0}?", selected[i]);
						return null;
					}
				}
				return destinations;
			}
			
			set {
				// We can take just the names because they're guaranteed to be unique
				List<string> names = new List<string>(value.Count);
				foreach (string dest in (IEnumerable<string>)value)
					if (dest != null)
						names.Add(dest);
				names.Sort();
				
				treeView.Selection.Changed -= SelectionChanged;
				try {
					TreeIter iter;
					treeView.Selection.UnselectAll();
					if (!treeStore.GetIterFirst(out iter))
						return;
					do {
						string name = treeStore.GetValue(iter, 0) as string;
						int where = names.BinarySearch(name);
						if (where >= 0) {
							treeView.Selection.SelectIter(iter);
							names.RemoveAt(where); // XXX: O(n)!!!
							if (names.Count == 0)
								break;
						}
					} while (treeStore.IterNext(ref iter));
				} finally {
					treeView.Selection.Changed += SelectionChanged;
					SelectionChanged(null, null);
				}
			}
		}
		
		
		
		private PhoneBookWindow()
				: base("Agenda de telefones")
		{
			// Some properties
			this.TransientFor = MainWindow.This;
			this.DestroyWithParent = true;
			this.WindowPosition = WindowPosition.CenterOnParent;
			this.Modal = true;
			this.TypeHint = Gdk.WindowTypeHint.Dialog;
			this.DefaultSize = new Gdk.Size(573, 357); // Magic size -- *DONT*CHANGE*!
			PhoneBook.Updated += UpdateList;
			
			// Just hide us when the window is closed
			this.DeleteEvent += delegate (object sender, DeleteEventArgs a) {				
				this.Hide();
				if (Util.OnWindows)
					MainWindow.This.Present();
				a.RetVal = true;
			};
			
			// Updates MainWindows's list of destinations (previously in SelectionChanged method)
			this.Hidden += delegate {
				MainWindow.This.Destinations = this.Destinations;
			};
			
			// HBox that will hold the widgets
			Box box = new HBox();
			box.BorderWidth = 12;
			box.Spacing = 12;
			this.Add(box);
			
			// TreeView that will hold the phones
			treeView.Model = treeStore;
			treeView.Selection.Mode = SelectionMode.Multiple;
			treeView.Reorderable = false;
			treeView.HeadersVisible = true;
			treeView.RulesHint = true;
			treeView.SearchColumn = 0; // Model's column, not TreeView's!
			
			TreeViewColumn first = new TreeViewColumn();
			first.Title = "Nome";
			CellRenderer cell = new CellRendererPixbuf();
			first.PackStart(cell, false);
			first.AddAttribute(cell, "stock_id", 3);
			cell = new CellRendererText();
			first.PackStart(cell, true);
			first.AddAttribute(cell, "text", 0);
			treeView.AppendColumn(first); 
			
			TreeViewColumn[] c = new TreeViewColumn[3];
			c[0] = first;
			c[1] = treeView.AppendColumn("Telefone", new CellRendererText(), "markup", 1);
			c[2] = treeView.AppendColumn("Operadora", new CellRendererText(), "markup", 2);
			treeView.RowActivated += delegate {	this.Hide(); };
			
			int i = 0;
			foreach (TreeViewColumn column in c) {
				column.SortIndicator = true;
				column.SortColumnId = i++;
				column.Resizable = true;
			}
			
			c[0].Click();
			
			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = ShadowType.In;
			sw.Add(treeView);
			box.PackStart(sw, true, true, 0);
						
			// The buttons
			ButtonBox buttonBox = new VButtonBox();
			buttonBox.Spacing = 6;
			buttonBox.Layout = ButtonBoxStyle.Start;
			box.PackStart(buttonBox, false, true, 0);
			
			// "Add" button
			Button addButton = new Button(Stock.Add);
			SetTip(addButton, "Adiciona um novo número à sua agenda de telefones");
			addButton.Clicked += delegate {
				NewPhoneWindow.This.ClearAndShow();
			};
			buttonBox.Add(addButton);
			
			// "Edit" button
			editButton = new Button(Util.GetProperty(typeof(Stock), "Edit", "gtk-edit"));
			SetTip(editButton, "Edita as informações do telefone selecionado");
			editButton.Clicked += delegate {
				TreeIter iter; TreePath path;
				path = treeView.Selection.GetSelectedRows()[0];
				if (treeStore.GetIter(out iter, path)) {
					string name = (string)treeStore.GetValue(iter, 0);
					EditPhoneWindow.This.Edit(name);
				}
			};
			buttonBox.Add(editButton);
			
			// "Remove" button
			removeButton = new Button(Stock.Remove);
			SetTip(removeButton, "Remove o telefone selecionado de sua agenda de telefones");
			removeButton.Clicked += RemoveClicked;
			buttonBox.Add(removeButton);
			
			// Separator
			buttonBox.Add(new Alignment(0, 0, 0, 0));
						
			// "Export" button
			exportButton = new Button(Stock.SaveAs);
			SetTip(exportButton, "Exporta toda a sua lista de telefones para um arquivo vCard");
			exportButton.Clicked += ExportClicked;
			buttonBox.Add(exportButton);
			
			// "Import" button
			Button importButton = new Button(Stock.Open);
			SetTip(importButton, "Importa uma lista de telefones de um arquivo vCard");
			importButton.Clicked += ImportClicked;
			buttonBox.Add(importButton);
						
			// Separator
			buttonBox.Add(new Alignment(0, 0, 0, 0));
			
			// "Close" button
			Button closeButton = new Button(Stock.Close);
			SetTip(closeButton, "Fecha esta janela e volta para a janela de envio de mensagem");
			closeButton.Clicked += delegate {
				this.Hide();
			};
			buttonBox.Add(closeButton);
								
			// This is here just to make ShowThis work correctly
			treeView.Selection.Changed += SelectionChanged;
		}
		
		
		
		public void ShowThis() {
			// Don't lose the user's selection
			IList<string> selected = MainWindow.This.Destinations;
			UpdateList(null, null);
			Destinations = selected;
			
			// Show the window
			treeView.GrabFocus();
			this.ShowAll();
			this.Present();
		}
		
		
		
		private void SelectionChanged(object sender, EventArgs args) {
			int destsCount = Destinations.Count;
			editButton.Sensitive = (destsCount == 1);
			removeButton.Sensitive = (destsCount >= 1);
			exportButton.Sensitive = PhoneBook.Count > 0;
		}
		
		
		
		private void UpdateList(object sender, EventArgs args) {
			// Avoid problems with event we generate
			treeView.Selection.Changed -= SelectionChanged;
			
			try {
				// Clear the treeStore
				treeStore.Clear();
				
				// Add the names to the treeStore
				foreach (KeyValuePair<string, PhoneContainer> kvp in PhoneBook.NamesPhones) {
					PhoneContainer cont = kvp.Value;
					Phone phone = cont.Phone;
					string engineName;
					if (cont.Engine == null)
						try {
							engineName = phone.Engine.Name;
						} catch {
							engineName = "<i>Desconhecido</i>";
						}
					else
						// Forced engine
						engineName = "<b>" + cont.Engine.Name + "</b>";	
					treeStore.AppendValues(kvp.Key, phone.ToString(), engineName, "emblem-people");
				}
			} finally {
				// We're finished
				treeView.Selection.Changed += SelectionChanged;
				SelectionChanged(this, EventArgs.Empty);
			}
		}
		
		
		
		
		private void ImportClicked(object sender, EventArgs args) {
			// See whats the default DDD
			int? defaultDDD = PhoneBook.DefaultDDD;
			string ddd;
			if (defaultDDD.HasValue) {
				ddd = defaultDDD.Value.ToString();
			} else {
				Dialog d = new Dialog("Digite DDD padrão", this, DialogFlags.Modal |
										DialogFlags.DestroyWithParent | DialogFlags.NoSeparator,
										Stock.Cancel, ResponseType.Cancel,
										Stock.Ok, ResponseType.Ok);
				
				VBox vbox = new VBox(false, 6);
				vbox.BorderWidth = 12;
				d.VBox.Add(vbox);
				
				Label titleLabel = new Label("<span size=\"large\" weight=\"bold\">Digite o " +
							 				 "DDD padrão</span>");
				titleLabel.UseMarkup = true;
				titleLabel.Justify = Justification.Center;
				vbox.PackStart(titleLabel, false, true, 0);
				
				Label explLabel = new Label("Por favor, insira abaixo o número de DDD que " +
					"será utilizado sempre que houver um contato sem um número de DDD " + 
					"especificado explicitamente.");
				explLabel.Wrap = true;
				vbox.PackStart(explLabel, false, true, 0);											
										
				Label dddLabel = new Label("<b>Insira o DDD padrão:</b>");
				dddLabel.UseMarkup = true;
				dddLabel.Xalign = 1;
				Entry dddEntry = new Entry();
				dddEntry.MaxLength = 2;
				dddEntry.WidthChars = 2;
				dddEntry.Xalign = 0;
				HBox dddBox = new HBox(false, 7);
				dddBox.PackStart(dddLabel, false, false, 0);
				dddBox.PackStart(dddEntry, false, false, 0);
				vbox.PackStart(dddBox, false, true, 0);
				
				vbox.BorderWidth = 12;
				vbox.Spacing = 12;
				vbox.ShowAll();
				d.DefaultResponse = ResponseType.Ok;
				int r = d.Run();
				try {
					if (r == (int) ResponseType.Ok) {
						ddd = dddEntry.Text;
						try {
							if (!Phone.ValidDDD(UInt16.Parse(ddd)))
								ddd = null;
						} catch (Exception) {
							ddd = null;
						}
						if (ddd == null) {
							Util.ShowMessage(this, "Número DDD inválido", "O número DDD " +
								"digitado é inválido, a operação de importação será " +
								"cancelada.", MessageType.Error, false);
							return;
						}
					} else
						return;
				} finally {
					d.Destroy();
				}
			}
			
			// Open the file
			FileChooserDialog fc = new FileChooserDialog("Escolha de onde abrir a " +
														"lista de contatos",
														this,
														FileChooserAction.Open,
														Stock.Cancel, ResponseType.Cancel,
														Stock.Open, ResponseType.Accept);
			fc.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			fc.Filter = new FileFilter();
			fc.Filter.AddMimeType("text/vcard");
			fc.Filter.AddMimeType("text/x-vcard");
			fc.Filter.AddPattern("*.vcf");
			
			int response = fc.Run();
			try {
				if (response == (int) ResponseType.Accept) {
					string filename = fc.Filename;
					Logger.Log(this, "Importing vCard from {0}", filename);
					using (FileStream file = File.OpenRead(filename)) {
						using (StreamReader reader = new StreamReader(file)) {
							string[] names = VCard.VCardToPhoneBook(reader.ReadToEnd(), ddd);
							System.Text.StringBuilder msgstr = new System.Text.StringBuilder(200);
							if (names == null || names.Length == 0)
								msgstr.Append("Nenhum contato foi importado.");
							else if (names.Length == 1) {
								msgstr.Append("O seguinte contato foi importado com sucesso: <b>");
								msgstr.Append(names[0]);
								msgstr.Append("</b>.");
							} else {
								Array.Sort<string>(names);
								msgstr.Append("Os seguintes contatos foram importados com sucesso: <b>");
								msgstr.Append(names[0]);
								for (int i = 1; i < names.Length - 1; i++) {
									msgstr.Append("</b>,<b> ");
									msgstr.Append(Util.Replace(names[i]));
								}
								msgstr.AppendFormat(" </b>e<b> {0}</b>.", names[names.Length-1]);
							}
							Util.ShowMessage(this, "Resultado da importação", msgstr.ToString(),
											MessageType.Info, false);
						}
					}
				}
			} finally {
				fc.Destroy();
			}
		}
	
	
	
	
			
		private void ExportClicked(object sender, EventArgs args) {
			FileChooserDialog fc = new FileChooserDialog("Escolha onde salvar sua " +
														"lista de contatos",
														this,
														FileChooserAction.Save,
														Stock.Cancel, ResponseType.Cancel,
														Stock.Save, ResponseType.Accept);
			fc.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			fc.Filter = new FileFilter();
			fc.Filter.AddPattern("*.vcf");
			Util.SetProperty(fc, "DoOverwriteConfirmation", true);
			
			try {
				if (fc.Run() == (int) ResponseType.Accept) {
					string filename = System.IO.Path.ChangeExtension(fc.Filename, "vcf");
					Logger.Log(this, "Saving vCard to {0}", filename);
					using (FileStream file = File.Create(filename)) {
						using (StreamWriter writer = new StreamWriter(file)) {
							writer.Write(VCard.PhoneBookToVCard());
						}
					}
				}
			} finally {
				fc.Destroy();
			}
		}
		
		
		
			
		private void RemoveClicked(object sender, EventArgs args) {
			IList<string> destinations = Destinations;
			int destinationsCount = destinations.Count;
			if (destinationsCount < 1)
				return;
			System.Text.StringBuilder names = new System.Text.StringBuilder();
			names.Append(destinations[0]);
			string plural = String.Empty;
			if (destinationsCount > 1) {
				plural = "s";
				for (int i = 1; i < destinationsCount; i++) {
					if (i == (destinationsCount-1))
						names.Append(" e ");
					else
						names.Append(", ");
					names.Append(Util.Replace(destinations[i]));
				}
			}
			MessageDialog md = Util.CreateMessageDialog(PhoneBookWindow.This,
				DialogFlags.DestroyWithParent, MessageType.Question,
				ButtonsType.YesNo, true, "Remover contatos permanentemente?", String.Format( 
				"Você está prestes a remover de sua agenda de telefones o{1} contato{1} " +
				"<b>{0}</b>. Após a remoção não será possível recuperar este{1} número{1}.\n" +
				"Você realmente deseja removê-lo{1}?", names, plural));
			int response = md.Run();
			md.Destroy();
			if (response == (int) ResponseType.Yes) {
				MainWindow.This.Destinations = null;	
				PhoneBook.Hold();
				try {
					foreach (string dest in (IEnumerable<string>)destinations)
						PhoneBook.Remove(dest);
				} finally {
					PhoneBook.Thew();
				}
			}
		}
	}
}