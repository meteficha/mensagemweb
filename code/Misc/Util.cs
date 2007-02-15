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

using MensagemWeb.Engines;
using MensagemWeb.Logging;
using MensagemWeb.Phones;

#if !LITE
using Gtk;
using MensagemWeb.Windows;
#endif
	
namespace MensagemWeb {
	public static class Util {
		private static Encoding encoding;
		static Util() {
			try {
				encoding = Encoding.GetEncoding("Latin1");
			} catch (NotSupportedException) {
				encoding = Encoding.ASCII;
			}			
		}
		
		
		private static readonly string[] masc = {"nenhum", "um", "dois", "três", "quatro", "cinco",
												 "seis", "sete", "oito", "nove", "dez", "onze",
												 "doze", "treze", "quatorze", "quinze", "dezesseis",
												 "dezessete", "dezoito", "dezenove", "vinte"};
		private static readonly string[] fem =  {"nenhuma", "uma", "duas"};		
	
		// e.g. Number(1, true) == "um", while Number(2, false) == "duas"
		public static string Number(int asInt, bool masculine) {
			if (asInt >= masc.Length || asInt < 0)
				return asInt.ToString();
			else if (!masculine && asInt < fem.Length)
				return fem[asInt];
			else
				return masc[asInt];
		}
		
		
		
		public static string ToPrettyString(DateTime date) {
			string time = date.ToShortTimeString();
			switch (DateTime.Now.Date.Subtract(date.Date).Days) {
				case 0:
					// Today
					return "Hoje às " + time;
				case 1:
					// Yesterday
					return "Ontem às " + time;
				default:
					// Any other day (including in the future -- there are bizarre wall clocks)
					return "Em " + date.ToShortDateString() + " às " + time;
			}
		}
		
		
		
		// Strings
		
		public static byte[] ToBytes(string str) {
			return encoding.GetBytes(str);
		}
		
		public static string ToSecureASCII(string str) {
			byte[] temp = encoding.GetBytes(str);
			StringBuilder result = new StringBuilder(temp.Length * 3);
			foreach (byte b in temp) {
				// Put into the form %XX (for example, %20 for " ")
				result.AppendFormat("%{0}", b.ToString("x2"));
			}
			return result.ToString();
		}
		
		public static string Split(string str, int maxchars) {
			StringBuilder result = new StringBuilder(str.Length * 2);
			int chars = 0;
			foreach (string word in str.Split(' ', '\n')) {
				int wordLength = word.Length;
				if (wordLength == 0)
					continue;
				else if (chars > 0 && (chars + 1 + wordLength) > maxchars) {
					result.Append('\n');
					result.Append(word);
					chars = wordLength;
				} else {
					if (chars > 0) {
						result.Append(' ');
						chars++;
					}
					result.Append(word);
					chars += wordLength;
				}
			}
			return result.ToString();
		}
		
		
#if !LITE		
		// MessageDialogs
		
		public static MessageDialog CreateMessageDialog(Window parent, DialogFlags flags,
					MessageType type, ButtonsType bt, bool use_markup, string title, string text)
		{	
			MessageDialog d;
			string formatted_title = String.Format(
				"<span size=\"large\" weight=\"bold\">{0}</span>", title);
			if (md_SecondaryUseMarkup != null) {
				d = new MessageDialog(parent, flags, type, bt, use_markup, title);
				md_Text.SetValue(d, formatted_title, null); 
				md_UseMarkup.SetValue(d, true, null);
				md_SecondaryText.SetValue(d, text, null);
				md_SecondaryUseMarkup.SetValue(d, use_markup, null);
			} else {
				string msg = formatted_title + "\n\n" + text;
				d = new MessageDialog(parent, flags, type, bt, true, msg);
			}
			return d;
		}
		private static readonly PropertyInfo md_Text =
			typeof(Gtk.MessageDialog).GetProperty("Text");
		private static readonly PropertyInfo md_UseMarkup =
			typeof(Gtk.MessageDialog).GetProperty("UseMarkup");
		private static readonly PropertyInfo md_SecondaryText = 
			typeof(Gtk.MessageDialog).GetProperty("SecondaryText");
		private static readonly PropertyInfo md_SecondaryUseMarkup =
			typeof(Gtk.MessageDialog).GetProperty("SecondaryUseMarkup");
		
		
		// Shows a message with a close button.
		public static void ShowMessage(Window parent, string title, string msg,
									   MessageType type, bool replace) {
			if (replace) {
				title = Replace(title);
				msg = Replace(msg);
			}
			MessageDialog md = CreateMessageDialog(parent, DialogFlags.DestroyWithParent, type,
												   ButtonsType.Close, true, title, msg);
			md.Run();
			md.Destroy();
		}
		public static void ShowMessage(string title, string msg, MessageType type, bool replace) {
			ShowMessage(MainWindow.This, title, msg, type, replace);
		}
		
		
		
		
		// This dictionary is used to cache the last color of the widget
		// to save time (P/invoke is expensive)
		private static Dictionary<Widget, int> colorCache = new Dictionary<Widget, int>();
		
		public static void SetColorNormal(Gtk.Widget wd) {
			if (colorCache.ContainsKey(wd)) {
				if (colorCache[wd] == 0)
					return;
				else
					wd.ModifyBase(Gtk.StateType.Normal);
			}
			colorCache[wd] = 0;
		}
		
		private static readonly Gdk.Color redColor = new Gdk.Color(0xFF, 0xEE, 0xEE);
		public static void SetColorRed(Gtk.Widget wd) {
			if (colorCache.ContainsKey(wd) && colorCache[wd] == 1)
				return;
			
			wd.ModifyBase(Gtk.StateType.Normal, redColor);
			colorCache[wd] = 1;
		}
		
		public static void SetColor(Gtk.Widget wd, bool normal) {
			if (normal)
				SetColorNormal(wd);
			else
				SetColorRed(wd);
		}
		
		
		public static readonly Assembly GtkSharp = Assembly.GetAssembly(typeof(Gtk.Button));
		

		// Convinience functions used above
		public static string Replace(string str) {
			return str.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
		}
		public static StringBuilder Replace(StringBuilder str) {
			return str.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
		}

#endif		
		
		
		// Writes a tag like <name>value_</name> in the XML writer.
		public static void WriteProperty(System.Xml.XmlWriter writer, string name, string value_) {
			writer.WriteStartElement(name);
			writer.WriteString(value_);
			writer.WriteEndElement();
		}
		
		// Writes a tag like <name attrkey="attrvalue">value_</name> 
		public static void WriteProperty(System.Xml.XmlWriter writer, 
				string name, string value_, IEnumerable<KeyValuePair<string, string>> attrs) {
			writer.WriteStartElement(name);
			foreach (KeyValuePair<string, string> kvp in attrs)
				writer.WriteAttributeString(kvp.Key, kvp.Value);
			writer.WriteString(value_);
			writer.WriteEndElement();
		}
		
		
		
		public static void OpenLink(string address) {
			new System.Threading.Thread(delegate () {
				Logger.Log(typeof(Util), "Trying to open link \"{0}\"...", address);
				try {
					if (OnWindows) {
						// Use Microsoft's way of opening sites
						System.Diagnostics.Process.Start(address);
					} else {
						// We're on Unix, try gnome-open, then open, then Firefox browser
						string cmdline = String.Format("gnome-open {0} || open {0} || "+
							"firefox {0} || mozilla-firefox {0}", address);
						System.Diagnostics.Process.Start(cmdline);
					}
				} catch (Exception e) {
					Logger.Log(typeof(Util), e.ToString());
				}
			}).Start();
		}
		
		
		
		// Sets a property on an object when it provides it, otherwise fails silently.
		public static bool SetProperty(object obj, string prop, object value_) {
			return SetProperty(obj, prop, value_, null);
		}
		
		public static bool SetProperty(object obj, string prop, object value_, object[] index) {
			PropertyInfo info = obj.GetType().GetProperty(prop);
			if (info != null && info.CanWrite)
				try {
					info.SetValue(obj, value_, index);
					return true;
				} catch (ArgumentException) {
					// Pass
				} catch (System.Reflection.TargetException) {
					// Pass
				} catch (System.Reflection.TargetInvocationException) {
					// Pass
				} catch (System.Reflection.TargetParameterCountException) {
					// Pass
				} 
			return false;
		}
		
				
		// Gets a property from an object when it provides it, otherwise returns the default value.
		
		public static T GetProperty<T>(object obj, string prop, T default_) {
			return GetProperty<T>(obj, prop, default_, null);
		}
		
		public static T GetProperty<T>(object obj, string prop) {
			return GetProperty<T>(obj, prop, default(T), null);
		}
		
		public static T GetProperty<T>(object obj, string prop, T default_, object[] index) {
			PropertyInfo info = obj.GetType().GetProperty(prop);
			if (info != null && info.CanRead) 
				try {
					return (T) info.GetValue(obj, index);
				} catch (ArgumentException) {
					// Pass
				} catch (InvalidCastException) {
					// Pass
				} catch (System.Reflection.TargetException) {
					// Pass
				} catch (System.Reflection.TargetInvocationException) {
					// Pass
				} catch (System.Reflection.TargetParameterCountException) {
					// Pass
				} 
			return default_;
		}
		
		
		public static readonly bool OnWindows = Environment.OSVersion.Platform != PlatformID.Unix;
	}
}