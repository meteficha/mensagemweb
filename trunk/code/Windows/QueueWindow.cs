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
			Waiting,
			Error,
			Cancelled,
			Sent
		}
		
		
		// An item in the message queue
		[TreeNode(ListOnly=true)]
		private class QueueItem : TreeNode, IComparable<QueueItem> {
			public QueueStatus status = QueueStatus.Waiting;
			public Message message;
			public IEngine engine;
			private EngineResult result = null;
			public string sendingStatus = "???";
			public double progress = 0.0;
			private double number;
			private const double cloneProp = 1 + 1e-15;
			
			public QueueItem(Message message) {
				if (message == null) 
					throw new ArgumentNullException("Message shouldn't be null.");
				if (message.Destinations.Count != 1)
					throw new ArgumentException("Message should have just one " +
						"destination to be put in the queue.");
				
				this.message = message;
				this.engine = PhoneBook.Get(message.Destinations[0]).RealEngine;
				this.number = DateTime.Now.Ticks;
				
				messageContents = Util.Split(message.Contents, 40);
				destinationName = message.Destinations[0];
			}
			
			public QueueItem Clone() {
				QueueItem clone = new QueueItem(message);
				clone.number = number * cloneProp;
				return clone;
			}
			
			public void FireChanged() {
				try {
					OnChanged();
				} catch {
					// Don't do anything
				}
			}
			
			public int CompareTo(QueueItem other) {
				//int ret = status.CompareTo(other.status);
				//if (ret != 0) return ret;
				return number.CompareTo(other.number);
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
							return "Enviada!";
						case QueueStatus.Waiting:
							return "Esperando na fila";
						case QueueStatus.Cancelled:
							return "Cancelada";
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
											"funcionando e se você precisa habilitar o proxy.";
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
            public string destinationName;
            
            [TreeNodeValue(Column=3)]
            public string messageContents;
		}
		
		
		
			
		private static QueueWindow this_;	
		public static QueueWindow This {
			get {
				if (this_ == null) this_ = new QueueWindow(); 
				return this_; 
			}
		}
	
	
		// The message queue, grouped by IEngine
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
			"Suas mensagens estão sendo enviadas aos seus destinatários...";
			
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
#pragma warning restore 649
			
		
		private QueueWindow() 
				: base(String.Empty) 
		{
			TransientFor = MainWindow.This;
			Stetic.Gui.Build(this, typeof(MensagemWeb.Windows.QueueWindow));
			this.DeleteEvent += delegate (object sender, DeleteEventArgs a) {
				a.RetVal = true;
				CloseWindow(sender, a);
			};
			
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
			geom.MinWidth = 300;
			geom.MinHeight = 200;
			this.SetGeometryHints(nodepanel, geom, Gdk.WindowHints.MinSize);
			
			// Nodeview menu
			nodeviewMenu = new Menu();
			nodeviewMenu.Append(resendAction.CreateMenuItem());
			nodeviewMenu.Append(new SeparatorMenuItem());
			nodeviewMenu.Append(cancelAction.CreateMenuItem());
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
			nodeview.AppendColumn("Destinatário", new CellRendererText(), "text", 2);
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
				
				foreach (string dest in (IEnumerable<string>)message.Destinations)
					foreach (Message msg in msgs) {
						QueueItem item = new QueueItem(msg.ChangeDestination(dest));
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
			MessageDialog m = Util.CreateMessageDialog(MainWindow.This, DialogFlags.DestroyWithParent,
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
					
					bool errors = false;
					foreach (QueueItem item in sent) {
						// XXX: Move this to the methods who change sent?
						if (item.status == QueueStatus.Error) {
							errors = true;
							break;
						}
					}
					if (errors) {
						errorbox.Show();
						progressbox.Hide();
					} else {
						errorbox.Hide();
						progressbox.Show();
					}
				} else {
					titleLabel.Markup = sendingTitle;
					subLabel.Markup = sendingSub;
					closebutton.Sensitive = false;
					errorbox.Hide();
					progressbox.Show();
					progressbar.Show();
				}
								
				nodes.Clear();
				remaining.AddRange(sent);
				remaining.Sort();
				foreach (QueueItem item in (IEnumerable<QueueItem>)remaining)
					nodes.AddNode(item);
				nodeview.ColumnsAutosize();
			}
			
			UpdateStatus();
		}
		
		
		
		// Updates our status (the progressbar, currently)
		private void UpdateStatus() {
			if (msgCount > 0) {
				double done = sentCount;
				lock (queue)
					foreach (Queue<QueueItem> q in queue.Values)
						foreach (QueueItem item in q)
							done += item.progress;
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
			item.status = QueueStatus.Sending;
			item.sendingStatus = "Baixando código...";
			item.progress = 0.0;
			item.FireChanged();
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
				item.sendingStatus = "Mostrando código...";
				item.progress = 0.3333333333333;
				item.FireChanged();
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
			item.sendingStatus = "Enviando código...";
			item.progress = 0.666666666;
			item.FireChanged();
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
				});
			});
			worker.Name = "SendCode -- " + engine.Name;
			worker.Start();
		}
		
		
		
		private void NodeSelectionChanged(object sender, EventArgs args) {
			ITreeNode[] selected = nodeview.NodeSelection.SelectedNodes;
			resendAction.Sensitive = selected.Length > 0;
			
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
			if (closebutton.Sensitive)
				this.Hide();
		}


#pragma warning disable 169
		private void ResendClicked(object sender, EventArgs args) {
			lock (queue) {
				CheckQueue(false); // assure that everything is on its places
				
				// Extract the items that will be resent
				ITreeNode[] selection = nodeview.NodeSelection.SelectedNodes;
				List<QueueItem> newItems = new List<QueueItem>(selection.Length);
				foreach (ITreeNode node in selection) {
					QueueItem item = node as QueueItem;
					switch (item.status) {
						case QueueStatus.Waiting:
							break;
						case QueueStatus.Sending:
							newItems.Add(item.Clone());
							break;
						default:
							if (sent.Remove(item)) {
								sentCount -= 1;
								msgCount -= 1;
								newItems.Add(item.Clone());
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
				bool changed = false;
				ITreeNode[] selected = nodeview.NodeSelection.SelectedNodes;
				foreach (ITreeNode node in selected) {
					QueueItem item = node as QueueItem;
					switch (item.status) {
						case QueueStatus.Sending:
							changed = true;
							item.engine.Abort();
							item.Result = EngineResult.UserCancel;
							if (verifying.HasValue && verifying.Value.ValueA == item)
								verificationWindow.Cancel();
							break;
						case QueueStatus.Waiting:
							changed = true;
							item.Result = EngineResult.UserCancel;
							break;
					}
				}
				CheckQueue(changed);
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
				ResetCounters();
				foreach (QueueItem item in newItems) {
					queue[item.engine].Enqueue(item.Clone());
					++msgCount;
				}
				CheckQueue(true);
			}
		}
#pragma warning restore 169

	}
}