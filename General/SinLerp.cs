using UnityEngine;
using System.Collections.Generic;

public enum SinShape {
	CRising,		// [0 -> 1] First quarter of Sin()
	CFalling,		// [1 -> 0] Second quarter
	JRising,		// [0 -> 1] Fourth quater + 1
	JFalling,		// [1 -> 0] Third quarter + 1
	LinearRising,	// [0 -> 1] linear
	LinearFalling,	// [1 -> 0] linear
	SRising,		// [0 -> 1] S-curve, -pi/2, pi/2
	SFalling,		// [1 -> 0] S-curve, pi/2, pi/2 + pi
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
		if( shape == SinShape.LinearRising ) {
			return Mathf.Lerp( min, max, factor );
		}
		if( shape == SinShape.LinearFalling ) {
			return Mathf.Lerp( min, max, 1f - factor );
		}
		
		var shift = 0f;
		var lift = 0f;
		var period = c_halfPI;
		var scale = 1f;
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
			case SinShape.SRising:
				shift = -c_halfPI;
				period = Mathf.PI;
				lift = 1;
				scale = 0.5f;
				break;
			case SinShape.SFalling:
				shift = c_halfPI;
				period = Mathf.PI;
				lift = 1;
				scale = 0.5f;
				break;
		}
		
		var result01 = (lift + Mathf.Sin( shift + factor *period )) *scale;
		return Mathf.Lerp( min, max, result01 );
	}
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}
