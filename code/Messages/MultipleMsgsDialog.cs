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
 
#if !LITE
 
using System;
using Gtk;

using MensagemWeb.Windows;

namespace MensagemWeb.Messages {
	public sealed class MultipleMsgsDialog : Dialog {
		private static string title = "Envio de múltiplas mensagens";
		private static DialogFlags dialog_flags = DialogFlags.DestroyWithParent |
			DialogFlags.NoSeparator;
		private static object[] button_data = new object[] {Stock.No, ResponseType.No,
			Stock.Yes, ResponseType.Yes};
	
		private Message[] msgs;
		private int current = 0;
		
		private Label msgsLabel;
		private TextBuffer msgBuffer;
		private Arrow leftArrow = new Arrow(ArrowType.Left, ShadowType.None);
		private Arrow rightArrow = new Arrow(ArrowType.Right, ShadowType.None);
		
		
		
		public MultipleMsgsDialog(Message[] msgsToSend)
				: base(title, MainWindow.This, dialog_flags, button_data)
		{
			if (msgsToSend == null)
				throw new ArgumentNullException("msgsToSend");
			if (msgsToSend.Length < 2)
				throw new ArgumentException("msgsToSend");
			msgs = msgsToSend;
			
			DefaultResponse = ResponseType.Yes;
			Modal = false;
			Resizable = false;
			
			HBox hbox = new HBox();
			VBox.Add(hbox);
			hbox.BorderWidth = 10;
			hbox.Spacing = 10;
			
			Image img = new Image(Stock.DialogQuestion, IconSize.Dialog);
			img.Yalign = 0;
			hbox.PackStart(img, false, false, 0);
			
			VBox vbox = new VBox();
			hbox.Add(vbox);
			vbox.Spacing = 7;
			
			Label label1 = new Label(
				"Você está prestes a enviar mais de uma\n" +
				"mensagem ao(s) destinatário(s):");
			vbox.PackStart(label1, false, true, 0);
			
			VBox msgsBox = new VBox();
			Frame port = new Frame();
			port.ShadowType = ShadowType.In;
			port.Add(msgsBox);
			vbox.PackStart(port, true, true, 0);
				
				HBox titleBox = new HBox();
				msgsBox.PackStart(titleBox, false, true, 0);
				
					EventBox leftEventBox = new EventBox();
					leftEventBox.ButtonReleaseEvent += delegate {
						current -= 1;
						UpdateMessage();
					};
					leftEventBox.Add(leftArrow);
					titleBox.PackStart(leftEventBox, false, true, 0);
					
					msgsLabel = new Label(String.Empty);
					titleBox.PackStart(msgsLabel, true, true, 0);
					
					EventBox rightEventBox = new EventBox();
					rightEventBox.ButtonReleaseEvent += delegate {
						current += 1;
						UpdateMessage();
					};
					rightEventBox.Add(rightArrow);
					titleBox.PackStart(rightEventBox, false, true, 0);
				
				TextView msgView = new TextView();
				msgView.CursorVisible = false;
				msgView.WrapMode = WrapMode.WordChar;
				msgView.Editable = false;
				msgView.KeyPressEvent += delegate (object o, KeyPressEventArgs args) {
					switch (args.Event.Key) {
						case Gdk.Key.space:
							current += 1;
							if (current >= msgs.Length)
								current = 0;
							UpdateMessage();
							break;
						case Gdk.Key.Escape:
							this.Respond(ResponseType.No);
							break;
						case Gdk.Key.Return:
						case Gdk.Key.KP_Enter:
							this.Respond(ResponseType.Yes);
							break;
					}
				};
				msgBuffer = msgView.Buffer;
				msgsBox.PackStart(msgView, true, true, 0);
			
			Label label2 = new Label();
			label2.Markup = "<b>Você deseja enviar essas mensagens?</b>";
			vbox.PackStart(label2, false, true, 0);
			
			UpdateMessage();
			hbox.ShowAll();
		}
	
	
		
		private void UpdateMessage() {
			// First assure that current is under the limit [0 .. msgs.Length[
			int length = msgs.Length;
			if (current < 0)
				current = 0;
			if (current >= length)
				current = length - 1;
			
			// Update the texts
			msgsLabel.Text = String.Format("Mensagem {0} de {1}", current + 1, length);
			msgBuffer.Text = msgs[current].Contents;
			
			// Update the arrows
			if (current == 0) {
				leftArrow.Sensitive = false;
				rightArrow.Sensitive = true;
			} else if (current == (length - 1)) {
				leftArrow.Sensitive = true;
				rightArrow.Sensitive = false;
			} else {
				leftArrow.Sensitive = true;
				rightArrow.Sensitive = true;
			}
		}
	}
}

#endif