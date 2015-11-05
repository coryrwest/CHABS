using System.Collections.Generic;

namespace CRWestropp.Utilities.Helpers {
	public class StringHelper {
		/// <summary>
		/// Will return the index of the next match after the supplied index
		/// </summary>
		/// <param name="text">Text to match on</param>
		/// <param name="startIndex">index to start search at</param>
		/// <param name="match">what to match on</param>
		/// <returns>int of next match after startIndex</returns>
		public static int IndexAfter(string text, int startIndex, string match) {
			if (startIndex == -1) {
				return -1;
			}
			// Cut beginning of text to the startIndex
			text = text.Substring(startIndex);
			// Check for a match, if not exit
			if (text.IndexOf(match) == -1) {
				return -1;
			}
			// Get index of the first match after the startIndex
			int index = text.IndexOf(match) + startIndex;
			return index;
		}

		/// <summary>
		/// Will return the index of the previous match before the supplied index
		/// </summary>
		/// <param name="text">Text to match on</param>
		/// <param name="startIndex">index to start search at</param>
		/// <param name="match">what to match on</param>
		/// <returns>int of previous match before startIndex</returns>
		public static int IndexBefore(string text, int startIndex, string match) {
			if (startIndex == -1) {
				return -1;
			}
			// Cut beginning of text to the startIndex
			text = text.Substring(0, startIndex);
			// Check for a match, if not exit
			if (text.IndexOf(match) == -1) {
				return -1;
			}

			int index = text.LastIndexOf(match);
			return index;
		}

		/// <summary>
		/// Will trim a string to the block of text between the two matches.
		/// Including the text of the match itself.
		/// </summary>
		/// <param name="startMatch"></param>
		/// <param name="endMatch"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public string TrimStringToBlock(string startMatch, string endMatch, string text, bool includeMatch = true) {
			int startIndex = 0;
			int endIndex = 0;
			if (includeMatch) {
				startIndex = text.IndexOf(startMatch);
				endIndex = IndexAfter(text, startIndex, endMatch);
			}
			else {
				startIndex = text.IndexOf(startMatch) + startMatch.Length;
				endIndex = IndexAfter(text, startIndex, endMatch) - endMatch.Length;
			}
			// Check for existing endIndex
			if (endIndex == -1) {
				return null;
			}
			// Get substring of text block plus the length of the match
			text = text.Substring(startIndex, endIndex - startIndex + endMatch.Length);
			return text;
		}

		/// <summary>
		/// Finds all matches in the string provided and returns a list. Optionally include the match itself.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="startMatch"></param>
		/// <param name="endMatch"></param>
		/// <param name="includeMatch">include the match text in the returned value.</param>
		/// <returns></returns>
		public List<string> FindAllStringMatches(string text, string startMatch, string endMatch = "", bool includeMatch = true) {
			string tempText = "";
			List<string> results = new List<string>();

			while (text.IndexOf(startMatch) != -1) {
				if (string.IsNullOrEmpty(endMatch)) {
					tempText = TrimStringToBlock(startMatch, startMatch, text);
					int startIndex = text.IndexOf(startMatch) + tempText.Length;
					text = text.Substring(startIndex);
				}
				else {
					string replaceText = TrimStringToBlock(startMatch, endMatch, text);
					if (includeMatch) {
						tempText = TrimStringToBlock(startMatch, endMatch, text);
					}
					else {
						tempText = TrimStringToBlock(startMatch, endMatch, text, false);
					}
					text = text.Replace(replaceText, "");
				}
				results.Add(tempText);
			}
			return results;
		}

		/// <summary>
		/// Finds a match in the string provided and returns the string from the match to the end. 
		/// Optionally include the match itself. Otionally include an end match and return everything between.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="startMatch"></param>
		/// <param name="endMatch"></param>
		/// <param name="includeMatch">include the match text in the returned value.</param>
		/// <returns></returns>
		public string FindStringMatch(string text, string startMatch, string endMatch = "", bool includeMatch = true) {
			string tempText = "";
			string results = "";

			if (string.IsNullOrEmpty(endMatch)) {
				tempText = TrimStringToBlock(startMatch, startMatch, text);
				int startIndex = text.IndexOf(startMatch) + tempText.Length;
				results = text.Substring(startIndex);
			}
			else {
				string replaceText = TrimStringToBlock(startMatch, endMatch, text);
				if (includeMatch) {
					tempText = TrimStringToBlock(startMatch, endMatch, text);
				}
				else {
					tempText = TrimStringToBlock(startMatch, endMatch, text, false);
				}
				text = text.Replace(replaceText, "");
			}
			results = tempText;

			return results;
		}
	}
}
