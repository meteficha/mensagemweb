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
using Gdk;

using MensagemWeb.Engines;
using MensagemWeb.Logging;

namespace MensagemWeb.Windows {
	public delegate void CodeDelegate(string code);
	
	
	public class VerificationWindow : Gtk.Window {
		private Gtk.Image image;
		private Entry code;
		private CodeDelegate callback;
		
		public VerificationWindow(Pixbuf pixbuf, CodeDelegate callback) 
				: base(String.Empty) 
		{
			this.callback = callback;
			
			this.TransientFor = QueueWindow.This;
			this.DestroyWithParent = true;
			this.WindowPosition = WindowPosition.Center;
			this.Resizable = false;
			this.Modal = Util.OnWindows; // Sometimes the windows appeared behind the window
			this.TypeHint = Gdk.WindowTypeHint.Dialog;
			
			this.DeleteEvent += TryToClose;
			
			VBox vbox = new VBox();
			vbox.Spacing = 6;
			vbox.BorderWidth = 12;
			this.Add(vbox);
			
			Label titleLabel = new Label("<span size=\"large\" weight=\"bold\">Transcreva" +
			                               " o código abaixo</span>"); 
			titleLabel.UseMarkup = true;
			titleLabel.Justify = Justification.Center;
			vbox.PackStart(titleLabel, false, true, 0);
			
			Label infoLabel = new Label("Por favor digite na caixa abaixo o que " +
				"você vê na imagem e pressione ENTER para enviar a mensagem.");
			infoLabel.UseMarkup = true;
			infoLabel.Wrap = true;
			vbox.PackStart(infoLabel, false, true, 0);
			
			image = new Gtk.Image(Scale(pixbuf));
			Frame imageFrame = new Frame();
			imageFrame.Shadow = ShadowType.In;
			imageFrame.Add(image);
			Alignment imageAlign = new Alignment(0.5f, 0.5f, 0, 0);
			imageAlign.Add(imageFrame);
			vbox.PackStart(imageAlign, false, false, 0);
			
			code = new Entry();
			code.ModifyFont(Pango.FontDescription.FromString("26"));
			code.Alignment = 0.5f;
			code.KeyReleaseEvent += delegate (object o, KeyReleaseEventArgs args) {
				if (args.Event.Key == Gdk.Key.Escape)
					TryToClose(this, null);
			};
			code.ActivatesDefault = true;
			vbox.PackStart(code, true, true, 0);
			
			ButtonBox buttonBox = new HButtonBox();
			buttonBox.Layout = ButtonBoxStyle.End;
			buttonBox.Spacing = 6;
			vbox.PackStart(buttonBox, false, true, 0);
			
			Button cancelButton = new Button(Stock.Cancel);
			cancelButton.Clicked += TryToClose;
			buttonBox.Add(cancelButton);
			
			Button okButton = new Button(Stock.Ok);
			okButton.Clicked += SendCode;
			buttonBox.Add(okButton);
			
			okButton.CanDefault = true;
			okButton.GrabDefault();
			vbox.ShowAll();
			this.Present();
			this.ShowAll();
			this.GrabFocus();
			code.GrabFocus();
			
			Util.SetProperty(this, "UrgencyHint", true);
		}
		
		
		
		protected void SendCode(object sender, EventArgs args) {
			this.Hide();
			callback(code.Text.Trim());
			this.Destroy();
		}
		
		
		public void Cancel() {
			this.Hide();
			callback(null);
			this.Destroy();			
		}
		
		
		protected void TryToClose(object sender, EventArgs args) {
			MessageDialog md = Util.CreateMessageDialog(this, DialogFlags.DestroyWithParent, 
				MessageType.Question, ButtonsType.YesNo, false,
				"Cancelar envio de mensagem?", "Ao fechar esta janela ou clicar no botão " +
				"\"Cancelar\" você deixará de enviar esta mensagem.");
			md.DefaultResponse = ResponseType.Yes;
			int response = md.Run();
			md.Destroy();
			if (response == (int) ResponseType.Yes)
				Cancel();
			else if (args is DeleteEventArgs)
				(args as DeleteEventArgs).RetVal = true;
		}
			
		
		
		private static Pixbuf Scale(Pixbuf source) {
			// Base and desired dimensions
			int destHeight = 90;
			int sourceHeight = source.Height;
			
			int destWidth = 370;
			int sourceWidth = source.Width;
			
			// Check if we really need to do something
			if (sourceHeight >= destHeight && sourceWidth >= destWidth)
				return source;
			
			// Keep proportion
			int tempWidth = (destHeight * sourceWidth) / sourceHeight;
			if (tempWidth >= destWidth)
				destWidth = tempWidth;
			else
				destHeight = (destWidth * sourceHeight) / sourceWidth;
			
			// Scale the image
			try {
				Pixbuf target = source.ScaleSimple(destWidth, destHeight, InterpType.Hyper);
				if (target != null) {
					source.Dispose();
					return target;
				}
			} catch (Exception e) {
				Logger.Log(typeof(VerificationWindow), 
					"Error while scaling, falling back to unscaled.\n{0}", e);
			}
			
			// If anything else happens, return the original image
			return source;
		}
	}
}
