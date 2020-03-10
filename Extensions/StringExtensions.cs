using System.Collections.Generic;

public static class StringExtensions {
	
#region Trims
	public static void TrimEnd( this System.Text.StringBuilder builder, int count ) {
		var removeStart = builder.Length - count;
		builder.Remove( removeStart, count );
	}
#endregion
	
	
#region Temporary
#endregion
}
