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
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Gdk;
using Gtk;

using MensagemWeb.Engines;
using MensagemWeb.Logging;
using MensagemWeb.Messages;
using MensagemWeb.Phones;

namespace MensagemWeb.Windows {	
	public sealed class QueueWindow : Gtk.Window {	
		// The status of each QueueItem
		private enum QueueStatus {
			Sending = 1,
			Waiting = 2,
			Error = 3,
			Cancelled = 4,
			Sent = 5
		}
		
		
		// An item in the message queue
		[TreeNode(ListOnly=true)]
		private sealed class QueueItem : TreeNode, IComparable<QueueItem> {
			public readonly Message message;
			public readonly IEngine engine;
			public QueueStatus status = QueueStatus.Waiting;
			
			private DateTime resultTime = DateTime.MinValue;
			private EngineResult result = null;
			private string sendingStatus = "???";
			private double progress = 0.0;
			
			private int number;
			private double time;
			
			public QueueItem(Message message, int number) {
				if (message == null) 
					throw new ArgumentNullException("Message shouldn't be null.");
				if (message.Destinations.Count != 1)
					throw new ArgumentException("Message should have just one " +
						"destination to be put in the queue.");
				
				this.message = message;
				this.engine = PhoneBook.Get(message.Destinations[0]).RealEngine;
				this.time = DateTime.Now.Ticks;
				this.number = number;
				
				MessageContents = Util.Split(message.Contents, 40);
				DestinationName = message.Destinations[0] + "\n<small><i>" +
				                  engine.Name + "</i></small>";
			}
			
			public QueueItem Clone(int newNumber) {
				QueueItem clone = new QueueItem(message, newNumber);
				clone.time = this.time;
				return clone;
			}
			
			public void UpdateProgress(QueueStatus? status, string sendingStatus, double progress) {
				if (status.HasValue)
					this.status = status.Value;
				this.sendingStatus = sendingStatus;
				this.progress = progress;
				FireChanged();
			}
			
			private void FireChanged() {
				try {
					OnChanged();
					QueueWindow.This.ResortNodes();
				} catch {
					// Don't do anything
				}
			}
			
			private int CompareToAux_Number(QueueItem other) {
				int ret = number.CompareTo(other.number);
				if (ret != 0) return ret;
				else return time.CompareTo(other.time);
			}
			
			public int CompareTo(QueueItem other) {
				if (this == other) return 0;
				int basic = this.status.CompareTo(other.status);
				switch (this.status) {
					case QueueStatus.Sending:
						// Order by status, then by progress, then by number
						if (basic == 0) {
							int ret = this.progress.CompareTo(other.progress);
							if (ret != 0) return (-ret);
							else return CompareToAux_Number(other);
						} else
							return basic;
						
					case QueueStatus.Waiting:
					case QueueStatus.Error:
						// Order by status, then by number
						if (basic == 0) 
							return CompareToAux_Number(other);
						else
							return basic;
					
					//case QueueStatus.Cancelled:
					//case QueueStatus.Sent:
					default:
						// Order by number, then by time when got the status
						int ret = CompareToAux_Number(other);
						if (ret != 0)
							return ret;
						else
							return this.resultTime.CompareTo(other.resultTime);
				}
			}
			
			public EngineResult Result {
				get { return result; }
				set {
					if (value == null)
						throw new ArgumentNullException("value");
						
					result = value;
					progress = 1.0;
					switch (value.Type) {
						case EngineResultType.Success:
							status = QueueStatus.Sent;
							break;
						case EngineResultType.UserCancel:
							status = QueueStatus.Cancelled;
							break;
						default: // any other error
							status = QueueStatus.Error;
							break;
					}
					resultTime = DateTime.Now.ToLocalTime();
					FireChanged();
				}
			}
			
			[TreeNodeValue(Column=0)]
			public string StatusStock {
				get {
					switch (status) {
						case QueueStatus.Sending: return Stock.Network;
						case QueueStatus.Sent: return Stock.Apply;
						case QueueStatus.Error: return Stock.DialogError;
						case QueueStatus.Cancelled: return Stock.Cancel;
						default: return String.Empty;
					}
				}
            }
            
            [TreeNodeValue(Column=1)]
			public string StatusString {
				get {
					switch (status) {
						case QueueStatus.Sending:
							return sendingStatus;
						case QueueStatus.Sent:
							return "Enviada!\n<small><i>" + Util.ToPrettyString(resultTime) + "</i></small>";
						case QueueStatus.Waiting:
							return "Esperando na fila";
						case QueueStatus.Cancelled:
							return "Cancelada\n<small><i>" + Util.ToPrettyString(resultTime) + "</i></small>";
						case QueueStatus.Error: 
							string msg = "Erro desconhecido no envio";
							string compl = null;
							if (result != null)
								switch (result.Type) {
									case EngineResultType.ServerMaintenence:
										msg = "Servidor em manutenção";
										compl = "Por favor, tente enviar sua mensagem " +
												"novamente mais tarde.";
										break;
									case EngineResultType.PhoneNotEnabled:
										msg = "Número desconhecido ou desabilitado";
										compl = result.Message;
										break;
									case EngineResultType.LimitsExceeded:
										msg = "Limite de mensagens excedido";
										compl = "A " + engine.Name + " bloqueou temporariamente " +
											"o envio de mensagens a este número porque algum " +
											"limite foi excedido.";
										break;
									case EngineResultType.MaxTryExceeded:
										msg = "Servidor inacessível";
										compl = "Verifique se sua conexão com a Internet está " +
											"funcionando ou se você precisa habilitar o proxy.";
										break;
								
								}
							msg = "<span weight=\"bold\">" + msg + "</span>";
							if (compl != null)
								return msg + "\n" + Util.Split(compl, 40);
							else
								return msg;
						default:
							throw new Exception();
					}
				}
            }
            
            [TreeNodeValue(Column=2)]
            public readonly string DestinationName;
            
            [TreeNodeValue(Column=3)]
            public readonly string MessageContents;
            
            [TreeNodeValue(Column=4)]
            public double Progress { get { return progress; } }
		}
		
		
		
			
		private static QueueWindow this_;	
		public static QueueWindow This {
			get {
				if (this_ == null) this_ = new QueueWindow(); 
				return this_; 
			}
		}
	
	
		// The message queue, grouped by IEngine
		private int number = 1000000;
		private Dictionary<IEngine, Queue<QueueItem>> queue;
		private NodeStore nodes;
		private int msgCount = 0, sentCount = 0; // Used for progressbar
		
		// All items that are not waiting nor being sent
		private List<QueueItem> sent = new List<QueueItem>(10);
		
		// The protected engines
		private Dictionary<IEngine, IEngine> protectedEngines;
		
		// The verification queue, used to prevent showing more than one code at a time
		private Queue<Tuple<QueueItem,IEngine,Pixbuf>> toVerify = 
			new Queue<Tuple<QueueItem,IEngine,Pixbuf>>(5);
		private Tuple<QueueItem,IEngine,Pixbuf>? verifying = null;
		private VerificationWindow verificationWindow = null;
		
		// Strings shown to the user on the labels
		private const string doneTitle = "<span size=\"large\" weight=" +
			"\"bold\">Nenhuma mensagem a ser enviada</span>";
		private const string doneSub = 
			"Atualmente não há nenhuma mensagem a ser enviada. Você pode\n" + 
			"digitar sua mensagem na janela principal do MensagemWeb e\n" +
			"clicar no botão Enviar para adicioná-la aqui."; 
		private const string sendingTitle = "<span size=\"large\" weight=" +
			"\"bold\">Enviando mensagens...</span>";
		private const string sendingSub = 
			"Suas mensagens estão sendo enviadas aos seus destinatários.";
			
		// Menu
		private Menu nodeviewMenu;
			
		// Widgets (Stetic)
#pragma warning disable 649
		private Gtk.ProgressBar progressbar;
		private Gtk.Label titleLabel;
		private Gtk.Label subLabel;
		private Gtk.NodeView nodeview;
		private Gtk.HBox nodepanel;
		private Gtk.HBox errorbox;
		private Gtk.HBox progressbox;
		private Gtk.Button closebutton;
		private Gtk.Action resendAction;
		private Gtk.Action cancelAction;
		private Gtk.Action clearAction;
		private Gtk.Action deleteAction;
		private Gtk.CheckButton autoclosecheckbutton;
#pragma warning restore 649
			
		[GLib.ConnectBefore]
		private void OnConfigureEvent(object o, ConfigureEventArgs args) {
			MensagemWeb.Config.QueueConfig.Width = args.Event.Width;
			MensagemWeb.Config.QueueConfig.Height = args.Event.Height;
		}
		
		private QueueWindow() 
				: base(String.Empty) 
		{
			TransientFor = MainWindow.This;
			Stetic.Gui.Build(this, typeof(MensagemWeb.Windows.QueueWindow));
			this.DeleteEvent += delegate (object sender, DeleteEventArgs a) {
				a.RetVal = true;
				CloseWindow(sender, a);
			};
			this.KeyReleaseEvent += delegate (object o, KeyReleaseEventArgs args) {
				if (args.Event.Key == Gdk.Key.Escape)
					CloseWindow(o, null);
			};
			
			// The size
			this.Resize(MensagemWeb.Config.QueueConfig.Width, MensagemWeb.Config.QueueConfig.Height);
			this.ConfigureEvent += OnConfigureEvent;
			
			
			// Initialize the nodeview
			nodes = new NodeStore(typeof(QueueItem));
			nodeview = new NodeView(nodes);
			nodeview.RulesHint = true;
			//nodeview.HeadersVisible = false;
			nodeview.NodeSelection.Mode = SelectionMode.Multiple;
			nodeview.NodeSelection.Changed += NodeSelectionChanged;
			ScrolledWindow sw = new ScrolledWindow();
			//sw.ShadowType = ShadowType.In;
			sw.Add(nodeview);
			nodepanel.PackStart(sw, true, true, 0);
			nodepanel.ShowAll();
			
			Gdk.Geometry geom = new Gdk.Geometry();
			geom.MinWidth = 150;
			geom.MinHeight = 200;
			this.SetGeometryHints(nodepanel, geom, Gdk.WindowHints.MinSize);
			
			// Nodeview menu
			nodeviewMenu = new Menu();
			nodeviewMenu.Append(resendAction.CreateMenuItem());
			nodeviewMenu.Append(new SeparatorMenuItem());
			nodeviewMenu.Append(cancelAction.CreateMenuItem());
			nodeviewMenu.Append(deleteAction.CreateMenuItem());
			nodeviewMenu.Append(clearAction.CreateMenuItem());
			nodeviewMenu.ShowAll();
			nodeview.ButtonPressEvent += ShowNodeviewMenu;
			
			// Add all the columns to the nodeview
			TreeViewColumn state = new TreeViewColumn();
			state.Title = "Estado";
			CellRenderer cell = new CellRendererPixbuf();
			state.PackStart(cell, false);
			state.AddAttribute(cell, "stock_id", 0);
			cell = new CellRendererText();
			state.PackStart(cell, true);
			state.AddAttribute(cell, "markup", 1);
			nodeview.AppendColumn(state); 
			nodeview.AppendColumn("Destinatário", new CellRendererText(), "markup", 2);
			nodeview.AppendColumn("Mensagem", new CellRendererText(), "text", 3);
			
			
			// Initialize the queue with all known engines
			int engineNos = EngineCatalog.Engines.Length;
			queue = new Dictionary<IEngine, Queue<QueueItem>>(engineNos);
			foreach (IEngine engine in EngineCatalog.Engines)
				queue[engine] = new Queue<QueueItem>(5);
			
			// Initialize the dictionary of protected engines
			protectedEngines = new Dictionary<IEngine, IEngine>(engineNos);
			foreach (IEngine engine in EngineCatalog.Engines) {
				IEngine prot = engine;
				prot = new WaitEngine(prot);
				prot = new RetryEngine(prot);
				protectedEngines[engine] = prot; 
			}
			
			// Autoclose checkbutton
			autoclosecheckbutton.Active = MensagemWeb.Config.QueueConfig.AutoClose;
			autoclosecheckbutton.Toggled += delegate {
				MensagemWeb.Config.QueueConfig.AutoClose = autoclosecheckbutton.Active;
			};
			
			// Updates our status
			UpdateQueue();
			UpdateStatus();
		}
		
		
		// If possible, reset the message counters
		private void ResetCounters() {
			lock (queue) {
				foreach (Queue<QueueItem> q in queue.Values)
					if (q.Count > 0)
						return;
				msgCount = sentCount = 0;
			}
		}
		
		
		public void AddMessage(Message message) {
			lock (queue) {
				Logger.Log(this, "Starting to send message:\n{0}", message);
				Message[] msgs = message.WithoutAccentuation().Split();
				
				if (msgs.Length > 1) {
					Dialog md = new MultipleMsgsDialog(msgs);
					int result = md.Run();
					md.Destroy();
					if (result != (int)ResponseType.Yes) {
						Logger.Log(this, "Cancel on MultipleMsgsDialog.");
						return;
					}
				}
				
				ResetCounters();
				
				number -= 1;
				foreach (string dest in (IEnumerable<string>)message.Destinations)
					foreach (Message msg in msgs) {
						QueueItem item = new QueueItem(msg.ChangeDestination(dest), number);
						queue[item.engine].Enqueue(item);
						msgCount += 1;
					}
				
				CheckQueue(true);
			}
			this.Show();
			this.Present();
		}
		
		
		
		
		public bool ExitOk() {
			// Check the number of messages that will be lost
			int error = 0, pending = 0;
			lock (queue) {
				foreach (Queue<QueueItem> q in queue.Values)
					foreach (QueueItem item in q)
						switch (item.status) {
							case QueueStatus.Error:
								error++;
								break;
							case QueueStatus.Sending:
							case QueueStatus.Waiting:
								pending++;
								break;
						}
				if (error == 0 && pending == 0)
					return true;
			}
			
			
			// Create the message strings
			string errMsg = null, pendMsg = null;
			
			if (error == 1)
				errMsg = "uma mensagem com erro";
			else if (error > 1)
				errMsg = Util.Number(error, false) + " mensagens com erro";
			
			if (pending == 1)
				pendMsg = "uma mensagem por enviar";
			else if (pending > 1)
				pendMsg = Util.Number(pending, false) + " mensagens por enviar";
				
			string message = "Ainda há ";
			if (errMsg != null && pendMsg != null)
				message += pendMsg + " e " + errMsg;
			else
				message += (pendMsg == null ? errMsg : pendMsg);
			message += ". Se você fechar o MensagemWeb, qualquer mensagem na fila deixará de " +
				"ser enviada.";
			
			// Show the dialog
			Gtk.Window parent = this.Visible ? (this as Gtk.Window) : MainWindow.This;
			MessageDialog m = Util.CreateMessageDialog(parent, DialogFlags.DestroyWithParent,
				MessageType.Question, ButtonsType.YesNo, true, 
				"Você deseja deixar de enviar as mensagens na fila?", message);
			m.DefaultResponse = ResponseType.Yes;
			int result = m.Run();
			m.Destroy();
			return (result == (int) ResponseType.Yes);
		}
		
		
		
		
		private void CheckQueue(bool forceUpdate) {
			bool changed = false;
			lock (queue) {
				foreach (Queue<QueueItem> q in queue.Values) {
					QueueItem item;
					
					// Remove sent and cancelled items from the queue
					// XXX: Maybe this should be done by the methods that mark items as sent?
					q.Enqueue(null);
					for (item = q.Dequeue(); item != null; item = q.Dequeue())
						switch (item.status) {
							case QueueStatus.Waiting:
							case QueueStatus.Sending:
								q.Enqueue(item);
								break;
							default:
								changed = true;
								sent.Add(item);
								sentCount += 1;
								break;
						}
										
					// Send the first item if it was waiting
					if (q.Count > 0) {
						item = q.Peek();
						if (item.status == QueueStatus.Waiting) {
							changed = true;
							Send(item);
						}
					}
				}
			}
			
			if (forceUpdate || changed)
				UpdateQueue();
		}
		
		
		
		// This method is relatively expensive and should be called only when
		// the queue is changed.
		private void UpdateQueue() {
			lock (queue) {
				List<QueueItem> remaining = new List<QueueItem>();
				foreach (Queue<QueueItem> q in queue.Values)
					foreach (QueueItem item in q)
						remaining.Add(item);
				
				clearAction.Sensitive = sent.Count > 0;
				if (remaining.Count == 0) {
					titleLabel.Markup = doneTitle;
					subLabel.Markup = doneSub;
					closebutton.Sensitive = true;
					progressbar.Hide();
					
					bool errors = false, cancelled = false;
					foreach (QueueItem item in sent) {
						// XXX: Move this to the methods who change sent?
						if (item.status == QueueStatus.Error) {
							errors = true;
							break;
						} else if (item.status == QueueStatus.Cancelled) {
							cancelled = true;
						}
					}
					if (errors) {
						errorbox.Show();
						progressbox.Hide();
					} else {
						errorbox.Hide();
						progressbox.Show();
						MainWindow.This.QueueComplete(cancelled);
					}
				} else {
					titleLabel.Markup = sendingTitle;
					subLabel.Markup = sendingSub;
					closebutton.Sensitive = false;
					errorbox.Hide();
					progressbox.Show();
					progressbar.Show();
				}
				
				remaining.AddRange(sent);
				remaining.Sort();
				nodeview.FreezeChildNotify();
				try {
					nodes.Clear();
					foreach (QueueItem item in (IEnumerable<QueueItem>)remaining)
						nodes.AddNode(item);
				} finally {
					nodeview.ThawChildNotify();
					nodeview.ColumnsAutosize();
				}
			}
			
			UpdateStatus();
		}
		
		private void ResortNodes() {
			lock (queue) {
				List<QueueItem> items = new List<QueueItem>(msgCount);
				foreach (ITreeNode node in nodes)
					items.Add(node as QueueItem);
				items.Sort();
				nodeview.FreezeChildNotify();
				try {
					nodes.Clear();
					foreach (QueueItem item in (IEnumerable<QueueItem>)items)
						nodes.AddNode(item);
				} finally {
					nodeview.ThawChildNotify();
					nodeview.ColumnsAutosize();
				}
			}
		}
		
		
		// Updates our status (the progressbar, currently)
		private void UpdateStatus() {
			if (msgCount > 0) {
				double done = sentCount;
				lock (queue)
					foreach (Queue<QueueItem> q in queue.Values)
						foreach (QueueItem item in q)
							done += item.Progress;
				progressbar.Fraction = done / msgCount;
				
				System.Text.StringBuilder text = new System.Text.StringBuilder(50);
				if (sentCount == 0) {
					text.Append("Enviando ");
					text.Append(msgCount);
					text.Append(msgCount == 1 ? " mensagem..." : " mensagens...");
				} else {
					if (sentCount == 1)
						text.Append("Foi enviada 1");
					else {
						text.Append("Foram enviadas ");
						text.Append(sentCount);
					}
					text.Append(" de ");
					text.Append(msgCount);
					text.Append(" mensagens...");
				}
				progressbar.Text = text.ToString(); 
			}
		}
		
		

		// Start sending a message
		private void Send(QueueItem item) {
			item.UpdateProgress(QueueStatus.Sending, "Baixando código...", 0.0);
			IEngine engine = protectedEngines[item.engine];
			Thread worker = new Thread(delegate () {
				engine.Clear();
				EngineResult result = engine.SendMessage(item.message, delegate (Stream stream) {
						SendMessageDelegate(stream, engine, item);
				});
				if (engine.Aborted)
					result = EngineResult.UserCancel;
				if (result.Type != EngineResultType.Success)
					Gtk.Application.Invoke(delegate {
						item.Result = result;
						CheckQueue(true);
					});
			});
			worker.Name = "SendMessage -- " + engine.Name;
			worker.Start();
		}
		
		
		// This method is called from the worker thread
		private void SendMessageDelegate(Stream stream, IEngine engine, QueueItem item) {
			if (engine.Aborted)
				// Send() will take care of this
				return;
			Gdk.Pixbuf pixbuf = new Gdk.Pixbuf(stream);
			Application.Invoke(delegate {
				item.UpdateProgress(null, "Mostrando código...", 0.3333333333333);
				UpdateStatus();
				RequestVerification(item, engine, pixbuf);
			});
		}
		
		
		
		private void RequestVerification(QueueItem item, IEngine engine, Gdk.Pixbuf pixbuf) {
			lock (toVerify) {
				toVerify.Enqueue(new Tuple<QueueItem,IEngine,Pixbuf>(item, engine, pixbuf));
				CheckVerification();
			}
		}
		
		
		// Checks if we can ask another code for the user.
		private void CheckVerification() {
			lock (toVerify) {
				if (verifying.HasValue || toVerify.Count == 0) return;
				Tuple<QueueItem,IEngine,Pixbuf> now;
				verifying = now = toVerify.Dequeue();
				if (now.ValueA.status == QueueStatus.Cancelled) {
					CheckVerification();
					return;
				}
				verificationWindow = new VerificationWindow(now.ValueC, 
					delegate (string code) {
						lock (toVerify) {
							SendCodeDelegate(code, now.ValueB, now.ValueA);
							verifying = null;
							verificationWindow = null;
							CheckVerification();
						}
				});
			}
		}
		
		
		// This method is called from the Gtk#'s thread.
		private void SendCodeDelegate(string code, IEngine engine, QueueItem item) {
			item.UpdateProgress(null, "Enviando código...", 0.666666666);
			UpdateStatus();
			Thread worker = new Thread(delegate () {
				EngineResult result = EngineResult.UserCancel;
				if (code != null && !engine.Aborted) {
					if (code.Length > 0) result = engine.SendCode(code);
					else result = EngineResult.WrongPassword;
				}
				
				Gtk.Application.Invoke(delegate {
					if (result == EngineResult.WrongPassword) {
						// XXX: Show some feedback
						Logger.Log(this, "Wrong password, trying again.");
						Send(item);
						UpdateStatus();
						return;
					}
					item.Result = result;
					CheckQueue(true); // calls UpdateStatus();
					if (!progressbar.Visible && !errorbox.Visible && autoclosecheckbutton.Active)
						CloseWindow(null, null);
				});
			});
			worker.Name = "SendCode -- " + engine.Name;
			worker.Start();
		}
		
		
		
		private void NodeSelectionChanged(object sender, EventArgs args) {
			ITreeNode[] selected = nodeview.NodeSelection.SelectedNodes;
			resendAction.Sensitive = deleteAction.Sensitive = selected.Length > 0;
			
			bool cancelable = false;
			foreach (ITreeNode node in selected)
				switch ((node as QueueItem).status) {
					case QueueStatus.Sending:
					case QueueStatus.Waiting:
						cancelable = true;
						goto Out;
				}
			Out:
			cancelAction.Sensitive = cancelable;
		}
		
		
		
		[GLib.ConnectBefore]
		private void ShowNodeviewMenu(object o, ButtonPressEventArgs args) {
			Gdk.EventButton evento = args.Event;
			if (evento.Type == Gdk.EventType.ButtonPress && evento.Button == 3) {
				nodeviewMenu.Popup(null, null, null, evento.Button, evento.Time);
				args.RetVal = false;
			}
		}
		
		
		private void CloseWindow(object sender, EventArgs e) {
			if (!closebutton.Sensitive) {
				lock (toVerify) {
					int rem = msgCount - sentCount;
					MessageDialog m = Util.CreateMessageDialog(this, DialogFlags.DestroyWithParent,
						MessageType.Question, ButtonsType.YesNo, true, 
						"Você deseja cancelar o envio das mensagens da fila?",
						"Ainda falta enviar " + Util.Number(rem, false) + " mensage" + 
						(rem > 1 ? "ns" : "m") + ". " +
						"Fechando esta janela, você interromperá o envio dessas mensagens.");
					m.DefaultResponse = ResponseType.Yes;
					int result = m.Run();
					m.Destroy();
					if (result != (int) ResponseType.Yes)
						return;
					nodeview.NodeSelection.SelectAll();
					CancelClicked(null, null);
				}
			}
			this.Hide();
		}


#pragma warning disable 169
		private void ResendClicked(object sender, EventArgs args) {
			lock (queue) {
				CheckQueue(false); // assure that everything is on its places
				
				// Extract the items that will be resent
				ITreeNode[] selection = nodeview.NodeSelection.SelectedNodes;
				List<QueueItem> newItems = new List<QueueItem>(selection.Length);
				number -= 1;
				foreach (ITreeNode node in selection) {
					QueueItem item = node as QueueItem;
					switch (item.status) {
						case QueueStatus.Waiting:
							break;
						case QueueStatus.Sending:
							newItems.Add(item.Clone(number));
							break;
						default:
							if (sent.Remove(item)) {
								sentCount -= 1;
								msgCount -= 1;
								newItems.Add(item.Clone(number));
							}
							break;
					}
				}
				
				// Put the messages in the queue (like AddMessage)
				ResetCounters();
				foreach (QueueItem item in newItems) {
					queue[item.engine].Enqueue(item);
					msgCount += 1;
				}
				CheckQueue(true);
			}
		}
		
		
		private void ClearClicked(object sender, System.EventArgs e) {
			lock (queue) {
				sent.Clear();
				sentCount = 0;
				msgCount = 0;
				foreach (Queue<QueueItem> q in queue.Values)
					msgCount += q.Count;
				
				UpdateQueue();
			}
		}
		
		private void CancelClicked(object sender, System.EventArgs e) {
			lock (queue) {
				bool changed = false, cancelVerification = false;
				ITreeNode[] selected = nodeview.NodeSelection.SelectedNodes;
				nodeview.FreezeChildNotify();
				try {
					foreach (ITreeNode node in selected) {
						QueueItem item = node as QueueItem;
						switch (item.status) {
							case QueueStatus.Sending:
								changed = true;
								item.engine.Abort();
								item.Result = EngineResult.UserCancel;
								if (verifying.HasValue && verifying.Value.ValueA == item)
									cancelVerification = true;
								break;
							case QueueStatus.Waiting:
								changed = true;
								item.Result = EngineResult.UserCancel;
								break;
						}
					}
					CheckQueue(changed);
				} finally {
					nodeview.ThawChildNotify();
				}
				if (cancelVerification) verificationWindow.Cancel();
			}
		}

		private void ResendWithError(object sender, System.EventArgs e) {
			lock (queue) {
				CheckQueue(false); // assure that everything is on its places
				
				// Extract the items that will be resent
				List<QueueItem> newItems = new List<QueueItem>(sent.Count);
				foreach (QueueItem item in sent)
					if (item.status == QueueStatus.Error)
						newItems.Add(item);
				foreach (QueueItem item in newItems) {
					--sentCount;
					--msgCount;
					sent.Remove(item);
				}
				
				// Put the messages in the queue (like AddMessage)
				number -= 1;
				ResetCounters();
				foreach (QueueItem item in newItems) {
					queue[item.engine].Enqueue(item.Clone(number));
					++msgCount;
				}
				CheckQueue(true);
			}
		}

		private void DeleteClicked(object sender, System.EventArgs e) {
			lock (queue) {
				// See how many items would need to be canceled
				ITreeNode[] selection = nodeview.NodeSelection.SelectedNodes;
				int toCancel = 0;
				foreach (ITreeNode node in selection) {
					QueueItem item = node as QueueItem;
					switch (item.status) {
						case QueueStatus.Waiting:
						case QueueStatus.Sending:
							toCancel++;
							break;
					}
				}
				
				// See if the user really wants to do it
				if (toCancel > 0) {
					selection = null;
					MessageDialog m = Util.CreateMessageDialog(this, DialogFlags.DestroyWithParent,
						MessageType.Question, ButtonsType.YesNo, true, 
						"Você realmente deseja remover as mensagens selecionadas?",
						"Entre as mensagens que você selecionou há " + Util.Number(toCancel, false)
						+ (toCancel >= 2 ? " mensagens que não foram enviadas. "
						                 : " mensagem que não foi enviada. ")
						+ "Não será possível desfazer a remoção.");
					m.DefaultResponse = ResponseType.Yes;
					int result = m.Run();
					m.Destroy();
					if (result != (int) ResponseType.Yes) return;
					selection = nodeview.NodeSelection.SelectedNodes;
				}
				
				// Cancel the relevant messages
				nodeview.FreezeChildNotify();
				try {
					bool changed = false, cancelVerification = false;
					foreach (ITreeNode node in selection) {
						QueueItem item = node as QueueItem;
						switch (item.status) {
							case QueueStatus.Sending:
								changed = true;
								item.engine.Abort();
								item.Result = EngineResult.UserCancel;
								if (verifying.HasValue && verifying.Value.ValueA == item)
									cancelVerification = true;
								break;
							case QueueStatus.Waiting:
								changed = true;
								item.Result = EngineResult.UserCancel;
								break;
						}
					}
					CheckQueue(changed);
					if (cancelVerification) verificationWindow.Cancel();
					
					// Remove them
					foreach (ITreeNode node in selection) {
						if (sent.Remove(node as QueueItem))
							sentCount -= 1;
					}
					UpdateQueue();
				} finally {
					nodeview.ThawChildNotify();
				}
			}
		}
#pragma warning restore 169

	}
}
