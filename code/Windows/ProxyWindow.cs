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
using System.Net;
using Gtk;

using MensagemWeb;
using MensagemWeb.Config;

namespace MensagemWeb.Windows {
	public class ProxyWindow : Window {
		private static ProxyWindow this_;
		public static ProxyWindow This {
			get {
				if (this_ == null)
					this_ = new ProxyWindow();
				return this_; 
			}
		}
		
		// Tooltips
		Tooltips tooltips = new Tooltips();
		private void SetTip(Widget wd, string text) {tooltips.SetTip(wd, text, text);}
		
		// Radio buttons
		private RadioButton noProxyRB;
		private RadioButton systemProxyRB;
		private RadioButton customProxyRB;
		
		// Labels
		private Label hostLabel = new Label("_Endereço:");
		private Label portLabel = new Label("_Porta:");
		private Label usernameLabel = new Label();
		private Label passwordLabel = new Label();
		
		// Entries
		private Entry hostEntry = new Entry();
		private SpinButton portEntry = new SpinButton(1, 65535, 1);
		private Entry usernameEntry = new Entry();
		private Entry passwordEntry = new Entry();
		
		// Buttons
		private Button revertButton = new Button(Stock.RevertToSaved);
		private Button saveButton = new Button(Stock.Save);
		
		
		
		public void ShowThis() {
			RevertChanges(null, null);
			CheckInput(null, null);
			this.ShowAll();
			this.Present();
		}
		
		
		private void RevertChanges(object sender, EventArgs args) {
			if (ProxyConfig.UseProxy) {
				if (ProxyConfig.UseSystemProxy)
					systemProxyRB.Active = true;
				else
					customProxyRB.Active = true;
			} else
				noProxyRB.Active = true;
			
			hostEntry.Text = ProxyConfig.Host;
			portEntry.Value = ProxyConfig.Port;
			usernameEntry.Text = ProxyConfig.Username;
			passwordEntry.Text = ProxyConfig.Password;
		}
		
		
		private void SaveAndClose(object sender, EventArgs args) {
			if (!HostOk || !PasswordOk) {
				Util.ShowMessage(this, "Dados preenchidos incorretamente",
				                     "Por favor preencha corretamente os campos desta" +
				                     "cados antes de salvar as configurações.",
				                     MessageType.Error, false);
				if (!HostOk)
					hostEntry.GrabFocus();
				else
					passwordEntry.GrabFocus();
				return;
			}
			if (noProxyRB.Active) {
				ProxyConfig.UseProxy = false;
				//ProxyConfig.UseSystemProxy doesn't matter
			} else {
				ProxyConfig.UseProxy = true;
				ProxyConfig.UseSystemProxy = systemProxyRB.Active;
			}
			
			ProxyConfig.Host = hostEntry.Text;
			ProxyConfig.Port = (int) portEntry.Value;
			ProxyConfig.Username = usernameEntry.Text;
			ProxyConfig.Password = passwordEntry.Text;
			
			this.Hide();
		}
		
		
		private void CheckInput(object sender, EventArgs args) {
			if (customProxyRB.Active) {
				// Activate the entries
				AddressEnabled = true;
				UsernameEnabled = true;
				PasswordEnabled = (usernameEntry.Text.Length > 0);
				
				// Check their color
				Util.SetColor(hostEntry, HostOk);
				Util.SetColor(passwordEntry, PasswordOk);
			} else {
				// Disable the entries
				AddressEnabled = false;
				UsernameEnabled = false;
				PasswordEnabled = false;
				
				// Change their color to the normal one
				Util.SetColorNormal(hostEntry);
				Util.SetColorNormal(passwordEntry);
			}
		}
		
		private bool HostOk {
			get { return (!customProxyRB.Active || hostEntry.Text.Length > 0); }
		}
		
		private bool PasswordOk  { 
			get { return (!passwordEntry.Sensitive || passwordEntry.Text.Length > 0); }
		}
		
		private bool addressEnabled = true;
		private bool AddressEnabled {
			get { return addressEnabled; }
			set {
				if (value == addressEnabled)
					return;
				addressEnabled = value;
				hostLabel.Sensitive = value;
				hostEntry.Sensitive = value;
				portLabel.Sensitive = value;
				portEntry.Sensitive = value;
			}
		}
		private bool usernameEnabled = true;
		private bool UsernameEnabled {
			get { return usernameEnabled; }
			set {
				if (value == usernameEnabled)
					return;
				usernameEnabled = value;
				usernameLabel.Sensitive = value;
				usernameEntry.Sensitive = value;
			}
		}
		private bool passwordEnabled = true;
		private bool PasswordEnabled {
			get { return passwordEnabled; }
			set {
				if (value == passwordEnabled)
					return;
				passwordEnabled = value;
				passwordLabel.Sensitive = value;
				passwordEntry.Sensitive = value;
			}
		}
		
		
		private ProxyWindow()
				: base("Configuração de Proxy")
		{
			// Some properties
			this.TransientFor = MainWindow.This;
			this.DestroyWithParent = true;
			this.WindowPosition = WindowPosition.CenterOnParent;
			this.Resizable = false;
			this.TypeHint = Gdk.WindowTypeHint.Dialog;
			this.DeleteEvent += WhenClosing;
			this.Modal = Util.OnWindows;
			
			// Main box
			VBox mainBox = new VBox();
			mainBox.Spacing = 6;
			mainBox.BorderWidth = 12;
			this.Add(mainBox);
			
			// Create the radio buttons
			noProxyRB = new RadioButton("Acessar a Internet _diretamente (sem proxy)");
			noProxyRB.Toggled += CheckInput;
			SetTip(noProxyRB, "Selecione caso sua conexão à Internet seja direta. Em geral esta " +
				"é a opção que você quer marcar caso você não saiba o que é um proxy.");
			systemProxyRB = new RadioButton(noProxyRB, "Usar configurações do _sistema para proxy");
			systemProxyRB.Toggled += CheckInput;
			SetTip(systemProxyRB, "Tenta descobrir automaticamente suas configurações de proxy. "+
				"(recomendado)");
			customProxyRB = new RadioButton(noProxyRB, "Configurar proxy _manualmente:");
			customProxyRB.Toggled += CheckInput;
			SetTip(customProxyRB, "Permite que você informe manualmente o tipo de proxy que "+
				"você usa.");
			mainBox.Add(noProxyRB);
			mainBox.Add(systemProxyRB);
			mainBox.Add(customProxyRB);
			
			// Create the panel that will carry the manual configuration
			Alignment align = new Alignment(0.5f, 0.5f, 1.0f, 1.0f);
			align.LeftPadding = 20;
			mainBox.Add(align);
			Table proxyTable = new Table(4, 4, false);
			proxyTable.RowSpacing = 6;
			proxyTable.ColumnSpacing = 6;
			align.Add(proxyTable);
			
				// Address
				hostLabel.Xalign = 1;
				hostLabel.MnemonicWidget = hostEntry;
				proxyTable.Attach(hostLabel, 0, 1, 0, 1,
					AttachOptions.Shrink | AttachOptions.Fill,
					AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
				
				hostEntry.Changed += CheckInput;
				SetTip(hostEntry, "O endereço do computador responsável pelo proxy. Note " +
					"que muitas vezes o endereço é dado na forma \"http://nome:número/\". " +
					"Nesses casos digite aqui a parte do \"nome\" e " + 
					"digite o \"número\" no campo \"Porta\".");
				proxyTable.Attach(hostEntry, 1, 2, 0, 1,
					AttachOptions.Expand | AttachOptions.Fill,
					AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
				
				// Port
				portLabel.Xalign = 1;
				portLabel.MnemonicWidget = portEntry;
				proxyTable.Attach(portLabel, 2, 3, 0, 1,
					AttachOptions.Shrink | AttachOptions.Fill,
					AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
				
				portEntry.Changed += CheckInput;
				SetTip(portEntry, "Porta de acesso usada pelo proxy.");
				proxyTable.Attach(portEntry, 3, 4, 0, 1,
					AttachOptions.Shrink | AttachOptions.Fill,
					AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
				
				// Username
				usernameLabel.Markup = "Usuário <small>(opcional)</small>:";
				usernameLabel.Xalign = 1;
				usernameLabel.MnemonicWidget = usernameEntry;
				proxyTable.Attach(usernameLabel, 0, 1, 1, 2,
					AttachOptions.Shrink | AttachOptions.Fill,
					AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
				
				usernameEntry.Changed += CheckInput;
				SetTip(usernameEntry, "Caso o proxy requeira autenticação (proxy autenticado),"+
					" digite aqui o seu nome de usuário.");
				proxyTable.Attach(usernameEntry, 1, 4, 1, 2,
					AttachOptions.Shrink | AttachOptions.Fill,
					AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
				
				// Password
				passwordLabel.Markup = "Senha:";
				passwordLabel.Xalign = 1;
				passwordLabel.MnemonicWidget = passwordEntry;
				proxyTable.Attach(passwordLabel, 0, 1, 2, 3,
					AttachOptions.Shrink | AttachOptions.Fill,
					AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
				
				passwordEntry.Changed += CheckInput;
				SetTip(passwordEntry, "Informe aqui a senha correspondente ao nome de usuário "+
					"informado acima.");
				passwordEntry.Visibility = false;
				proxyTable.Attach(passwordEntry, 1, 4, 2, 3,
					AttachOptions.Shrink | AttachOptions.Fill,
					AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
			
			// Buttons of the window
			HButtonBox buttonBox = new HButtonBox();
			buttonBox.Spacing = 6;
			buttonBox.Layout = ButtonBoxStyle.End;
			proxyTable.Attach(buttonBox, 1, 4, 3, 4,
				AttachOptions.Shrink | AttachOptions.Fill,
				AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
							
				SetTip(revertButton, "Reverte as configurações ao estado em que estavam " +
					"anteriormente (descarta suas alterações).");
				revertButton.Clicked += RevertChanges;
				buttonBox.Add(revertButton);
				
				SetTip(saveButton, "Salva as configurações e fecha esta janela de configuração.");
				saveButton.Clicked += SaveAndClose;
				buttonBox.Add(saveButton);
			
			// Now we're done! (finally)
		}
		
		
		private void WhenClosing(object sender, DeleteEventArgs args) {
			bool changed;
			// Check if the proxy type changed
			if (ProxyConfig.UseProxy) {
				if (ProxyConfig.UseSystemProxy) {
					changed = !systemProxyRB.Active;
				} else {
					changed = !customProxyRB.Active;
				}
			} else {
				changed = !noProxyRB.Active;
			}
			
			// If not, check if the manual proxy settings changed
			if (!changed) {
				changed = (hostEntry.Text != ProxyConfig.Host 
					|| portEntry.Value != ProxyConfig.Port
					|| usernameEntry.Text != ProxyConfig.Username 
					|| passwordEntry.Text != ProxyConfig.Password);
			}
			
			if (changed) {
				// If they changed, asks to the user if he/she wants to save it
				MessageDialog m = Util.CreateMessageDialog(this,
					DialogFlags.DestroyWithParent, MessageType.Question,
					ButtonsType.None, false, "Alterações ainda não salvas",
					"Você gostaria de salvar as alterações feitas às configurações de proxy " +
					"antes de fechar esta janela?");
				m.AddButton("Fechar _sem salvar", ResponseType.No);
				m.AddButton(Stock.Cancel, ResponseType.Cancel);
				m.AddButton(Stock.Save, ResponseType.Yes);
				m.DefaultResponse = ResponseType.Yes;
				int result = m.Run();
				m.Destroy();
				if (result == (int) ResponseType.Yes)
					SaveAndClose(null, null);
				else if (result == (int) ResponseType.No)
					this.Hide();
				// else if (result == ResponseType.Cancel) { }
			} else
				// Or else just hides the window
				this.Hide();
			
			// We'll never destroy this window
			args.RetVal = true;
		}
	}
}
