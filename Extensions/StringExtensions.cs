using System.Collections.Generic;

public static class StringExtensions {
	
#region Trims
	public static void TrimEnd( this System.Text.StringBuilder builder, int count ) {
		var removeStart = builder.Length - count;
		builder.Remove( removeStart, count );
	}
	
	public static string TakeFirst( this string s, int count ) {
		return s.Substring( 0, count.AtMost( s.Length ) );
	}
#endregion
	
	
#region Temporary
#endregion
}
