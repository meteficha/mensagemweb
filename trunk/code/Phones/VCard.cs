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

using MensagemWeb.Logging;

namespace MensagemWeb.Phones {
	/// <summary>
	/// Contains useful functions for exporting and importing vCards.
	/// Its name should be "vCard" but is "VCard" to follow name conventions.
	/// </summary>
	public static class VCard {
		/// <summary>
		/// Converts a vCard into a dictionary in the form name:phone.
		/// Does not handle contacts with more than one cellular phone.
		/// Does handle more than one person per vCard.
		/// </summary>
		public static IDictionary<string, Phone> VCardToPhone(string vCards, string defaultDDD) {
			bool inside = false;
			string name = null, phone = null, altphone = null;
			Dictionary<string, Phone> result = new Dictionary<string, Phone>();
			
			foreach (string l in vCards.Split('\n', '\r')) {
				string line = l.Trim();
				if (line.Length == 0)
					continue;
				string uline = line.ToUpper();
				if (!inside) {
					if (uline == "BEGIN:VCARD")
						inside = true;
				} else {
					if (uline.StartsWith("FN"))
						name = line.Substring(line.LastIndexOf(':') + 1).Trim();
					else if (uline.StartsWith("TEL;TYPE=CELL"))
						phone = line.Substring(line.LastIndexOf(':') + 1).Trim();
					else if (uline.StartsWith("TEL"))
						altphone = line.Substring(line.LastIndexOf(':') + 1).Trim();
					else if (uline == "END:VCARD") {
						if (phone == null)
							phone = altphone;
						if (name != null && phone != null) {
							Phone realPhone = null;
							StringBuilder phoneNums = new StringBuilder(15);
							foreach (char c in phone)
								if (Char.IsDigit(c))
									phoneNums.Append(c);
							phone = phoneNums.ToString();
							
							// Remove 9090, if any and not part of the number
							if (phone.Length > 10 && phone.Substring(0, 4) == "9090")
								phone = phone.Substring(4);
								
							// Remove 0 from DDD (061 -> 61), if not part of the number
							if (phone.Length > 10 && phone[0] == '0')
								phone = phone.Substring(1);
							
							// Remove the operator, if not part of the number or the DDD
							if (phone.Length >= 12)
								phone = phone.Substring(2);
							
							// Now, the real tests
							if (phone.Length == 8) {
								// NNNNNNNN
								realPhone = new Phone(defaultDDD, phone);
							} else if (phone.Length == 10) {
								// DDNNNNNNNN
								realPhone = new Phone(phone.Substring(0, 2),
								                                      phone.Substring(2));
							} else {
								// Give up
								Logger.Log(typeof(VCard), "Número não reconhecido: {0}", phone);
								realPhone = null;
							}
							
							if (realPhone != null)
								result[name] = realPhone;
						} else {
							Logger.Log(typeof(VCard), "Ignorando {0} -- {1}", name, phone);
						}
						name = null;
						phone = null;
						altphone = null;
						inside = false;
					} // if (uline == "END:VCARD")
				} // if (!inside)
			} // foreach (string ...
			
			return result;
		}
		
		/// <summary>
		/// Reads a vCard and appends it to the PhoneBook, renaming duplicates.
		/// </summary>
		/// <return>
		/// The list of imported names.
		/// </return>
		public static string[] VCardToPhoneBook(string vcard, string defaultDDD) {
			IDictionary<string, Phone> dict = VCardToPhone(vcard, defaultDDD);
			List<string> names = new List<string>(dict.Count);
			PhoneBook.Hold();
			try {
				foreach (KeyValuePair<string, Phone> kvp in 
						(IEnumerable<KeyValuePair<string, Phone>>) dict) {
					string name = kvp.Key;
					names.Add(name);
					Phone phone = kvp.Value;
					
					PhoneContainer temp;
					temp = PhoneBook.TryGet(name);
					
					if (temp == null) {
						// Saves temp into the PhoneBook
						Logger.Log(typeof(VCard), "Contact {0} imported with success!", name);
						PhoneBook.Add(name, phone, null);
					} else if (temp.Phone == phone) {
						// Nothing to do
						Logger.Log(typeof(VCard), "Contact {0} already exists with same number.", name);
					} else if (PhoneBook.FindName(phone) != null) {
						// Nothing to do as well
						Logger.Log(typeof(VCard), "Phone {0} already exists.", phone);
					} else {
						// Find a non-duplicate name
						string newname = name;
						for (int i = 2; PhoneBook.Contains(newname); i++)
							newname = String.Format("{0} ({1})", name, i);
						Logger.Log(typeof(VCard), "Contact {0} already exists, renaming to {1}.",
								   name, newname);
						PhoneBook.Add(newname, phone, null);
					}
				}
			} finally {
				PhoneBook.Thew();
			}
			return names.ToArray();
		}
				
		/// <summary>
		/// Converts a IDictionary in the form name:phone to a vCard.
		/// </summary>
		public static string PhoneToVCard(IDictionary<string, Phone> phones) {
			StringBuilder result = new StringBuilder();
			foreach (KeyValuePair<string, Phone> kvp in 
					(IEnumerable<KeyValuePair<string, Phone>>) phones)
				result.AppendFormat("BEGIN:VCARD\nFN:{0}\nX-EVOLUTION-FILE-AS:{0}\n" + 
									"TEL;TYPE=CELL:{1}\nEND:VCARD\n\n", kvp.Key, kvp.Value);
			return result.ToString();
		}
		
		/// <summary>
		/// Converts a group of Destination's to a vCard.
		/// </summary>
		public static string DestinationToVCard(string[] destinations) {
			Dictionary<string, Phone> dict = new Dictionary<string, Phone>();
			foreach (string destination in destinations) {
				dict[destination] = PhoneBook.Get(destination).Phone;
			}
			return PhoneToVCard(dict);
		}
		
		/// <summary>
		/// Converts the PhoneBook to a vCard.
		/// </summary>
		public static string PhoneBookToVCard() {
			Dictionary<string, Phone> dict = new Dictionary<string, Phone>();
			foreach (KeyValuePair<string, PhoneContainer> kvp in PhoneBook.NamesPhones) {
				dict[kvp.Key] = kvp.Value.Phone;
			}
			return PhoneToVCard(dict);			
		}
	}
}