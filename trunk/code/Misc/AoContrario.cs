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
using System.Text;

namespace AoContrario {
	class Conversor {
		public static string Converter(string text) {
			// Add a final stop char to make the filter stop and include the last word
			string oldText = text + "!";
			
			StringBuilder newText = new StringBuilder();
			int startIndex = 0, curIndex = 0, length = oldText.Length;
			while (curIndex < length) {
				// Find the next stop character
				if (Char.IsLetter(oldText[curIndex])) {
					// Not now, continue searching
					curIndex++;
					continue;
				}
				
				// We found one, now take the word out of it
				if (curIndex > startIndex) {
					string word = oldText.Substring(startIndex, curIndex - startIndex);
					
					// Invert it
					int wordLength = word.Length;
					string newWord = String.Empty;
					for (int i = 0; i < wordLength; i++) {
						char current = word[i], oposite = word[wordLength - i -1];
						if (Char.IsUpper(oposite))
							newWord = Char.ToUpper(current) + newWord;
						else if (Char.IsLower(oposite))
							newWord = Char.ToLower(current) + newWord;
						else
							newWord = current + newWord;
					}
					
					// Add to the stream
					newText.Append(newWord);
				}
				
				// Now add the stop character
				newText.Append(oldText[curIndex]);
				
				// Go to the next one
				curIndex++;
				startIndex = curIndex;
			}	
			
			// Remove that last stop char
			newText.Remove(newText.Length-1, 1);
			return newText.ToString();
		}
	}
}