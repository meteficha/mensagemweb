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

using MensagemWeb.Phones;

namespace MensagemWeb.Messages {
	public class Message {		
		public readonly IList<string> Destinations;
		public readonly Phone FromPhone;
		public readonly string FromName;
		public readonly string Contents;
		
		public readonly string FromDDD;
		public readonly string FromNumber;
		
		public Phone Destination { get {
			if (Destinations.Count == 1)
				return PhoneBook.Get(Destinations[0]).Phone;
			else
				throw new ArgumentException("More than one destination here");
		} }
		
		public Message(IList<string> destinations, Phone fromPhone, 
				string fromName, string contents) {
				
			// Lots of sanity checks
			contents = contents.Trim();
			if (destinations == null)
				throw new ArgumentNullException("Falta o destinatário");
			if (contents == null || contents.Length < 1)
				throw new ArgumentException("A mensagem não pode estar em branco");
			if (fromName == null)
				throw new ArgumentNullException("O nome do remetente não deve ser nulo");
			if (fromName.Length < 1)
				throw new ArgumentException("O nome do remetente não pode estar em branco");
			if (contents.Length > 7500)
				throw new ArgumentException("Mensagem muito longa para ser enviada");
				
//			// XXX: ICollection`1.Contains is bugged 
//          //      http://bugzilla.ximian.com/show_bug.cgi?id=80563
//			if (destinations.Count < 1 || destinations.Contains(null))
//				throw new ArgumentException("Falta um destinatátio");
			if (destinations.Count < 1)
				throw new ArgumentException("Falta um destinatátio");
			foreach (string d in (IEnumerable<string>)destinations)
				if (d == null) throw new ArgumentException("Falta um destinatátio"); 
			
			
			Destinations = destinations;
			FromName = fromName;
			Contents = contents;
			FromPhone = fromPhone;
			
			if (fromPhone == null) {
				FromNumber = FromDDD = String.Empty;
			} else {
				FromDDD = fromPhone.DDD.ToString();
				FromNumber = fromPhone.Number.ToString();
			}
		}
		
		
		
		public string ToString(string formatString, bool forMarkup) {
			// The sender
			string sender;
			if (FromPhone == null)
				sender = FromName;
			else
				sender = String.Format("{0} [{1}]", FromName, FromPhone);
			
			// The destinations
			System.Text.StringBuilder dests = new System.Text.StringBuilder();
			foreach (string dest in Destinations as IEnumerable<string>) {
				if (dests.Length > 0)
					dests.Append(", ");
				dests.Append(dest);
			}
			
			// Is this going to a markup?
			string contents = Contents;
			if (forMarkup) {
				dests = Util.Replace(dests);
				sender = Util.Replace(sender);
				contents = Util.Replace(contents);
			}
			
			// Format the final string
			return String.Format(formatString, sender, dests.ToString(), contents);
		}
		
		
		
		public override string ToString() {
			return ToString("De: {0}\nPara: {1}\n\n{2}", false);
		}
		
		
		
		public Message ChangeDestination(IList<string> newDests) {
			return new Message(newDests, FromPhone, FromName, Contents);
		}
		
		
		
		public Message ChangeDestination(string newDest) {
			return new Message(new string[] {newDest}, FromPhone, FromName, Contents);
		}
		
		
		
		
		
		private static readonly char[] convertChars = new char[] {
			'á', 'a', 'â', 'a', 'ã', 'a', 'ä', 'a',
			'é', 'e', 'ê', 'e',           'ë', 'e',
			'í', 'i', 'î', 'i', 'ĩ', 'i', 'ï', 'i',
			'ó', 'o', 'ô', 'o', 'õ', 'o', 'ö', 'o',
			'ú', 'u', 'û', 'u', 'ũ', 'u', 'ü', 'u',
			'ç', 'c'};
		public Message WithoutAccentuation() {
			// Convert most common (in pt_BR) non-ASCII chars
			StringBuilder convName = new StringBuilder(FromName);
			StringBuilder convMsg = new StringBuilder(Contents);
			for (int i = 0; i < convertChars.Length; i += 2) {
				// Replace the chars on both strings
				char chr1 = convertChars[i];
				char chr2 = convertChars[i + 1];
				convMsg.Replace(chr1, chr2);
				convName.Replace(chr1, chr2);
				
				// Now the uppercase
				chr1 = Char.ToUpper(chr1);
				chr2 = Char.ToUpper(chr2);
				convMsg.Replace(chr1, chr2);
				convName.Replace(chr1, chr2);
			}
			
			// Create the new message
			return new Message(Destinations, FromPhone, convName.ToString(), convMsg.ToString());
		}
		
		
		
		private int MaxContentsSize() {
			int maxContentsSize = Int32.MaxValue;
			foreach (string dest in Destinations as IEnumerable<string>) {
				int thisMax = PhoneBook.Get(dest).RealEngine.MaxTotalChars;
				if (thisMax < maxContentsSize)
					maxContentsSize = thisMax;
			}
			maxContentsSize -= FromName.Length;
			if (FromPhone != null)
				maxContentsSize -= 10;
			return maxContentsSize;
		} 		
		
		
		// Version of Split() that doesn't create strings or messages
		public int SplitN() {
			int maxContentsSize = MaxContentsSize();
			string[] lines = Contents.Split('\n');
			if (Contents.Length <= maxContentsSize)
				return lines.Length;
			
			// Try to estimate the number of messages to be sent, adding a 5% of error safety.
			double expected = ((double)Contents.Length) / maxContentsSize + (lines.Length - 1);
			expected *= 1.05;
			
			// maxContentsSize will have to be limited even more because of
			// the indications of more than one message
			int max = maxContentsSize;
			if (expected <= 9)
				max -= 4; // "X/Y>"
			else if (expected <= 99)
				max -= 6; // "XX/YY>"
			else
				max -= 8; // "XXX/YYY>"
			
			// Each line is a separate message
			int number = 0;     // Number of messages
			foreach (string line in lines) {
				// Now split the line into words
				string[] words = line.Split(' ');
				
				// Try to make the strings
				int currentLength = 0;
				foreach (string word in words) {
					int wordLength = word.Length;
					if ((currentLength + 1 /* space */ + wordLength) <= max) {
						if (currentLength > 0)
							currentLength += 1;
						currentLength += wordLength;
					} else {
						if (currentLength > 0)
							number++;
						currentLength = wordLength;
						while (currentLength > max) {
							currentLength -= max;
							number++;
						}
					}
				}
				if (currentLength > 0 || words.Length == 0 || 
						(words[0].Length == 0 && words.Length == 1))
					number++;
			}
			
			return number;		
		}
		
		
		
		public Message[] Split() {
			int i;
			int maxContentsSize = MaxContentsSize();
			string[] lines = Contents.Split('\n');
			
			if (lines.Length == 1 && Contents.Length <= maxContentsSize)
				return new Message[] {this};
			
			// maxContentsSize will have to be limited even more because of
			// the indications of more than one message
			double expected = ((double)Contents.Length) / maxContentsSize + (lines.Length - 1);
			expected *= 1.05;
			int max = maxContentsSize;
			if (expected <= 9)
				max -= 4; // "X/Y>"
			else if (expected <= 99)
				max -= 6; // "XX/YY>"
			else
				max -= 8;
			
			// The strings that represent the messages
			List<string> strings = new List<string>((int)Math.Ceiling(expected));
						
			// Each line is a separate message
			StringBuilder current = new StringBuilder(max, max);
			foreach (string line in lines) {
				// Now split the line into words
				string[] words = line.Split(' ');
				
				// Try to make the strings
				foreach (string word in words) {
					int wordLength = word.Length;
					int currentLength = current.Length;
					if ((currentLength + 1 /* space */ + wordLength) <= max) {
						if (currentLength > 0)
							current.Append(" ");
						current.Append(word);
					} else {
						if (currentLength > 0)
							strings.Add(current.ToString());
						current.Remove(0, currentLength);
						if (wordLength <= max)
							current.Append(word);
						else {
							for (i = 0; i < wordLength; i+=max) {
								if ((i + max) <= wordLength)
									strings.Add(word.Substring(i, max));
								else
									current.Append(word.Substring(i, wordLength - i));
							}
						} // if (wordLength <= max)
					} // if ((currentLength + 1 /* space */ + wordLength) <= max)
				} // foreach (string word in words)
				
				if (current.Length > 0 || words.Length == 0 || 
						(words[0].Length == 0 && words.Length == 1))
					strings.Add(current.ToString());
				current.Remove(0, current.Length);
			}// foreach (string line in lines)
			
			// Sanity check
			int total = strings.Count;
			if (total == 1)
				return new Message[] {this};
			
			// Now add the header to everyone and create the messages
			i = 0;
			Message[] result = new Message[total];
			string middle = "/" + total + ">";
			foreach (string str in strings)
				result[i++] = new Message(Destinations, FromPhone, FromName, i + middle + str);
			return result;
		}
	}
}