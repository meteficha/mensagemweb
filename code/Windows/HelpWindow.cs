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

namespace MensagemWeb.Windows {
	/// <summary>
	/// The window that is shown when the user clicks on the Help button.
	/// </summary>
	public sealed class HelpWindow : Window {
		private static HelpWindow this_;
		public static HelpWindow This {
			get {
				if (this_ == null)
					this_ = new HelpWindow();
				return this_; 
			}
		}
		
		private HelpWindow()
				: base("Ajuda")
		{
			// Some properties
			this.TransientFor = MainWindow.This;
			this.DestroyWithParent = true;
			this.TypeHint = Gdk.WindowTypeHint.Normal;
			this.WindowPosition = WindowPosition.CenterOnParent;
			this.Modal = false;
			this.Resizable = true;
			
			// Just hide us when the window is closed
			this.DeleteEvent += delegate (object sender, DeleteEventArgs a) {
				this.Hide();
				a.RetVal = true;
			};
			
			// Main VBox
			VBox mainBox = new VBox();
			mainBox.Spacing = 12;
			mainBox.BorderWidth = 12;
			this.Add(mainBox);
			
			// The TextView
			RichTextBuffer buffer;
			Widget sw = RichTextBuffer.NewTextView(out buffer);
			Gdk.Geometry geom = new Gdk.Geometry();
			geom.MinWidth = 550; geom.MinHeight = 400;
			this.SetGeometryHints(sw, geom, Gdk.WindowHints.MinSize);
			buffer.AddTitle("Ajuda");
			
				buffer.AddSubtitle("Dicas curtas");
				buffer.AddItem("Todos os campos que faltam ser preenchidos ou que estão "+
					"preenchidos de forma errada aparecem destacados com uma cor diferente.\n");
				buffer.AddItem("A imagem do código veio ilegível ou muito difícil de escrever? "+
					"Apenas deixe o campo do código em branco e aperte Enter para que outra "+
					"imagem seja mostrada para você.\n");
				buffer.AddItem("Prefere usar o teclado? Na janela que aparece quando mais de uma "+
					"mensagem será enviada, aperte [Espaço] para ver a próxima mensagem, [ESC] "+
					"para cancelar e [Enter] para enviar.\n");
				
				buffer.AddSubtitle("Enviando uma mensagem");
				buffer.AddText("\tPara enviar uma mensagem siga estes simples passos:\n");
				buffer.AddItem("Escolha seu destinatário: isso pode ser feito clicando na seta "+
					"posicionada à direita da caixa nomeada \"Destinatário\" e selecionando o "+
					"nome da pessoa que irá receber a mensagem (veja a parte \"Adicionando "+
					"alguém à agenda\" desta ajuda para saber como colocar uma pessoa nesta "+
					"caixa). Para enviar a mesma mensagem para mais de um destinatário, clique no "+
					"botão com um símbolo de adição (\"+\") quantas vezes forem necessárias "+
					"até você ter escolhido o nome de todas as pessoas que irão receber a "+
					"mensagem.\n");
				buffer.AddItem("Escreva seu nome e seu número de telefone nas caixas da seção "+
					"nomeada \"Remetente\". Esses dados não serão confirmados e nenhum tipo de "+
					"informação sua será usada para qualquer outro fim senão exibir para o "+
					"destinatário quem foi que enviou a mensagem. Não é necessário escrever "+
					"o telefone, apenas o seu nome é de preenchimento obrigatório.\n");
				buffer.AddItem("Digite a mensagem que será enviada no campo \"Mensagem\". Note "+
					"que todos os acentos serão eliminados por restrições do sistema.\n");
				buffer.AddItem("Clique no botão \"Enviar\" e siga as instruções que aparecerão "+
					"na tela.\n");
				
				buffer.AddSubtitle("Adicionando alguém à agenda");
				buffer.AddText("\tO MensagemWeb possui uma agenda de telefones própria e para "+
					"enviar uma mensagem o destinatário desta deve estar incluso na sua agenda."+
					" Para adicionar alguém à ela, siga os seguintes passos:\n");
				buffer.AddItem("Clique na seta posicionada à direita da caixa nomeada "+
					"\"Destinatário\" e selecione a opção \"Novo telefone...\". Uma janela "+
					"com alguns campos a serem preenchidos será aberta.\n");
				buffer.AddItem("Preencha os dados da pessoa que você "+
					"deseja adicionar à sua agenda e clique no botão \"Salvar\".\n");
			
				buffer.AddSubtitle("O que é um \"número de celular desconhecido\"?");
				buffer.AddText("\tEste programa possui um sistema de detecção automática da "+
					"operadora usada em um número de celular de acordo com o seu DDD e o seu "+
					"prefixo.");
				buffer.AddText("O mais provável é que a operadora correspondente ao celular "+
					"que você selecionou não é suportada pelo MensagemWeb. Para saber se esse "+
					"é realmente o problema, verifique com o dono do celular qual é a operadora "+
					"que ele usa e veja se ela está disponível na caixa \"Operadora\" que pode "+
					"ser achada na janela de adição ou edição de telefones na sua agenda de "+
					"contatos. Agora:\n");
				buffer.AddItem("Caso a operadora conste na lista, tente selecioná-la (ao invés "+
					"de selecionar \"Automático (recomendado)\") e veja se é possível enviar "+
					"mensagens (lembre-se de verificar se o destinatário realmente as "+
					"recebeu). Se isso for possível, por favor ");
				buffer.AddLink("contate-nos", "http://mensagemweb.codigolivre.org.br/");
				buffer.AddText(" assim que possível, visto que isso é um defeito no MensagemWeb.\n");
				buffer.AddItem("Se a operadora não constar na lista, nós pedimos que, se possível "+
					"você nos ajude a suportá-la na próxima versão do MensagemWeb. Nos ajudar é "+
					"muito fácil e rápido e você não precisa ter nenhum conhecimento técnico. "+
					"Se você puder e quiser nos ajudar, ");
				buffer.AddLink("entre em contato conosco", "http://mensagemweb.codigolivre.org.br/");
				buffer.AddText("!\n");
					
				
				buffer.AddSubtitle("Obtendo mais ajuda");
				buffer.AddText("\tSe seu problema não foi solucionado ou sua dúvida não foi "+
					"respondida, por favor procure-nos no site ");
				buffer.AddMensagemWebSite();
				buffer.AddText(". Ficaremos felizes em ajudá-lo!\n");
			mainBox.PackStart(sw, true, true, 0);
			
			// Buttons
			ButtonBox buttonBox = new HButtonBox();
			buttonBox.Spacing = 6;
			buttonBox.Layout = ButtonBoxStyle.Spread;
			mainBox.PackStart(buttonBox, false, true, 0);
			
				Button closeButton = new Button(Stock.Close);
				closeButton.Clicked += delegate {
					this.Hide();
				};
				buttonBox.Add(closeButton);
		}
		
		
		public void ShowThis() {
			this.ShowAll();
			this.Present();
		}
	}
}
