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

using MensagemWeb.Logging;
using MensagemWeb.Phones;

namespace MensagemWeb.Windows {
	public sealed partial class DestinationWidget : ScrolledWindow {
		private bool sensitive = true; // Used to cache the status
		private bool suppressSelectFirst = false;
		private List<SingleDestinationWidget> DestWidgets = new List<SingleDestinationWidget>();
		
		private VBox destBox = new VBox();
		private Tooltips tooltips = new Tooltips();
		
		// Events
		public event EventHandler Changed;
		
		
		
		public DestinationWidget()
				: base()
		{
			this.ShadowType = ShadowType.None;
			this.SetPolicy(PolicyType.Never, PolicyType.Automatic);
		
			FirstDestinationWidget widget = new FirstDestinationWidget(this);
			DestWidgets.Add(widget);			
			destBox.PackStart(widget, false, false, 0);
			widget.ComboBox.Destination = null;
			
			Viewport port = new Viewport();
			destBox.BorderWidth = 5;
			destBox.Spacing = 3;
			port.Add(destBox);
			this.Add(port);
			this.ShowAll();
		}
		
		public Widget MnemonicWidget { get { return DestWidgets[0].ComboBox; } }
		
		
		
		public IList<Destination> Selected {
			get {
				List<Destination> result = new List<Destination>(DestWidgets.Count);
				foreach (SingleDestinationWidget sdw in DestWidgets) {
					Destination d = sdw.ComboBox.Destination; 
					if (d != null) result.Add(d);
				}
				return result;
			}
			set {
				// Sanity check
				if (value == null || value.Count <= 0) {
					NumberOfDestinations = 1;
					DestWidgets[0].ComboBox.Destination = null;
					return;
				}
				
				suppressSelectFirst = true;
				try {
					NumberOfDestinations = value.Count;
				} finally {
					suppressSelectFirst = false;
				}
				
				using (IEnumerator<SingleDestinationWidget> en = DestWidgets.GetEnumerator())
					foreach (Destination d in (IEnumerable<Destination>)value) {
						en.MoveNext();
						en.Current.ComboBox.Destination = d;
					}
			}
		}
		
		
		
		public int NumberOfDestinations {
			get {
				return DestWidgets.Count;
			}
			set {
				if (value < 1) {
					if (PhoneBook.Count == 0)
						return;
					else
						throw new ArgumentException("There should be at least one destination.");
				}
				int difference = value - DestWidgets.Count;
				if (difference == 0) {
					return;
				} else if (difference < 0) {
					// We have to remove some widgets
					for (int i = difference; i < 0; i++)
						RemoveDestination(1);
				} else if (difference > 0) {
					// We have to add some widgets
					AddDestinations(difference);
				}
				FireChanged();
			}
		}
		
		
		
		
		public new bool Sensitive {
			get {
				return sensitive;
			}
			set {
				if (sensitive == value)
					return;
				foreach (SingleDestinationWidget widget in DestWidgets)
					widget.Sensitive = value;
				sensitive = value;
			}
		}
		
		
		
		private void FireChanged() {
			if (Changed != null)
				Changed(null, EventArgs.Empty);
		}
		
		
		
		private void AddDestinations(int n) {
			SingleDestinationWidget widget;
			for (int i = 0; i < n; i++) {
				widget = new SingleDestinationWidget(this);
				widget.AddAction(RemoveDestination);
				if (!suppressSelectFirst)
					widget.ComboBox.Destination = null;
				DestWidgets.Add(widget);
				destBox.PackStart(widget, false, false, 0);
				widget.ShowAll();
			}
		}
		
		
		private void RemoveDestination(int index) {
			if (index <= 0 || index >= DestWidgets.Count)
				throw new ArgumentOutOfRangeException("index");
			
			SingleDestinationWidget widget = DestWidgets[index];
			DestWidgets.RemoveAt(index);
			destBox.Remove(widget);
		}
		
		
		private void RemoveDestination(object sender, EventArgs args) {
			Widget w = (sender as Widget).Parent;
			while (w as SingleDestinationWidget == null)
				w = w.Parent;
			RemoveDestination(DestWidgets.IndexOf(w as SingleDestinationWidget));
			FireChanged();
		}	
	
	
	
	
	

		// Widget responsable of a single destination.
		private class SingleDestinationWidget : HBox {
			protected DestinationWidget wd;
			protected DestinationWidget.DestinationComboBox comboBox;
			protected Button button;
			
			
			// Creates a new SingleDestinationWidget with a remove ("-") button.
			public SingleDestinationWidget(DestinationWidget wd)
					: this(wd, new Image(Stock.Remove, IconSize.Menu),
					       "Remove este destinatário da lista dos receberão esta mensagem.") 
			{  }
			
			
			protected SingleDestinationWidget(DestinationWidget wd, Image buttonImage, string buttonTip)
					: base()
			{
				this.Spacing = 3;
				this.wd = wd;		                                 	
				
				comboBox = new DestinationComboBox(wd);
				this.PackStart(comboBox, true, true, 0);
				
				button = new Button(buttonImage);
				this.PackStart(button, false, true, 0);
				wd.tooltips.SetTip(button, buttonTip, buttonTip);
			}
			
			public void AddAction(EventHandler handler) {
				button.Clicked += handler;
			}
			
			public DestinationWidget.DestinationComboBox ComboBox {
				get { return comboBox; }
			}
		}
		
		
		
		
		// The first destination (with the + button)
		private sealed class FirstDestinationWidget : SingleDestinationWidget {
			private static Image Image_ = new Image(Stock.Add, IconSize.Menu);
			private static string Tip = "Adiciona mais um destinatário à lista dos que " +
										"receberão esta mensagem.";
			
			public FirstDestinationWidget(DestinationWidget wd)
					: base(wd, Image_, Tip)
			{
				// The button action
				AddAction(delegate { wd.NumberOfDestinations++; });
			
				// Add hooks to check if the limit of destinations is okay
				PhoneBook.Updated += UpdateLimit;
				wd.Changed += UpdateLimit;
				this.UpdateLimit(null, null);
			}
			
			public void UpdateLimit(object sender, EventArgs args) {
				int n_dests = wd.NumberOfDestinations;
				int phones = PhoneBook.Count;
				
				if (n_dests > phones) {
					wd.NumberOfDestinations = phones;
					n_dests = wd.NumberOfDestinations;
				}				
					
				button.Sensitive = (n_dests < phones);
			}
		}
	} // partial DestinationWidget
} // namespace