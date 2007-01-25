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
	// WARNING: The only safe methods to use are the ones created or
	// overriden on this class, all others may corrupt the state held
	// on this class.
	public class RichTextBuffer : TextBuffer {
		// Persistent (non-anonymous) TextTags
		private TextTag boldTag;
		private TextTag titleTag;
		private TextTag subtitleTag;
		private TextTag linkTag;
		
		private TextView tv;
		private TextIter endIter;
		
		
		// Creates a RichTextBuffer, a TextView and a ScrolledWindow.
		// The TextView is non-editable and wrap words by default.
		public static ScrolledWindow NewTextView(out RichTextBuffer buffer) {
			TextView tv = new TextView();
			buffer = new RichTextBuffer(tv);
			tv.LeftMargin = 4;
			tv.RightMargin = 4;
			tv.CursorVisible = false;
			tv.Editable = false;
			tv.Buffer = buffer;
			tv.WrapMode = WrapMode.WordChar;
			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = ShadowType.In;
			sw.Add(tv);
			return sw;
		}
		
		
		
		private RichTextBuffer(TextView tv)
				: base(null)
		{
			this.tv = tv;
			
			titleTag = new TextTag("title");
			titleTag.Justification = Justification.Center;
			titleTag.PixelsBelowLines = 15;
			titleTag.Scale = 1.8;
			titleTag.Weight = Pango.Weight.Bold;
			
			subtitleTag = new TextTag("subtitle");
			subtitleTag.PixelsAboveLines = 15;
			subtitleTag.PixelsBelowLines = 8;
			subtitleTag.Scale = 1.2;
			subtitleTag.Weight = Pango.Weight.Bold;
			
			boldTag = new TextTag("bold");
			boldTag.Weight = Pango.Weight.Bold;
			
			linkTag = new TextTag("link");
			linkTag.WrapMode = WrapMode.None;
			linkTag.Foreground = "blue";
			linkTag.Underline = Pango.Underline.Single;
			
			TagTable.Add(boldTag);
			TagTable.Add(titleTag);
			TagTable.Add(subtitleTag);
			TagTable.Add(linkTag);
			
			Clear();
		}
		
		
		
		public new void Clear() {
			base.Clear();
			
			TextIter start;
			this.GetBounds(out start, out endIter);
		}
		
		
		public void AddText(string text) {
			Append(text);
		}
		
		
		public void AddBoldText(string text) {
			Append(text, boldTag);
		}
		
		
		public void AddTitle(string text) {
			Append(text + "\n", titleTag);
		}
		
		
		public void AddSubtitle(string text) {
			Append(text + "\n", subtitleTag);
		}
		
		
		public void AddItem(string text) {
			// TODO Do something better
			TextChildAnchor anchor = this.CreateChildAnchor(ref endIter);
			Alignment align = new Alignment(0.5f, 0.5f, 1, 1);
			align.LeftPadding = 10;
			align.RightPadding = 5;
			align.Add(new Arrow(ArrowType.Right, ShadowType.None));
			align.ShowAll(); // XXX Why is this causing warnings on UpdateWindow?
			tv.AddChildAtAnchor(align, anchor);
			Insert(ref endIter, text);
		}
		
		
		public void AddLink(string text, string link) {
			// TODO Links should have a different cursor
			TextTag thisLinkTag = new TextTag(null);
			string link_ = link;
			thisLinkTag.TextEvent += delegate (object o, TextEventArgs args) {
				if (args.Event.Type == Gdk.EventType.ButtonRelease)
					Util.OpenLink(link_);
			};
			TagTable.Add(thisLinkTag);
			Append(text, linkTag, thisLinkTag);
		}
		
		
		public void AddMensagemWebSite() {
			AddLink("mensagemweb.codigolivre.org.br", "http://mensagemweb.codigolivre.org.br/");
		}
		
		
		private void Append(string text, params TextTag[] tags) {
			if (tags.Length > 0)
				this.InsertWithTags(ref endIter, text, tags);
			else
				this.Insert(ref endIter, text);
		}
	}
}