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
using System.Reflection;
using Gtk;

using MensagemWeb.Logging;
using MensagemWeb.Phones;

namespace MensagemWeb.Windows {
	public sealed partial class DestinationWidget {
		// A ComboBox that uses DestinationModel and has some convenience functions.
		private sealed class DestinationComboBox : ComboBox, IDisposable {
			private static DestinationModel model;
			private DestinationWidget wd;
			private string LastState = null;
			private string CurrentState = null;
			private bool forcingChanges = false;
			
			public DestinationComboBox(DestinationWidget wd)
					: base()
			{
				// Basic setup
				this.wd = wd;
				if (model == null)
					model = new DestinationModel();
				model.SetUpComboBox(this);
				
				// State saver
				model.BeforePopulate += SaveState;
				model.AfterPopulate += LoadState;
			}
			
			private bool disposed = false;
			~DestinationComboBox() {
				(this as IDisposable).Dispose();
			}
			
			void IDisposable.Dispose() {
				if (!disposed) {
					base.Dispose();
					disposed = true;
					model.BeforePopulate -= SaveState;
					model.AfterPopulate -= LoadState;
					LastState = CurrentState = null;
					wd = null;
					GC.SuppressFinalize(this);
				}
			}
			
			
			// Returns or sets the Destination of this widget.
			public string Destination {
				get {
					return CurrentState;
				}
				set {
					forcingChanges = true;
					try {
						if (value == null) {
							this.SelectFirst();
							return;
						}
						
						TreeIter? iter = model.GetIter(
							delegate (Tuple<string, string> values) {
								return (values.ValueA == value);
							});
						
						if (iter.HasValue) {
							this.SetActiveIter(iter.Value);
							LastState = CurrentState = value;
						} else {
							this.SelectFirst();
						}
					} finally {
						forcingChanges = false;
					}
				}
			}
			
			
			protected override void OnChanged() {
				if (forcingChanges)
					return;
				TreeIter iter;
				if (this.GetActiveIter(out iter)) {
					if (model.IsSpecialAction(iter)) {
						this.Destination = LastState;
						model.DoSpecialAction(this, iter);
					} else {
						string newDest = model.GetDestination(iter);
						ICollection<string> selected = wd.Selected;
						if (selected.Count <= 1 || !selected.Contains(newDest)) {
							SaveState(null, null);
							wd.FireChanged();
						} else {
							// There's another widget with the same person as us,
							// fallback to our last (valid) state
							this.Destination = LastState;
						}
					}
				}
			}
			
			
			
			private void SelectFirst() {
				// Get the Destinations that we cannot select
				ICollection<string> dontSelect = wd.Selected;
				List<string> names = new List<string>(dontSelect.Count);
				names.AddRange(dontSelect);
				names.Sort(); // BinarySearch
							
				// Get the first name in alphabetical order that is valid
				string name = null;
				TreeIter iter;
				StringComparer comparer = StringComparer.CurrentCultureIgnoreCase;
				foreach (KeyValuePair<TreeIter, Tuple<string, string>> kvp in 
						(IEnumerable<KeyValuePair<TreeIter, Tuple<string, string>>>) model)
				{
					Tuple<string, string> values = kvp.Value;
					if (!model.IsSpecialAction(values) 
						&& names.BinarySearch(values.ValueA) < 0  
						&& (name == null || comparer.Compare(name, values.ValueA) > 0))
					{
						name = values.ValueA;
						iter = kvp.Key;
					}
				}
				
				// Fallback
				if (name == null && model.GetIterFirst(out iter)) {
					name = String.Empty; // It doesn't matter the value, just that it's != null
				}
				
				// Set as current
				if (name != null) {			
					this.SetActiveIter(iter);
					CurrentState = model.GetDestination(iter);
					wd.FireChanged();
					LastState = CurrentState; // Is order important?
				}
			}
			
			
			
			private void SaveState(object sender, EventArgs args) {
				// Save our state, if possible
				TreeIter iter;
				if (!this.GetActiveIter(out iter))
					CurrentState = null;
				else
					CurrentState = model.GetDestination(iter);
				LastState = CurrentState;
			}
			
			private void LoadState(object sender, EventArgs args) {
				this.Destination = LastState;
			}
			
			
			
			
			
			private sealed class DestinationModel 
				: ListStore, IEnumerable<KeyValuePair<TreeIter, Tuple<string, string>>> 
			{
				// Cache the result of GetValue.
				private Dictionary<TreeIter, Tuple<string, string>> iterCache;
				
				public event EventHandler BeforePopulate;
				public event EventHandler AfterPopulate;
				
				public const string AddSomeone = "Adicione alguém à sua agenda!";
				public const string OpenPhoneBookWindow = "Agenda de telefones...";
				public const string OpenNewPhoneWindow = "Novo telefone...";
				
				private int count = 0;
				
				internal DestinationModel()
						: base(typeof(string), typeof(string))
				{
					// Create the icons
					IconFactory factory = new IconFactory();
					factory.AddDefault();
					IconSet iconset = new IconSet(new Gdk.Pixbuf(null, "emblem-people.png"));
					factory.Add("emblem-people", iconset);
					// This emblem-people.png icon was stolen from the GNOME theme.
					
					iterCache = new Dictionary<TreeIter, Tuple<string, string>>(50);
					Populate(null, null);
					PhoneBook.Updated += Populate;
				}
				
				
				IEnumerator<KeyValuePair<TreeIter, Tuple<string, string>>> 
				IEnumerable<KeyValuePair<TreeIter, Tuple<string, string>>>.GetEnumerator()
				{
					return iterCache.GetEnumerator();
				}
				
				
				
				public void SetUpComboBox(DestinationComboBox combo) {
					// Misc stuff
					combo.Clear();
					CellRendererPixbuf image = new CellRendererPixbuf();
					combo.PackStart(image, false);
					combo.AddAttribute(image, "stock_id", 1);
					CellRendererText text = new CellRendererText();
					combo.PackStart(text, true);
					combo.AddAttribute(text, "text", 0);
					combo.Model = this;
					
					// Make empty rows separators
					Type TVRSF = Util.GtkSharp.GetType("Gtk.TreeViewRowSeparatorFunc", false);
					if (TVRSF != null) {
						Delegate isSeparator = Delegate.CreateDelegate(TVRSF, this, "IsSeparator");
						Util.SetProperty(combo, "RowSeparatorFunc", isSeparator);
					}
				}
				
				
				public bool IsSeparator(TreeModel model, TreeIter iter) {
					try {
						return (iterCache[iter].ValueA.Length == 0);
					} catch (KeyNotFoundException) { 
						// The key has not been added in the dictionary yet, get the value directly
						return String.IsNullOrEmpty(model.GetValue(iter, 0) as string);
					}
				}
				
				
				public TreeIter? GetIter(Predicate<Tuple<string, string>> predicate) {
					foreach (KeyValuePair<TreeIter, Tuple<string, string>> kvp in 
								(IEnumerable<KeyValuePair<TreeIter, Tuple<string, string>>>) iterCache)
					{
						if (predicate(kvp.Value))
							return kvp.Key;
					}
					return null;
				}
				
				
				// Get the destination represented by an iter.
				// If it's not a valid destination, null is returned.
				public string GetDestination(TreeIter iter) {
					try {
						string str = iterCache[iter].ValueA;
						if (str == null || str.Length == 0 || str == OpenPhoneBookWindow
						 		|| str == AddSomeone || str == OpenNewPhoneWindow)
							return null;
						else
							return str;
					} catch (Exception e) {
						Logger.Log(this, e.ToString());
						return null;
					}
				}
				
				
				
				public void DoSpecialAction(DestinationComboBox sender, TreeIter iter) {
					string str = iterCache[iter].ValueA;
					if (str == OpenPhoneBookWindow)
						PhoneBookWindow.This.ShowThis();
					else if (str == OpenNewPhoneWindow)
						NewPhoneWindow.This.ClearAndShow();
				}
				
				
				public bool IsSpecialAction(TreeIter iter) {
					try {
						return IsSpecialAction(iterCache[iter]);
					} catch (Exception e) {
						Logger.Log(this, "Ooops at IsSpecialAction:\n{0}", e);
						return IsSpecialAction(
							new Tuple<string, string>(GetValue(iter, 0) as string,
													  GetValue(iter, 1) as string));
					}
				}
				
				
				public bool IsSpecialAction(Tuple<string, string> val) {
					if (val.ValueB == "emblem-people")
						return false;
					string str = val.ValueA;
					return (str == null || str.Length == 0 || str == OpenPhoneBookWindow ||
							str == OpenNewPhoneWindow || str == AddSomeone);
				}
				
				
				
				// Clears and populates this model.
				private void Populate(object sender, EventArgs args) {
					if (BeforePopulate != null)
						BeforePopulate(this, EventArgs.Empty);
					
					
					// Items we're going to insert
					List<Tuple<string, string>> array = new List<Tuple<string, string>>();
					if (PhoneBook.Count > 0) {
						foreach (string name in PhoneBook.Names)
							if (name != OpenPhoneBookWindow)
								array.Add(new Tuple<string, string>(name, "emblem-people"));
					} else
						array.Add(new Tuple<string, string>(AddSomeone, Stock.DialogInfo));
					array.Add(new Tuple<string, string>(String.Empty, String.Empty));
					array.Add(new Tuple<string, string>(OpenNewPhoneWindow, Stock.New));
					array.Add(new Tuple<string, string>(OpenPhoneBookWindow, "gtk-edit"));
					
					
					// Insert the items on the model
					lock (iterCache) {
						TreeIter iter;
						this.GetIterFirst(out iter);
						
						int weWant = array.Count;
						if (count > weWant)
							for (; count > weWant; count--) {
								iterCache.Remove(iter);
								this.Remove(ref iter);
							}
						else if (count < weWant)
							for (; count < weWant; count++) {
								this.Append();
							}
						this.GetIterFirst(out iter);
						
						foreach (Tuple<string, string> list in array) {
							iterCache[iter] = list;
							this.SetValue(iter, 0, list.ValueA);
							this.SetValue(iter, 1, list.ValueB);
							this.IterNext(ref iter);
						}
					}
						
					if (AfterPopulate != null)
						AfterPopulate(this, EventArgs.Empty);
				}
				
			} // TreeModel
		} // ComboBox
	} // partial DestinationWidget
} // namespace