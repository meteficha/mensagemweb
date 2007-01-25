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

using MensagemWeb.Engines;
using MensagemWeb.Phones;

namespace MensagemWeb.Windows {
	public class EditPhoneWindow : NewPhoneWindow {
		private static EditPhoneWindow this_;
		public new static EditPhoneWindow This {
			get {
				if (this_ == null)
					this_ = new EditPhoneWindow();
				return this_;
			}
		}
		
		private string prevName;
		private Phone prevPhone;
		private IEngine prevEngine;
		
		protected EditPhoneWindow()
				: base()
		{
			this.Title = "Editar telefone";
		}
		
		
		
		protected override void Save(object sender, EventArgs a) {
			if (saveButton.Sensitive) {
				IEngine newEngine = SelectedEngine;
				if (!CheckEngine(newEngine))
					return;
				
				string newName = name.Text;
				Phone newPhone = new Phone(ddd.Text, number.Text);
				
				PhoneBook.Hold();
				PhoneBook.Remove(prevName);
				PhoneBook.Add(newName, newPhone, newEngine);
				PhoneBook.Thew();
				
				this.Hide();
			}
		}
		
		
		public void Edit(string name) {
			// Get the data we need
			prevName = name;
			PhoneContainer cont = PhoneBook.Get(name);
			prevPhone = cont.Phone;
			prevEngine = cont.Engine;
			
			// Find the correct engine
			TreeIter iter;
			TreeModel model = engine.Model;
			if (model.GetIterFirst(out iter)) {
				if (prevEngine == null) {
					engine.SetActiveIter(iter);
				} else {
					while (model.IterNext(ref iter))
						if ((string)model.GetValue(iter, 0) == prevEngine.Name) {
							engine.SetActiveIter(iter);
							break;
						}
				}
			}
			
			// Fill the entries
			this.ddd.Text = prevPhone.DDD.ToString();
			this.number.Text = prevPhone.Number.ToString();
			this.name.Text = prevName;
			this.name.GrabFocus();
			
			// Show the window
			this.Check(null, null);
			this.ShowAll();
			this.Present();
		}
		
		
		
		[Obsolete("Call Edit method instead of ClearAndShow.")]
		public new void ClearAndShow() {
			throw new NotImplementedException("Call Edit method instead.");
		}
		
		
		
		protected override void Check(object sender, EventArgs args) {
			Check(prevName);
		}
	}
}