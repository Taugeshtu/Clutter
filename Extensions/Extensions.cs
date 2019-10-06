using UnityEngine;
using System.Collections.Generic;

public static class Extensions {
	
	public static bool GetSide( this Plane plane, Vector3 point, bool invert ) {
		if( invert ) {
			return !plane.GetSide( point );
		}
		else {
			return plane.GetSide( point );
		}
	}
	
	public static void TrimEnd( this System.Text.StringBuilder builder, int count ) {
		var removeStart = builder.Length - count;
		builder.Remove( removeStart, count );
	}
	
#region Temporary
#endregion
}
