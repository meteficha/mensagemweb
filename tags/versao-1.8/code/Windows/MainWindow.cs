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
using System.Text;
using System.Reflection;
using Gtk;

using MensagemWeb.Config;
using MensagemWeb.Engines;
using MensagemWeb.Logging;
using MensagemWeb.Messages;
using MensagemWeb.Phones;

namespace MensagemWeb.Windows {
	public class MainWindow : Window {
		private static MainWindow this_;
		public static MainWindow This {
			get { 
				if (this_ == null)
					this_ = new MainWindow();
				return this_;
			}
		}
				
		// Private variables
		private bool destsOk = false;
		private enum ActivatedWidgets { All, Dest, None }
		private ActivatedWidgets activatedWidgets_ = ActivatedWidgets.All;
		
		// Widgets
		private Tooltips tooltips = new Tooltips();
		private Entry fromDDD = new Entry(); 
		private Entry fromNumber = new Entry();
		private Entry fromName = new Entry();
		private DestinationWidget destination = new DestinationWidget();
		private TextView contents = new TextView();
		private Label msgNoLabel = new Label(String.Empty);
		private Button sendButton;
		private Button clearButton;
		private ImageMenuItem phoneItem;
		
		// The menu
		private Menu menu;
		private Button menuButton;
		
		
		
		private bool sent = true;
		private bool Sent {
			get {
				if (sent == false && Contents.Length > 0)
					return false;
				else
					return true;
			}
		}
		
		
		// null if the user didn't type it
		public Phone FromPhone {
			get {
				string ddd = fromDDD.Text;
				string num = fromNumber.Text;
				if (ddd.Length > 0 || num.Length > 0) {
					// Something was typed
					if (ddd.Length == 2 && num.Length == 8) {
						// Everything is okay
						try {
							Phone p = new Phone(ddd, num);
							Util.SetColorNormal(fromDDD);
							Util.SetColorNormal(fromNumber);
							return p;
						} catch { }
					}
					
					// Something is not okay
					bool normal = false;
					try {
						if (Phone.ValidDDD(Convert.ToUInt16(fromDDD.Text)))
							normal = true;
					} catch { }
					if (normal) {
						// If this is okay, then the other is not
						Util.SetColorNormal(fromDDD);
						Util.SetColorRed(fromNumber);
						return null;
					} else
						Util.SetColorRed(fromDDD);
					
					// If we're here, it's because the other was
					// not okay, so let's see if this is not okay, too.
					normal = false;
					try {
						if (Convert.ToUInt32(fromNumber.Text) > 10000000)
							normal = true;
					} catch { }
					if (normal)
						Util.SetColorNormal(fromNumber);
					else
						Util.SetColorRed(fromNumber);
				} else {
					// Nothing was typed, so it's okay
					Util.SetColorNormal(fromDDD);
					Util.SetColorNormal(fromNumber);
				}
				return null;
			}
		}
		
		
		public string FromPhoneDDD {
			get { return fromDDD.Text; }
			set { fromDDD.Text = value; }
		}
		
		public string FromPhoneNumber {
			get { return fromNumber.Text; }
			set { fromNumber.Text = value; }
		}
		
		public IList<string> Destinations {
			get { return destination.Selected; }
			set { 
				destination.Selected = value;
				CheckDestination(null, null); 
				UpdateMsgNo(null, null);
			}
		}
		
		public string FromName {
			get { return fromName.Text; }
			set { fromName.Text = value; }
		}
		
		private string Contents_ = String.Empty;
		public string Contents {
			get { return Contents_; }
			set { contents.Buffer.Text = value; }
		}
		
		private ActivatedWidgets activatedWidgets {
			get { return activatedWidgets_; }
			set {
				bool destSensitive = false, 
				     fromSensitive = false,
				     otherSensitive = false, 
				     buttonsSensitive = false;
				activatedWidgets_ = value;
				switch (value) {
					case ActivatedWidgets.All:
						destSensitive = true;
						fromSensitive = true;
						otherSensitive = true;
						buttonsSensitive = true;
						break;
					case ActivatedWidgets.Dest:
						destSensitive = true;
						fromSensitive = false;
						otherSensitive = false;
						buttonsSensitive = true;
						break;
				}
				destination.Sensitive = destSensitive;
				phoneItem.Sensitive = destSensitive;
				fromDDD.Sensitive = fromSensitive;
				fromNumber.Sensitive = fromSensitive;
				fromName.Sensitive = fromSensitive;
				contents.Sensitive = otherSensitive;
				clearButton.Sensitive = buttonsSensitive;
				CheckSendButton(null, null);
			}
		}
		
		// Creates a frame around the widget. Used in the MainWindow constructor
		// not to duplicate code.
		private Frame AddToFrame(string frameTitle, Widget widget) {
			return AddToFrame(frameTitle, widget, widget); }
		private Frame AddToFrame(string frameTitle, Widget widget, Widget mnemonic) {
			Frame frame = new Frame();
			frame.Shadow = ShadowType.None;
			
			Label label = new Label(frameTitle);
			label.MnemonicWidget = mnemonic;
			frame.LabelWidget = label;
			
			Alignment align = new Alignment(0.5f, 0.5f, 1.0f, 1.0f);
			align.TopPadding = 3;
			align.LeftPadding = 12;
			
			frame.Add(align);
			align.Add(widget);
			
			return frame;
		}
		
		
		private void SetTip(string text, params Widget[] wds) {
			foreach (Widget wd in wds)
				tooltips.SetTip(wd, text, text);
		}
		
		
		private MainWindow()
				: base ("Enviar mensagem") {
			// This is going to be used to set the Image property if this Gtk# version we're
			// running currently supports it (AFAIK Gtk# >= 2.6)
			System.Reflection.PropertyInfo imageInfo = typeof(Button).GetProperty("Image");
		
			// Some properties
			this.TypeHint = Gdk.WindowTypeHint.Normal;
			this.WindowPosition = WindowPosition.Center;
			this.DeleteEvent += TryToClose;
			this.KeyReleaseEvent += delegate (object o, KeyReleaseEventArgs args) {
				if (args.Event.Key == Gdk.Key.Escape)
					TryToClose(o, null);
			};
			
			// Creates the table that will contain the widgets
			Table mainTable = new Table(4, 2, false);
			mainTable.BorderWidth = 12;
			mainTable.RowSpacing = 6;
			mainTable.ColumnSpacing = 0;
			this.Add(mainTable);
			
			// First frame: destination
			destination.Changed += CheckDestination;
			destination.Changed += UpdateMsgNo;
			mainTable.Attach(AddToFrame("_Destinatários", 
						destination, destination.MnemonicWidget),
					0, 1, 0, 1,
					AttachOptions.Shrink | AttachOptions.Fill, 
					AttachOptions.Expand | AttachOptions.Fill, 0, 0);
			
			// Second frame: sender
			Table senderTable = new Table(2, 3, false);
			senderTable.RowSpacing = 6;
			senderTable.ColumnSpacing = 6;
			mainTable.Attach(AddToFrame("Remetente", senderTable),
					0, 1, 1, 2,
					AttachOptions.Shrink | AttachOptions.Fill, 
					AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
			
				// First line: name
				Label fromNameLabel = new Label("_Nome:");
				fromNameLabel.Xalign = 1;
				fromNameLabel.MnemonicWidget = fromName;
				senderTable.Attach(fromNameLabel, 0, 1, 0, 1,
					AttachOptions.Shrink | AttachOptions.Fill, 
					AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
				
				SetTip("Preencha com seu nome", 
					fromName, fromNameLabel);
				fromName.MaxLength = 25;
				fromName.WidthChars = 10;
				fromName.Changed += CheckSendButton;
				fromName.Changed += UpdateMsgNo;
				fromName.KeyReleaseEvent += PutAoContrario;
				senderTable.Attach(fromName, 1, 3, 0, 1,
					AttachOptions.Expand | AttachOptions.Fill, 
					AttachOptions.Fill, 0, 0);
				
				// Second line: phone number
				Label fromNoLabel = new Label("_Telefone:");
				fromNoLabel.Xalign = 1;
				fromNoLabel.MnemonicWidget = fromDDD;
				senderTable.Attach(fromNoLabel, 0, 1, 1, 2,
					AttachOptions.Shrink | AttachOptions.Fill, 
					AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
				
				SetTip("Preencha com o DDD de seu telefone (opcional)",fromDDD);
				fromDDD.MaxLength = 2;
				fromDDD.WidthChars = 2;
				fromDDD.Changed += CheckSendButton;
				fromDDD.Changed += UpdateMsgNo;
				senderTable.Attach(fromDDD, 1, 2, 1, 2,
					AttachOptions.Fill, 
					AttachOptions.Fill, 0, 0);
				
				SetTip("Preencha com o número de seu telefone (opcional)", fromNumber);
				fromNumber.MaxLength = 8;
				fromNumber.WidthChars = 8;
				fromNumber.Changed += CheckSendButton;
				fromNumber.Changed += UpdateMsgNo;
				senderTable.Attach(fromNumber, 2, 3, 1, 2,
					AttachOptions.Expand | AttachOptions.Fill, 
					AttachOptions.Fill, 0, 0);
			
			// Third frame: message
			contents.WrapMode = WrapMode.WordChar;
			contents.AcceptsTab = false;
			contents.Buffer.Changed += delegate (object sender, EventArgs a) { 
				Contents_ = (sender as TextBuffer).Text;
			};
			contents.Buffer.Changed += CheckSendButton;
			contents.Buffer.Changed += UpdateMsgNo;
			contents.KeyReleaseEvent += PutAoContrario;
			contents.KeyReleaseEvent += delegate (object sender, KeyReleaseEventArgs args) {
				Gdk.ModifierType control = (args.Event.State & Gdk.ModifierType.ControlMask);
				if (args.Event.Key == Gdk.Key.Return && control == Gdk.ModifierType.ControlMask)
					SendMessage(sender, args);
			};
			ScrolledWindow contentsPanel = new ScrolledWindow();
			SetTip("Escreva aqui a sua mensagem", contentsPanel, contents);
			contentsPanel.ShadowType = ShadowType.In;
			contentsPanel.Add(contents);
			Gdk.Geometry geom = new Gdk.Geometry();
			geom.MinWidth = 180;
			geom.MinHeight = -1;
			this.SetGeometryHints(contentsPanel, geom, Gdk.WindowHints.MinSize);
			mainTable.Attach(AddToFrame("Men_sagem", contentsPanel, contents),
					1, 2, 0, 2,
					AttachOptions.Expand | AttachOptions.Fill, 
					AttachOptions.Expand | AttachOptions.Fill, 0, 0);
			
			// The message count label
			msgNoLabel.Selectable = true;
			msgNoLabel.Justify = Justification.Center;
			mainTable.Attach(msgNoLabel, 0, 2, 2, 3,
					AttachOptions.Shrink | AttachOptions.Fill, 
					AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
			
			// The last line:
			HBox lastLine = new HBox();
			lastLine.Spacing = 6;
			mainTable.Attach(lastLine, 0, 2, 3, 4,
					AttachOptions.Shrink | AttachOptions.Fill, 
					AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
					
				// The "menu"
				CreateMenu();
				menuButton = new Button("_Menu");
				if (imageInfo != null)
					imageInfo.SetValue(menuButton, new Arrow(ArrowType.Down, ShadowType.None), null);
				SetTip("Mostra um menu com mais opções", menuButton);
				menuButton.FocusOnClick = false;
				EventHandler popupMenu = delegate {
					menu.Popup(null, null, MenuPositionFunc, 1, Global.CurrentEventTime);
				};
				menuButton.Clicked += popupMenu;
				menuButton.Pressed += popupMenu;
				HButtonBox menuBBox = new HButtonBox();
				menuBBox.Add(menuButton);
				lastLine.PackStart(menuBBox, false, true, 0);
				
				// The buttons
				HButtonBox buttonBox = new HButtonBox();
				buttonBox.Spacing = 6;
				buttonBox.Layout = ButtonBoxStyle.End;
				lastLine.PackStart(buttonBox, true, true, 0);
				
					clearButton = new Button(Stock.Clear);
					SetTip("Limpa a área de texto no primeiro clique e limpa o nome e o telefone" +
						" do remetente no segundo clique", clearButton);
			        clearButton.FocusOnClick = false;
					clearButton.Clicked += delegate {
						if (!CheckSent("Você realmente deseja limpar a mensagem?"))
							return;
						if (Contents.Length > 0) {
							contents.Buffer.Text = String.Empty;
							contents.GrabFocus();
						} else {
							fromDDD.Text = String.Empty;
							fromNumber.Text = String.Empty;
							fromName.Text = String.Empty;
							fromName.GrabFocus();
						}
						sent = true;
					};
					buttonBox.Add(clearButton);
					
					sendButton = new Button("_Enviar");
					SetTip("Envia a mensagem ao destinatário", sendButton); 
					sendButton.FocusOnClick = false;
					if (imageInfo != null)
						imageInfo.SetValue(sendButton, new Image(Stock.GoForward, IconSize.Button), null);
					sendButton.Sensitive = false;
					sendButton.Clicked += SendMessage;
					buttonBox.Add(sendButton);
			
				// Adjust menu button size to be the same as the others
				if (imageInfo != null) {
					Widget sendButton_Image = (Widget) imageInfo.GetValue(sendButton, null);
					Requisition req = sendButton_Image.SizeRequest();
					Widget menuButton_Image = (Widget) imageInfo.GetValue(menuButton, null); 
					menuButton_Image.SetSizeRequest(req.Width, req.Height);
				}
			
			// Check if we have configurations to load
			if (MainWindowConfig.Loaded) {
				FromPhoneDDD = MainWindowConfig.FromPhoneDDD;
				FromPhoneNumber = MainWindowConfig.FromPhoneNumber;
				FromName = MainWindowConfig.FromName;
				Contents = MainWindowConfig.Contents;
				Resize(MainWindowConfig.Width, MainWindowConfig.Height);
				if (MainWindowConfig.Destinations.Count > 0) {
					Destinations = MainWindowConfig.Destinations;
				} else if (MainWindowConfig.DestPhoneStr.Length > 0) {
					Destinations = new string[] { MainWindowConfig.DestPhoneStr };
				} else
					Destinations = null;
			}
			
			// Now that we created everything, call the functions that will
			// finish to setup everything.
			CheckDestination(null, null);
			UpdateMsgNo(null, null);
			sent = true;
			
			// Show the window (finally...)
			this.ShowAll();
			Logger.Log(this, "Main window presented.");
			
			// Check for updates if needed
			GLib.Timeout.Add(300, delegate() {
				if (DateTime.Now.Subtract(UpdateManager.LastAutomaticCheck).TotalDays >= 5) {
					UpdateWindow.This.ShowThis(true);
				}
				return false;
			});
		}
		
		
		private void CreateMenu() {
			menu = new Menu();
			
			// PhoneBook menu item
			phoneItem = new ImageMenuItem("_Agenda de telefones...");
			phoneItem.Image = new Image(Util.GetProperty(typeof(Stock), "Edit", "gtk-edit"), IconSize.Menu);
			phoneItem.Activated += delegate {
				PhoneBookWindow.This.ShowThis();
			};
			menu.Append(phoneItem);
			
			// Queue menu item
			ImageMenuItem queueItem = new ImageMenuItem("_Fila de mensagens...");
			queueItem.Image = new Image(Stock.Network, IconSize.Menu);
			queueItem.Activated += delegate {
				QueueWindow.This.Present();
			};
			menu.Append(queueItem);
			
			// Separator
			menu.Append(new SeparatorMenuItem());
			
			
			// Proxy menu item
			ImageMenuItem proxyItem = new ImageMenuItem("_Configurar proxy");
			proxyItem.Image = new Image(Stock.Properties, IconSize.Menu);
			proxyItem.Activated += delegate {
				ProxyWindow.This.ShowThis();
			};
			menu.Append(proxyItem);
			
			// Info menu item
			ImageMenuItem infoItem = new ImageMenuItem("_Informações de sistema");
			infoItem.Image = new Image(Stock.DialogInfo, IconSize.Menu);
			infoItem.Activated += delegate {
				InfoWindow.This.ShowThis();
			};
			menu.Append(infoItem);
			
			// Update menu item
			ImageMenuItem updateItem = new ImageMenuItem("_Procurar por atualizações");
			updateItem.Image = new Image(Stock.Execute, IconSize.Menu);
			updateItem.Activated += delegate {
				UpdateWindow.This.ShowThis(false);
			};
			menu.Append(updateItem);
			
			// Separator
			menu.Append(new SeparatorMenuItem());
			
			// Help menu item
			ImageMenuItem helpItem = new ImageMenuItem(Stock.Help, null);
			helpItem.Activated += delegate {
				HelpWindow.This.ShowThis();
			};
			menu.Append(helpItem);
			
			// About menu item
			Type AboutDialog = Util.GtkSharp.GetType("Gtk.AboutDialog", false);
			if (AboutDialog != null) {
				// The menu item itself
				string Stock_About = Util.GetProperty<string>(typeof(Stock), "About", "gtk-about");
				ImageMenuItem aboutItem = new ImageMenuItem(Stock_About, null);
				aboutItem.Activated += ShowAbout;
				menu.Append(aboutItem);
				
				// Hook to the LinkButton		
				Type ADALF = Util.GtkSharp.GetType("Gtk.AboutDialogActivateLinkFunc", false);
				MethodInfo SetUrlHook = AboutDialog.GetMethod("SetUrlHook", new Type[] {ADALF});
				MethodInfo AboutOpenLink = typeof(MainWindow).GetMethod("AboutOpenLink");
				Delegate OpenLink = Delegate.CreateDelegate(ADALF, AboutOpenLink);
				SetUrlHook.Invoke(null /* static */, new object[] {OpenLink});
			}
			
			menu.ShowAll();
		}
		
		public void QueueComplete(bool cancelled) {
			// Called by QueueWindow when all messages were successfully sent (or canceled)
			// XXX: Should we use delegates, events, or...?
			
			// If we say the message was not sent, it's because the user is typing another one,
			// so we won't bother him.
			if (!Sent) return;
			string text = "Todas as mensagens foram enviadas com sucesso!";
			if (cancelled) text += "\n(algumas tiveram o envio cancelado)";
			msgNoLabel.Markup = "<b>" + text + "</b>";
				                  
		}
		
		private bool CheckSent(string question) {
			if (!Sent) {
				MessageDialog m = Util.CreateMessageDialog(this, DialogFlags.DestroyWithParent,
					MessageType.Question, ButtonsType.YesNo, true, question,
					"Esta mensagem ainda <b>não</b> foi enviada ao seu destinatário.");
				m.DefaultResponse = ResponseType.Yes;
				int result = m.Run();
				m.Destroy();
				return (result == (int) ResponseType.Yes);
			} else
				return true;
		}
		
		private void UpdateMsgNo(object sender, EventArgs a) {
			sent = false;
			string text = null;
			if (!destsOk) {
				if (PhoneBook.Count == 0)
					text = String.Empty;
				else {
					List<string> unsupported = new List<string>();
					foreach (string dest in (IEnumerable<string>)Destinations)
						if (dest != null && PhoneBook.Get(dest).RealEngine == null)
							unsupported.Add(dest);
						
					int count = unsupported.Count;	
					if (count == 1)
						text = "O número de celular de " + unsupported[0] + " não é reconhecido.";
					else if (count > 1) {	
						unsupported.Sort();
						int lastIdx = unsupported.Count-1;
						string last = unsupported[lastIdx];
						unsupported.RemoveAt(lastIdx);
						
						StringBuilder builder = new StringBuilder();
						builder.Append("Os números de celular de ");
						builder.Append(unsupported[0]);
						unsupported.RemoveAt(0);
						foreach (string name in unsupported) {
							builder.Append(", ");
							builder.Append(name);
						}
						builder.Append(" e ");
						builder.Append(last);
						builder.Append(" não são reconhecidos.");
						text = Util.Split(builder.ToString(), 60);
					}
					text += "\nPor favor veja na Ajuda mais informações sobre o assunto.";
				}
			} else /* if (destsOk) */ {
				try {
					Phone fromPhone = null;
					if (fromDDD.Text.Length > 0 || fromNumber.Text.Length > 0) {
						fromPhone = FromPhone;
						if (fromPhone == null)
							goto End;
					}			
					Message msg = new Message(Destinations, fromPhone, FromName, Contents);
						
					int number = msg.SplitN();
					switch (number) {
						case 0:
							text = "Nenhuma mensagem a ser enviada";
							break;
						case 1:
							text = "Será enviada uma mensagem";
							break;
						default:
							text = String.Format("Serão enviadas {0} mensagens",
							                            Util.Number(number, false));
							break;
					}
					
					number = Destinations.Count;
					if (number >= 2)
						text = String.Format("{0} para {1} pessoas", text,
						                           Util.Number(number, false));
				} catch (Exception excp) {
					Logger.Log(this, "UpdateMsgNo caught:\n{0}", excp);
					if (excp.GetType() == typeof(ArgumentException))
						text = excp.Message; //"Mensagem muito longa para ser enviada";
				}
			}
			
			End:
			if (text == null)
				text = "Por favor preencha os campos destacados";
			msgNoLabel.Text = text;
		}
		
		private void SendMessage(object sender, EventArgs a) {
			if (destsOk) {
				Message msg = new Message(Destinations, FromPhone, FromName, Contents);
				QueueWindow.This.AddMessage(msg);
				sent = true;
			}
		}
		
		private void CheckSendButton(object sender, EventArgs a) {		
			bool enabled = false;
			if (activatedWidgets != ActivatedWidgets.None && 
					activatedWidgets != ActivatedWidgets.Dest) {
				try {
					bool Try = true;
					// Check the sender's name
					if (FromName.Length < 1) {
						Util.SetColorRed(fromName);
						Try = false;
					} else
						Util.SetColorNormal(fromName);
					
					// Check the contents
					if (Contents.Length < 1) {
						Util.SetColorRed(contents);
						Try = false;
					} else
						Util.SetColorNormal(contents);
					
					// Checks if an invalid phone number was given
					if ((fromDDD.Text.Length + fromNumber.Text.Length) > 0 && FromPhone == null)
						Try = false;
					
					// If everything went okay, try to create the message object.
					if (Try) {
						Message msg = new Message(Destinations, FromPhone, FromName, Contents);
						if (msg != null)
							enabled = true;
					}
				} catch { }
			}
			
			// If *everything* (now really) went ok, this is going to be true
			sendButton.Sensitive = enabled;
		}
		
		
		private void CheckDestination(object sender, EventArgs a) {
			// Default result, where the destination is not correct
			ActivatedWidgets result = ActivatedWidgets.Dest;
			
			// Check if all destinations are ok to send messages to
			ICollection<string> dests = Destinations;
			if (dests.Count < 1)
				destsOk = false;
			else {
				destsOk = true;
				foreach (string dest in (IEnumerable<string>)dests)
					if (dest == null || PhoneBook.Get(dest).RealEngine == null) {
						destsOk = false;
						break;
					}
				if (destsOk) {
					result = ActivatedWidgets.All;
					contents.GrabFocus();
				}
			}
			
			// Now do the dirty work over there =P
			activatedWidgets = result;
		}
		
		
		private void TryToClose(object sender, DeleteEventArgs a) {
			if (activatedWidgets != ActivatedWidgets.None && 
					CheckSent("Você realmente deseja sair do MensagemWeb?") &&
					QueueWindow.This.ExitOk()) {
				MainWindowConfig.savedDests = Destinations;
				this.Hide();
				MainClass.Exit();
			} else if (a != null)
				a.RetVal = true;
		}
		
		
		
		public static void AboutOpenLink(object about, string link) {
			Util.OpenLink(link);
		}
		
		private void ShowAbout(object sender, EventArgs args) {
			Type AboutDialog = Util.GtkSharp.GetType("Gtk.AboutDialog", false);
			Dialog d = (Dialog) AboutDialog.GetConstructor(System.Type.EmptyTypes).Invoke(new object[] {});
			
			string version = UpdateManager.CurrentVersion.ToString();
			while (version.EndsWith(".0") && version.Length > 3)
				version = version.Substring(0, version.Length - 2);
			Util.SetProperty(d, "Version", version);
			
			Util.SetProperty(d, "License",
				"MensagemWeb é um software livre; você pode redistribuí-lo e/ou modificá-lo\n" +
				"sob os termos da Licença Pública Geral GNU (GPL) como publicada pela\n" +
				"Fundação do Software Livre; seja a versão 2 da Licença ou (se preferir)\n" +
				"qualquer versão mais recente.\n" +
				"\n" +
				"O MensagemWeb é distribuído na esperança de que seja útil,\n" +
				"mas SEM NENHUMA GARANTIA; até mesmo sem a garantia implicada\n" +
				"de COMERCIALIZAÇÃO ou de ADAPTAÇÃO A UM PROPÓSITO EM PARTICULAR.\n" +
				"Veja a Licença Pública Geral GNU (GPL) para mais detalhes.\n" +
				"\n" +
				"Você deve ter recebido uma cópia da Licença Pública Geral GNU (GPL)\n" +
				"junto com o MensagemWeb; se não, escreva para a Free Software Foundation, Inc.,\n" +
				"51 Franklin St, Fifth Floor. Boston, MA  02110-1301  USA");
			
			Util.SetProperty(d, "Name", "MensagemWeb");
			Util.SetProperty(d, "Logo", new Gdk.Pixbuf(null, "icone64.png"));
			Util.SetProperty(d, "Website", "http://mensagemweb.codigolivre.org.br/");
			Util.SetProperty(d, "WebsiteLabe", "Site do MensagemWeb");
			Util.SetProperty(d, "Comments",
						"Envia mensagens de texto a celulares de diversas operadoras " +
						"gratuitamente pela Internet.");
			Util.SetProperty(d, "Authors", 
				new string[] {"Felipe A. Lessa <felipe.lessa@gmail.com>"});
			Util.SetProperty(d, "Copyright", "Copyright © 2005-2007 Felipe A. Lessa");
			
			d.Run();
			d.Destroy();
		}
		
		private void MenuPositionFunc(Menu menu, out int x, out int y, out bool push_in) {
			int rx, ry;
			
			push_in = true;
			Requisition rq = menuButton.SizeRequest();
			menuButton.TranslateCoordinates(this, 0, rq.Height, out x, out y);
			
			this.Gravity = Gdk.Gravity.Static;
			this.GetPosition(out rx, out ry);
			this.Gravity = Gdk.Gravity.NorthWest;
			
			x+=rx;
			y+=ry;
		}
		
		
		// Just for fun. Everytime someone, on Linux, press the left "windows" key,
		// it inverts the text of that widget. See the AoContrario namespace.
		// So far nobody besides me discovered this ;-). If you're reading this,
		// tell me why and when you were here.
		private void PutAoContrario(object sender, KeyReleaseEventArgs args) {
			if (args.Event.Key != Gdk.Key.Super_L)
				return;
			
			try {				
				string original;
				if (sender is Entry)
					original = (sender as Entry).Text;
				else if (sender is TextView)
					original = (sender as TextView).Buffer.Text;
				else
					return;
					
				string modified = AoContrario.Conversor.Converter(original);
				if (sender is Entry)
					(sender as Entry).Text = modified;
				else
					(sender as TextView).Buffer.Text = modified;
				
				args.RetVal = false;
			} catch { }
		}
	}
}