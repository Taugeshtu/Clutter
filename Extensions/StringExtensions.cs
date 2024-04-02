using System.Collections.Generic;

public static class StringExtensions {
	
#region Trims
	public static void TrimEnd( this System.Text.StringBuilder builder, int count ) {
		var removeStart = builder.Length - count;
		builder.Remove( removeStart, count );
	}
	
	public static string TrimEnd( this string source, int count ) {
		return source.Substring( 0, source.Length - count );
	}
	
	public static string TakeFirst( this string s, int count ) {
		return s.Substring( 0, count.AtMost( s.Length ) );
	}
#endregion
	
	
#region Ops
	public static IEnumerable<int> AllIndicesOf( this string text, string pattern ) {
		return text.AllIndicesOf( pattern, 0, text.Length );
	}
	// Note: calling convention is that "endIndex" is the next index BEYOND the range we're looking in. Could be text.Length
	public static IEnumerable<int> AllIndicesOf( this string text, string pattern, int startIndex, int endIndex ) {
		// source: https://stackoverflow.com/a/62282084
		// Promises O(N+M) complexity
		if( string.IsNullOrEmpty( pattern ) )
			throw new System.ArgumentNullException( nameof( pattern ) );
		return _KMP( text, pattern, startIndex, endIndex );
	}
	
	private static IEnumerable<int> _KMP( string text, string pattern, int startIndex, int endIndex ) {
		var M = pattern.Length;
		var N = endIndex - startIndex;
		
		var lps = _LongestPrefixSuffix( pattern );
		int i = startIndex, j = 0;
		
		while( i < endIndex ) {
			if( pattern[j] == text[i] ) {
				j++;
				i++;
			}
			if( j == M ) {
				yield return i - j;
				j = lps[j - 1];
			}
			else if( i < endIndex && pattern[j] != text[i] ) {
				if( j != 0 )
					j = lps[j - 1];
				else
					i++;
			}
		}
	}
	
	private static int[] _LongestPrefixSuffix( string pattern ) {
		var lps = new int[pattern.Length];
		var length = 0;
		var i = 1;
		
		while( i < pattern.Length ) {
			if( pattern[i] == pattern[length] ) {
				length++;
				lps[i] = length;
				i++;
			}
			else {
				if( length != 0 )
					length = lps[length - 1];
				else {
					lps[i] = length;
					i++;
				}
			}
		}
		return lps;
	}
#endregion
}
