using UnityEngine;
using System.Collections.Generic;

public enum SinShape {
	CRising,	// [0 -> 1] First quarter of Sin()
	CFalling,	// [1 -> 0] Second quarter
	JRising,	// [0 -> 1] Fourth quater + 1
	JFalling	// [1 -> 0] Third quarter + 1
}

public static class Sin {
	private const float c_halfPI = Mathf.PI *0.5f;
	
#region Implementation
#endregion
	
	
#region Public
	public static float Lerp( float factor, SinShape shape ) {
		return Lerp( 0, 1, factor, shape );
	}
	
	public static float Lerp( float min, float max, float factor, SinShape shape ) {
		var shift = 0f;
		var lift = 0f;
		switch( shape ) {
			case SinShape.CFalling:
				shift = c_halfPI;
				break;
			case SinShape.JRising:
				shift = 3 *c_halfPI;
				lift = 1f;
				break;
			case SinShape.JFalling:
				shift = 2 *c_halfPI;
				lift = 1f;
				break;
		}
		
		var result01 = lift + Mathf.Sin( shift + factor *c_halfPI );
		return Mathf.Lerp( min, max, result01 );
	}
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}
