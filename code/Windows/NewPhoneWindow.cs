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
using Gtk;

using MensagemWeb;
using MensagemWeb.Engines;
using MensagemWeb.Phones;

namespace MensagemWeb.Windows {
	public class NewPhoneWindow : Window {
		private static NewPhoneWindow this_;
		public static NewPhoneWindow This {
			get {
				if (this_ == null)
					this_ = new NewPhoneWindow();
				return this_;
			}
		}
		
		protected Tooltips tooltips = new Tooltips();
		protected void SetTip(Widget wd, string text) { tooltips.SetTip(wd, text, text); }
		
		protected const string automaticEngine = "Automático <small>(recomendado)</small>";
		protected Entry name = new Entry();
		protected Entry ddd = new Entry();
		protected Entry number = new Entry();
		protected ComboBox engine = ComboBox.NewText();
		protected Button saveButton;
		
		
		
		protected NewPhoneWindow()
				: base("Novo telefone")
		{
			this.TransientFor = PhoneBookWindow.This;
			this.WindowPosition = WindowPosition.CenterOnParent;
			this.DestroyWithParent = true;
			this.TypeHint = Gdk.WindowTypeHint.Dialog;
			this.Modal = true;
			this.Resizable = false;
			this.DeleteEvent += delegate (object sender, DeleteEventArgs a) {
				this.Hide();
				a.RetVal = true;
			};
			
			// Main table
			Table mainTable = new Table(4, 3, false);
			mainTable.BorderWidth = 12;
			mainTable.RowSpacing = 6;
			mainTable.ColumnSpacing = 6;
			this.Add(mainTable);
			
			// First line: Name
			Label nameLabel = new Label("Nome:");
			nameLabel.Xalign = 1;
			mainTable.Attach(nameLabel, 0, 1, 0, 1,
				AttachOptions.Shrink | AttachOptions.Fill, 
				AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
			
			mainTable.Attach(new Image("emblem-people", IconSize.Menu), 1, 2, 0, 1,
				AttachOptions.Shrink | AttachOptions.Fill, 
				AttachOptions.Shrink | AttachOptions.Fill, 0, 0);		
			
			SetTip(name, "Preencha com o nome do dono deste telefone");
			name.Changed += Check;
			name.Activated += Save;
			mainTable.Attach(name, 2, 3, 0, 1,
				AttachOptions.Expand | AttachOptions.Fill, 
				AttachOptions.Expand | AttachOptions.Fill, 0, 0);
			
			// Second line: Phone number
			Label phoneLabel = new Label("Telefone:");
			phoneLabel.Xalign = 1;
			mainTable.Attach(phoneLabel, 0, 1, 1, 2,
				AttachOptions.Shrink | AttachOptions.Fill, 
				AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
			
			SetTip(ddd, "Preencha com o DDD do telefone");
			ddd.MaxLength = 2;
			ddd.WidthChars = 2;
			ddd.Changed += Check;
			ddd.Activated += Save;
			mainTable.Attach(ddd, 1, 2, 1, 2,
				AttachOptions.Shrink | AttachOptions.Fill, 
				AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
				
			SetTip(number, "Preencha com o número do telefone");
			number.MaxLength = 8;
			number.WidthChars = 8;
			number.Changed += Check;
			number.Activated += Save;
			mainTable.Attach(number, 2, 3, 1, 2,
				AttachOptions.Expand | AttachOptions.Fill, 
				AttachOptions.Expand | AttachOptions.Fill, 0, 0);
			
			// Third line: engine
			Label engineLabel = new Label("Operadora:");
			engineLabel.Xalign = 1;
			mainTable.Attach(engineLabel, 0, 1, 2, 3,
				AttachOptions.Shrink | AttachOptions.Fill, 
				AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
			engine.Clear();
			CellRendererText renderer = new CellRendererText();
			engine.PackStart(renderer, true);
			engine.AddAttribute(renderer, "markup", 0);
			mainTable.Attach(engine, 1, 3, 2, 3,
				AttachOptions.Shrink | AttachOptions.Fill, 
				AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
			
			// Populates the engine combobox	
			List<string> engineList = new List<string>();
			foreach (IEngine e in EngineCatalog.Engines)
				engineList.Add(e.Name);
			engineList.Sort();
			
			engine.AppendText(automaticEngine);
			foreach (string s in engineList)
				engine.AppendText(s);
			
			// Fourth line: buttons
			ButtonBox buttonBox = new HButtonBox();
			buttonBox.Spacing = 6;
			buttonBox.Layout = ButtonBoxStyle.End;
			mainTable.Attach(buttonBox, 1, 3, 3, 4,
				AttachOptions.Shrink | AttachOptions.Fill, 
				AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
			
			Button cancelButton = new Button(Stock.Cancel);
			SetTip(cancelButton, "Fecha esta janela sem fazer nenhuma" +
				" alteração à sua agenda de telefones");
			cancelButton.Clicked += delegate {
				this.Hide();
			};
			buttonBox.Add(cancelButton);
			
			saveButton = new Button(Stock.Save);
			SetTip(saveButton, "Salva as alterações feitas à sua agenda" +
				" de telefones e fecha esta janela");
			saveButton.Clicked += Save;
			buttonBox.Add(saveButton);
		}
		
		
		protected IEngine SelectedEngine {
			get {
				TreeIter iter;
				if (engine.GetActiveIter(out iter)) {
					string str = (string)engine.Model.GetValue(iter, 0);
					if (str != automaticEngine)
						return EngineCatalog.ByName(str);
				}
				return null;
			}
		}
		
		
		protected bool CheckEngine(IEngine selected) {
			if (selected != null) {
				string message = "Ao forçar o uso da operadora {0} com este destinatário " +
					"você poderá fazer com que não seja possível enviar mensagens ao " +
					"mesmo, sendo que os avisos de erro que eventualmente apareçam " +
					"durante o envio podem não fazer sentido e tornar o problema ainda mais " +
					"difícil de ser resolvido. \n\nVocê realmente deseja que o uso da " + 
					"operadora {0} seja forçado?";
				string title = "Forçar o uso da operadora {0}?";
				string name = selected.Name;
				MessageDialog md = Util.CreateMessageDialog(this, DialogFlags.DestroyWithParent,
										MessageType.Question, ButtonsType.YesNo, false, 
										String.Format(title, name), String.Format(message, name));
				int response = md.Run();
				md.Destroy();
				if (response != (int) ResponseType.Yes)
					return false;
			}
			
			return true;
		}
		
		protected virtual void Save(object sender, EventArgs a) {
			if (saveButton.Sensitive) {
				IEngine selected = SelectedEngine;
				if (!CheckEngine(selected))
					return;
				Phone p = new Phone(ddd.Text, number.Text);
				PhoneBook.Add(name.Text, p, selected);
				this.Hide();
			}
		}
		
		public void ClearAndShow() {
			TreeIter iter;
			if (engine.Model.GetIterFirst(out iter))
				engine.SetActiveIter(iter);
			
			int? defaultDDD = PhoneBook.DefaultDDD;
			if (defaultDDD.HasValue)
				ddd.Text = defaultDDD.ToString();
			else
				ddd.Text = String.Empty;
			
			number.Text = String.Empty;
			name.Text = String.Empty;
			name.GrabFocus();
			
			this.Check(null, null);
			this.ShowAll();
			this.Present();
		}
		
		protected virtual void Check(object sender, EventArgs args) {
			Check(null);
		}
		
		protected void Check(string allowedName) {
			bool enabled = true;
			try {
				string name_Text = name.Text;
				if ((allowedName == null || name_Text != allowedName) &&
					(name_Text.Length < 1 || PhoneBook.Contains(name_Text)))
				{
					Util.SetColorRed(name);
					enabled = false;
				} else
					Util.SetColorNormal(name);
				
				bool normal = false;
				try {
					if (Phone.ValidDDD(Convert.ToUInt16(ddd.Text)))
						normal = true;
				} catch { }
				if (normal)
					Util.SetColorNormal(ddd);
				else {
					Util.SetColorRed(ddd);
					enabled = false;
				}
				
				normal = false;
				try {
					if (Convert.ToUInt32(number.Text) > 10000000)
						normal = true;
				} catch { }
				if (normal)
					Util.SetColorNormal(number);
				else {
					Util.SetColorRed(number);
					enabled = false;
				}
			} catch {
				enabled = false; 
			}
			saveButton.Sensitive = enabled;
		}
	}
}
