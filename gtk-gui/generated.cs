// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.42
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace Stetic {
    
    
    internal class Gui {
        
        private static bool initialized;
        
        public static void Build(object obj, System.Type type) {
            Stetic.Gui.Build(obj, type.FullName);
        }
        
        public static void Build(object obj, string id) {
            if ((Stetic.Gui.initialized == false)) {
                Stetic.Gui.Initialize();
            }
            System.Collections.Hashtable bindings = new System.Collections.Hashtable();
            if ((id == "MensagemWeb.Windows.ErrorWindow")) {
                Gtk.Dialog cobj = ((Gtk.Dialog)(obj));
                // Widget MensagemWeb.Windows.ErrorWindow
                cobj.Title = "";
                cobj.WindowPosition = ((Gtk.WindowPosition)(4));
                cobj.HasSeparator = false;
                cobj.Events = ((Gdk.EventMask)(256));
                cobj.Name = "MensagemWeb.Windows.ErrorWindow";
                // Internal child MensagemWeb.Windows.ErrorWindow.VBox
                Gtk.VBox w1 = cobj.VBox;
                w1.Events = ((Gdk.EventMask)(256));
                w1.Name = "dialog_VBox";
                // Container child dialog_VBox.Gtk.Box+BoxChild
                Gtk.HBox w2 = new Gtk.HBox();
                w2.Spacing = 12;
                w2.BorderWidth = ((uint)(12));
                w2.Events = ((Gdk.EventMask)(0));
                w2.Name = "hbox1";
                // Container child hbox1.Gtk.Box+BoxChild
                Gtk.Image w3 = new Gtk.Image();
                w3.Pixbuf = Gtk.IconTheme.Default.LoadIcon("gtk-dialog-error", 48, 0);
                w3.Yalign = 0F;
                w3.Events = ((Gdk.EventMask)(0));
                w3.Name = "image3";
                bindings["image3"] = w3;
                w2.Add(w3);
                Gtk.Box.BoxChild w4 = ((Gtk.Box.BoxChild)(w2[w3]));
                w4.Position = 0;
                w4.Expand = false;
                w4.Fill = false;
                // Container child hbox1.Gtk.Box+BoxChild
                Gtk.VBox w5 = new Gtk.VBox();
                w5.Spacing = 12;
                w5.Events = ((Gdk.EventMask)(0));
                w5.Name = "vbox1";
                // Container child vbox1.Gtk.Box+BoxChild
                Gtk.Label w6 = new Gtk.Label();
                w6.LabelProp = "<span size=\"large\" weight=\"bold\">Ocorreu algum erro inesperado</span>";
                w6.UseMarkup = true;
                w6.Selectable = true;
                w6.Xalign = 0F;
                w6.CanFocus = true;
                w6.Events = ((Gdk.EventMask)(0));
                w6.Name = "label1";
                bindings["label1"] = w6;
                w5.Add(w6);
                Gtk.Box.BoxChild w7 = ((Gtk.Box.BoxChild)(w5[w6]));
                w7.Position = 0;
                w7.Expand = false;
                w7.Fill = false;
                // Container child vbox1.Gtk.Box+BoxChild
                Gtk.Label w8 = new Gtk.Label();
                w8.LabelProp = "Isso significa que existe algum problema interno no MensagemWeb que ainda não foi identificado, então nós gostaríamos que você procurasse ajuda em nossos site (<i>mensagemweb.codigolivre.org.br</i>) para que possamos resolvê-lo e evitar novos transtornos.";
                w8.UseMarkup = true;
                w8.Wrap = true;
                w8.Selectable = true;
                w8.Xalign = 0F;
                w8.CanFocus = true;
                w8.Events = ((Gdk.EventMask)(0));
                w8.Name = "label2";
                bindings["label2"] = w8;
                w5.Add(w8);
                Gtk.Box.BoxChild w9 = ((Gtk.Box.BoxChild)(w5[w8]));
                w9.Position = 1;
                w9.Expand = false;
                w9.Fill = false;
                // Container child vbox1.Gtk.Box+BoxChild
                Gtk.Label w10 = new Gtk.Label();
                w10.LabelProp = "<b>Informações detalhadas (se possível, copie e cole no seu pedido de ajuda):</b>";
                w10.UseMarkup = true;
                w10.Wrap = true;
                w10.Selectable = true;
                w10.Xalign = 0F;
                w10.CanFocus = true;
                w10.Events = ((Gdk.EventMask)(0));
                w10.Name = "label3";
                bindings["label3"] = w10;
                w5.Add(w10);
                Gtk.Box.BoxChild w11 = ((Gtk.Box.BoxChild)(w5[w10]));
                w11.Position = 2;
                w11.Expand = false;
                w11.Fill = false;
                // Container child vbox1.Gtk.Box+BoxChild
                Gtk.ScrolledWindow w12 = new Gtk.ScrolledWindow();
                w12.VscrollbarPolicy = ((Gtk.PolicyType)(1));
                w12.HscrollbarPolicy = ((Gtk.PolicyType)(1));
                w12.ShadowType = ((Gtk.ShadowType)(1));
                w12.CanFocus = true;
                w12.Events = ((Gdk.EventMask)(0));
                w12.Name = "scrolledwindow1";
                // Container child scrolledwindow1.Gtk.Container+ContainerChild
                Gtk.TextView w13 = new Gtk.TextView();
                w13.Editable = false;
                w13.CanFocus = true;
                w13.Events = ((Gdk.EventMask)(0));
                w13.Name = "textview";
                bindings["textview"] = w13;
                w12.Add(w13);
                bindings["scrolledwindow1"] = w12;
                w5.Add(w12);
                Gtk.Box.BoxChild w15 = ((Gtk.Box.BoxChild)(w5[w12]));
                w15.Position = 3;
                // Container child vbox1.Gtk.Box+BoxChild
                Gtk.Label w16 = new Gtk.Label();
                w16.LabelProp = "<i>Se desejar, verifique se há informações pessoais acima e troque-as por \"confidencial\" antes de enviar o pedido de ajuda.</i>";
                w16.UseMarkup = true;
                w16.Wrap = true;
                w16.Xalign = 0F;
                w16.Events = ((Gdk.EventMask)(0));
                w16.Name = "label4";
                bindings["label4"] = w16;
                w5.Add(w16);
                Gtk.Box.BoxChild w17 = ((Gtk.Box.BoxChild)(w5[w16]));
                w17.Position = 4;
                w17.Expand = false;
                w17.Fill = false;
                bindings["vbox1"] = w5;
                w2.Add(w5);
                Gtk.Box.BoxChild w18 = ((Gtk.Box.BoxChild)(w2[w5]));
                w18.Position = 1;
                bindings["hbox1"] = w2;
                w1.Add(w2);
                Gtk.Box.BoxChild w19 = ((Gtk.Box.BoxChild)(w1[w2]));
                w19.Position = 0;
                bindings["dialog_VBox"] = w1;
                // Internal child MensagemWeb.Windows.ErrorWindow.ActionArea
                Gtk.HButtonBox w20 = cobj.ActionArea;
                w20.LayoutStyle = ((Gtk.ButtonBoxStyle)(4));
                w20.Spacing = 6;
                w20.BorderWidth = ((uint)(12));
                w20.Events = ((Gdk.EventMask)(256));
                w20.Name = "MensagemWeb.ErrorWindow_ActionArea";
                // Container child MensagemWeb.ErrorWindow_ActionArea.Gtk.ButtonBox+ButtonBoxChild
                Gtk.Button w21 = new Gtk.Button();
                w21.UseStock = true;
                w21.UseUnderline = true;
                w21.CanFocus = true;
                w21.Events = ((Gdk.EventMask)(0));
                w21.Name = "button5";
                w21.CanDefault = true;
                w21.Label = "gtk-quit";
                bindings["button5"] = w21;
                cobj.AddActionWidget(w21, -5);
                Gtk.ButtonBox.ButtonBoxChild w22 = ((Gtk.ButtonBox.ButtonBoxChild)(w20[w21]));
                w22.Expand = false;
                w22.Fill = false;
                bindings["MensagemWeb.ErrorWindow_ActionArea"] = w20;
                cobj.DefaultWidth = 500;
                cobj.DefaultHeight = 550;
                bindings["MensagemWeb.Windows.ErrorWindow"] = cobj;
                w3.Show();
                w6.Show();
                w8.Show();
                w10.Show();
                w13.Show();
                w12.Show();
                w16.Show();
                w5.Show();
                w2.Show();
                w1.Show();
                w21.Show();
                w20.Show();
                cobj.Show();
            }
            else {
                if ((id == "MensagemWeb.Windows.QueueWindow")) {
                    Gtk.Window cobj = ((Gtk.Window)(obj));
                    // Widget MensagemWeb.Windows.QueueWindow
                    cobj.Title = "Fila de mensagens";
                    Gtk.UIManager w1 = new Gtk.UIManager();
                    Gtk.ActionGroup w2 = new Gtk.ActionGroup("Default");
                    Gtk.Action w3 = new Gtk.Action("cancelAction", "Interromper", "Deixa de enviar as mensagens selecionadas", "gtk-stop");
                    w3.ShortLabel = "Interromper";
                    w3.IsImportant = true;
                    w3.Sensitive = false;
                    bindings["cancelAction"] = w3;
                    w2.Add(w3, null);
                    Gtk.Action w4 = new Gtk.Action("resendAction", "_Reenviar", "Envia novamente as mensagens selecionadas", "gtk-go-forward");
                    w4.ShortLabel = "Reenviar";
                    w4.IsImportant = true;
                    w4.Sensitive = false;
                    bindings["resendAction"] = w4;
                    w2.Add(w4, null);
                    Gtk.Action w5 = new Gtk.Action("clearAction", "_Limpar", "Retira da lista todos os itens enviados ou com erro", "gtk-clear");
                    w5.ShortLabel = "Limpar";
                    w5.IsImportant = true;
                    bindings["clearAction"] = w5;
                    w2.Add(w5, null);
                    Gtk.Action w6 = new Gtk.Action("deleteAction", null, null, "gtk-delete");
                    w6.Sensitive = false;
                    bindings["deleteAction"] = w6;
                    w2.Add(w6, null);
                    w1.InsertActionGroup(w2, 0);
                    cobj.AddAccelGroup(w1.AccelGroup);
                    cobj.WindowPosition = ((Gtk.WindowPosition)(4));
                    cobj.BorderWidth = ((uint)(12));
                    cobj.Events = ((Gdk.EventMask)(0));
                    cobj.Name = "MensagemWeb.Windows.QueueWindow";
                    // Container child MensagemWeb.Windows.QueueWindow.Gtk.Container+ContainerChild
                    Gtk.VBox w7 = new Gtk.VBox();
                    w7.Spacing = 12;
                    w7.Events = ((Gdk.EventMask)(0));
                    w7.Name = "vbox2";
                    // Container child vbox2.Gtk.Box+BoxChild
                    Gtk.HBox w8 = new Gtk.HBox();
                    w8.Spacing = 12;
                    w8.Events = ((Gdk.EventMask)(0));
                    w8.Name = "progressbox";
                    // Container child progressbox.Gtk.Box+BoxChild
                    Gtk.Image w9 = new Gtk.Image();
                    w9.Pixbuf = Gtk.IconTheme.Default.LoadIcon("gtk-network", 48, 0);
                    w9.Xalign = 0F;
                    w9.Yalign = 0F;
                    w9.Events = ((Gdk.EventMask)(0));
                    w9.Name = "image2";
                    bindings["image2"] = w9;
                    w8.Add(w9);
                    Gtk.Box.BoxChild w10 = ((Gtk.Box.BoxChild)(w8[w9]));
                    w10.Position = 0;
                    w10.Expand = false;
                    w10.Fill = false;
                    // Container child progressbox.Gtk.Box+BoxChild
                    Gtk.VBox w11 = new Gtk.VBox();
                    w11.Spacing = 6;
                    w11.Events = ((Gdk.EventMask)(0));
                    w11.Name = "vbox4";
                    // Container child vbox4.Gtk.Box+BoxChild
                    Gtk.Label w12 = new Gtk.Label();
                    w12.LabelProp = "<span size=\"large\" weight=\"bold\">Nenhuma mensagem a ser enviada</span>";
                    w12.UseMarkup = true;
                    w12.Xalign = 0F;
                    w12.Yalign = 0F;
                    w12.Events = ((Gdk.EventMask)(0));
                    w12.Name = "titleLabel";
                    bindings["titleLabel"] = w12;
                    w11.Add(w12);
                    Gtk.Box.BoxChild w13 = ((Gtk.Box.BoxChild)(w11[w12]));
                    w13.Position = 0;
                    w13.Expand = false;
                    // Container child vbox4.Gtk.Box+BoxChild
                    Gtk.Label w14 = new Gtk.Label();
                    w14.LabelProp = "Atualmente não há nenhuma mensagem a ser enviada. \nVá para a janela principal do MensagemWeb para colocar\numa nova mensagem na fila para ser enviada.";
                    w14.Xalign = 0F;
                    w14.Yalign = 0F;
                    w14.Events = ((Gdk.EventMask)(0));
                    w14.Name = "subLabel";
                    bindings["subLabel"] = w14;
                    w11.Add(w14);
                    Gtk.Box.BoxChild w15 = ((Gtk.Box.BoxChild)(w11[w14]));
                    w15.Position = 1;
                    w15.Expand = false;
                    // Container child vbox4.Gtk.Box+BoxChild
                    Gtk.ProgressBar w16 = new Gtk.ProgressBar();
                    w16.Text = "Foram enviadas 4 de 6 mensagens...";
                    w16.Fraction = 1;
                    w16.Events = ((Gdk.EventMask)(0));
                    w16.Name = "progressbar";
                    bindings["progressbar"] = w16;
                    w11.Add(w16);
                    Gtk.Box.BoxChild w17 = ((Gtk.Box.BoxChild)(w11[w16]));
                    w17.Position = 2;
                    w17.Expand = false;
                    bindings["vbox4"] = w11;
                    w8.Add(w11);
                    Gtk.Box.BoxChild w18 = ((Gtk.Box.BoxChild)(w8[w11]));
                    w18.Position = 1;
                    bindings["progressbox"] = w8;
                    w7.Add(w8);
                    Gtk.Box.BoxChild w19 = ((Gtk.Box.BoxChild)(w7[w8]));
                    w19.Position = 0;
                    w19.Expand = false;
                    w19.Fill = false;
                    // Container child vbox2.Gtk.Box+BoxChild
                    Gtk.HBox w20 = new Gtk.HBox();
                    w20.Spacing = 12;
                    w20.Events = ((Gdk.EventMask)(0));
                    w20.Name = "errorbox";
                    // Container child errorbox.Gtk.Box+BoxChild
                    Gtk.Image w21 = new Gtk.Image();
                    w21.Pixbuf = Gtk.IconTheme.Default.LoadIcon("gtk-dialog-error", 48, 0);
                    w21.Xalign = 0F;
                    w21.Yalign = 0F;
                    w21.Events = ((Gdk.EventMask)(0));
                    w21.Name = "image1";
                    bindings["image1"] = w21;
                    w20.Add(w21);
                    Gtk.Box.BoxChild w22 = ((Gtk.Box.BoxChild)(w20[w21]));
                    w22.Position = 0;
                    w22.Expand = false;
                    w22.Fill = false;
                    // Container child errorbox.Gtk.Box+BoxChild
                    Gtk.VBox w23 = new Gtk.VBox();
                    w23.Spacing = 6;
                    w23.Events = ((Gdk.EventMask)(0));
                    w23.Name = "vbox1";
                    // Container child vbox1.Gtk.Box+BoxChild
                    Gtk.Label w24 = new Gtk.Label();
                    w24.LabelProp = "<span size=\"large\" weight=\"bold\">Houve falhas no envio</span>";
                    w24.UseMarkup = true;
                    w24.Xalign = 0F;
                    w24.Yalign = 0F;
                    w24.Events = ((Gdk.EventMask)(0));
                    w24.Name = "label1";
                    bindings["label1"] = w24;
                    w23.Add(w24);
                    Gtk.Box.BoxChild w25 = ((Gtk.Box.BoxChild)(w23[w24]));
                    w25.Position = 0;
                    w25.Expand = false;
                    w25.Fill = false;
                    // Container child vbox1.Gtk.Box+BoxChild
                    Gtk.Label w26 = new Gtk.Label();
                    w26.LabelProp = "Uma ou mais mensagens não puderam ser\nenviadas e estão marcadas abaixo.";
                    w26.Xalign = 0F;
                    w26.Yalign = 0F;
                    w26.Events = ((Gdk.EventMask)(0));
                    w26.Name = "label2";
                    bindings["label2"] = w26;
                    w23.Add(w26);
                    Gtk.Box.BoxChild w27 = ((Gtk.Box.BoxChild)(w23[w26]));
                    w27.Position = 1;
                    w27.Expand = false;
                    w27.Fill = false;
                    // Container child vbox1.Gtk.Box+BoxChild
                    Gtk.HButtonBox w28 = new Gtk.HButtonBox();
                    w28.LayoutStyle = ((Gtk.ButtonBoxStyle)(3));
                    w28.Events = ((Gdk.EventMask)(0));
                    w28.Name = "hbuttonbox2";
                    // Container child hbuttonbox2.Gtk.ButtonBox+ButtonBoxChild
                    Gtk.Button w29 = new Gtk.Button();
                    w29.CanFocus = true;
                    w29.Events = ((Gdk.EventMask)(0));
                    w29.Name = "errorbutton";
                    // Container child errorbutton.Gtk.Container+ContainerChild
                    Gtk.Alignment w30 = new Gtk.Alignment(0.5F, 0.5F, 0F, 0F);
                    w30.Events = ((Gdk.EventMask)(0));
                    w30.Name = "GtkAlignment";
                    // Container child GtkAlignment.Gtk.Container+ContainerChild
                    Gtk.HBox w31 = new Gtk.HBox();
                    w31.Spacing = 2;
                    w31.Events = ((Gdk.EventMask)(0));
                    w31.Name = "GtkHBox";
                    // Container child GtkHBox.Gtk.Container+ContainerChild
                    Gtk.Image w32 = new Gtk.Image();
                    w32.Pixbuf = Gtk.IconTheme.Default.LoadIcon("gtk-refresh", 16, 0);
                    w32.Events = ((Gdk.EventMask)(0));
                    w32.Name = "image1";
                    bindings["image1"] = w32;
                    w31.Add(w32);
                    // Container child GtkHBox.Gtk.Container+ContainerChild
                    Gtk.Label w34 = new Gtk.Label();
                    w34.LabelProp = "Tentar reenviar as que falharam";
                    w34.Events = ((Gdk.EventMask)(0));
                    w34.Name = "GtkLabel";
                    bindings["GtkLabel"] = w34;
                    w31.Add(w34);
                    bindings["GtkHBox"] = w31;
                    w30.Add(w31);
                    bindings["GtkAlignment"] = w30;
                    w29.Add(w30);
                    bindings["errorbutton"] = w29;
                    w28.Add(w29);
                    Gtk.ButtonBox.ButtonBoxChild w38 = ((Gtk.ButtonBox.ButtonBoxChild)(w28[w29]));
                    w38.Expand = false;
                    w38.Fill = false;
                    bindings["hbuttonbox2"] = w28;
                    w23.Add(w28);
                    Gtk.Box.BoxChild w39 = ((Gtk.Box.BoxChild)(w23[w28]));
                    w39.Position = 2;
                    w39.Expand = false;
                    w39.Fill = false;
                    bindings["vbox1"] = w23;
                    w20.Add(w23);
                    Gtk.Box.BoxChild w40 = ((Gtk.Box.BoxChild)(w20[w23]));
                    w40.Position = 1;
                    bindings["errorbox"] = w20;
                    w7.Add(w20);
                    Gtk.Box.BoxChild w41 = ((Gtk.Box.BoxChild)(w7[w20]));
                    w41.Position = 1;
                    w41.Expand = false;
                    w41.Fill = false;
                    // Container child vbox2.Gtk.Box+BoxChild
                    Gtk.ScrolledWindow w42 = new Gtk.ScrolledWindow();
                    w42.VscrollbarPolicy = ((Gtk.PolicyType)(2));
                    w42.HscrollbarPolicy = ((Gtk.PolicyType)(2));
                    w42.ShadowType = ((Gtk.ShadowType)(1));
                    w42.CanFocus = true;
                    w42.Events = ((Gdk.EventMask)(0));
                    w42.Name = "scrolledwindow1";
                    // Container child scrolledwindow1.Gtk.Container+ContainerChild
                    Gtk.Viewport w43 = new Gtk.Viewport();
                    w43.ShadowType = ((Gtk.ShadowType)(0));
                    w43.Events = ((Gdk.EventMask)(0));
                    w43.Name = "GtkViewport";
                    // Container child GtkViewport.Gtk.Container+ContainerChild
                    Gtk.VBox w44 = new Gtk.VBox();
                    w44.Events = ((Gdk.EventMask)(0));
                    w44.Name = "vbox3";
                    // Container child vbox3.Gtk.Box+BoxChild
                    w1.AddUiFromString("<ui><toolbar name='toolbar1'><toolitem action='resendAction'/><separator/><toolitem action='cancelAction'/><toolitem action='clearAction'/></toolbar></ui>");
                    Gtk.Toolbar w45 = ((Gtk.Toolbar)(w1.GetWidget("/toolbar1")));
                    w45.ShowArrow = false;
                    w45.Tooltips = true;
                    w45.ToolbarStyle = ((Gtk.ToolbarStyle)(3));
                    w45.IconSize = ((Gtk.IconSize)(3));
                    w45.Events = ((Gdk.EventMask)(0));
                    w45.Name = "toolbar1";
                    bindings["toolbar1"] = w45;
                    w44.Add(w45);
                    Gtk.Box.BoxChild w46 = ((Gtk.Box.BoxChild)(w44[w45]));
                    w46.Position = 0;
                    w46.Expand = false;
                    w46.Fill = false;
                    // Container child vbox3.Gtk.Box+BoxChild
                    Gtk.HBox w47 = new Gtk.HBox();
                    w47.Events = ((Gdk.EventMask)(0));
                    w47.Name = "nodepanel";
                    bindings["nodepanel"] = w47;
                    w44.Add(w47);
                    Gtk.Box.BoxChild w48 = ((Gtk.Box.BoxChild)(w44[w47]));
                    w48.Position = 1;
                    bindings["vbox3"] = w44;
                    w43.Add(w44);
                    bindings["GtkViewport"] = w43;
                    w42.Add(w43);
                    bindings["scrolledwindow1"] = w42;
                    w7.Add(w42);
                    Gtk.Box.BoxChild w51 = ((Gtk.Box.BoxChild)(w7[w42]));
                    w51.Position = 2;
                    // Container child vbox2.Gtk.Box+BoxChild
                    Gtk.HBox w52 = new Gtk.HBox();
                    w52.Spacing = 20;
                    w52.Events = ((Gdk.EventMask)(0));
                    w52.Name = "hbox1";
                    // Container child hbox1.Gtk.Box+BoxChild
                    Gtk.CheckButton w53 = new Gtk.CheckButton();
                    w53.Label = "Fechar esta janela após as mensagens serem enviadas";
                    w53.Active = true;
                    w53.DrawIndicator = true;
                    w53.CanFocus = true;
                    w53.Events = ((Gdk.EventMask)(0));
                    w53.Name = "autoclosecheckbutton";
                    bindings["autoclosecheckbutton"] = w53;
                    w52.Add(w53);
                    Gtk.Box.BoxChild w54 = ((Gtk.Box.BoxChild)(w52[w53]));
                    w54.Position = 0;
                    w54.Expand = false;
                    // Container child hbox1.Gtk.Box+BoxChild
                    Gtk.HButtonBox w55 = new Gtk.HButtonBox();
                    w55.LayoutStyle = ((Gtk.ButtonBoxStyle)(4));
                    w55.Spacing = 6;
                    w55.Events = ((Gdk.EventMask)(0));
                    w55.Name = "hbuttonbox1";
                    // Container child hbuttonbox1.Gtk.ButtonBox+ButtonBoxChild
                    Gtk.Button w56 = new Gtk.Button();
                    w56.UseStock = true;
                    w56.UseUnderline = true;
                    w56.CanFocus = true;
                    w56.Events = ((Gdk.EventMask)(0));
                    w56.Name = "closebutton";
                    w56.Label = "gtk-close";
                    bindings["closebutton"] = w56;
                    w55.Add(w56);
                    Gtk.ButtonBox.ButtonBoxChild w57 = ((Gtk.ButtonBox.ButtonBoxChild)(w55[w56]));
                    w57.Expand = false;
                    w57.Fill = false;
                    bindings["hbuttonbox1"] = w55;
                    w52.Add(w55);
                    Gtk.Box.BoxChild w58 = ((Gtk.Box.BoxChild)(w52[w55]));
                    w58.Position = 1;
                    bindings["hbox1"] = w52;
                    w7.Add(w52);
                    Gtk.Box.BoxChild w59 = ((Gtk.Box.BoxChild)(w7[w52]));
                    w59.Position = 3;
                    w59.Expand = false;
                    w59.Fill = false;
                    bindings["vbox2"] = w7;
                    cobj.Add(w7);
                    cobj.DefaultWidth = 519;
                    cobj.DefaultHeight = 391;
                    bindings["MensagemWeb.Windows.QueueWindow"] = cobj;
                    w9.Show();
                    w12.Show();
                    w14.Show();
                    w16.Show();
                    w11.Show();
                    w8.Show();
                    w21.Show();
                    w24.Show();
                    w26.Show();
                    w32.Show();
                    w34.Show();
                    w31.Show();
                    w30.Show();
                    w29.Show();
                    w28.Show();
                    w23.Show();
                    w45.Show();
                    w47.Show();
                    w44.Show();
                    w43.Show();
                    w42.Show();
                    w53.Show();
                    w56.Show();
                    w55.Show();
                    w52.Show();
                    w7.Show();
                    w3.Activated += ((System.EventHandler)(System.Delegate.CreateDelegate(typeof(System.EventHandler), cobj, "CancelClicked")));
                    w4.Activated += ((System.EventHandler)(System.Delegate.CreateDelegate(typeof(System.EventHandler), cobj, "ResendClicked")));
                    w5.Activated += ((System.EventHandler)(System.Delegate.CreateDelegate(typeof(System.EventHandler), cobj, "ClearClicked")));
                    w6.Activated += ((System.EventHandler)(System.Delegate.CreateDelegate(typeof(System.EventHandler), cobj, "DeleteClicked")));
                    w29.Clicked += ((System.EventHandler)(System.Delegate.CreateDelegate(typeof(System.EventHandler), cobj, "ResendWithError")));
                    w56.Clicked += ((System.EventHandler)(System.Delegate.CreateDelegate(typeof(System.EventHandler), cobj, "CloseWindow")));
                }
            }
            System.Reflection.FieldInfo[] fields = obj.GetType().GetFields(((System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic) | System.Reflection.BindingFlags.Instance));
            for (int n = 0; (n < fields.Length); n = (n + 1)) {
                System.Reflection.FieldInfo field = fields[n];
                object widget = bindings[field.Name];
                if (((widget != null) && field.FieldType.IsInstanceOfType(widget))) {
                    field.SetValue(obj, widget);
                }
            }
        }
        
        private static void Initialize() {
            Gtk.IconFactory w1 = new Gtk.IconFactory();
            Gtk.IconSet w2 = new Gtk.IconSet();
            Gtk.IconSource w3 = new Gtk.IconSource();
            w3.Pixbuf = Gdk.Pixbuf.LoadFromResource("icone64.png");
            w2.AddSource(w3);
            Gtk.IconSource w4 = new Gtk.IconSource();
            w4.Pixbuf = Gdk.Pixbuf.LoadFromResource("icone48.png");
            w2.AddSource(w4);
            Gtk.IconSource w5 = new Gtk.IconSource();
            w5.Pixbuf = Gdk.Pixbuf.LoadFromResource("icone16.png");
            w2.AddSource(w5);
            w1.Add("MensagemWeb", w2);
            w1.AddDefault();
        }
    }
    
    internal class ActionGroups {
        
        public static Gtk.ActionGroup GetActionGroup(System.Type type) {
            return Stetic.ActionGroups.GetActionGroup(type.FullName);
        }
        
        public static Gtk.ActionGroup GetActionGroup(string name) {
            return null;
        }
    }
}
