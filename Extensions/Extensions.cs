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
	
#region Temporary
#endregion
}
