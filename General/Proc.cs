using UnityEngine;
using System;
using System.Collections.Generic;

using FuncX = System.Func<float, float>;

public static class Proc {
	
#region Primitives
	public static FuncX P2 { get { return x => x *x; } }
	public static FuncX P3 { get { return x => x *x *x; } }
	public static FuncX P4 { get { return x => x *x *x *x; } }
	public static FuncX P5 { get { return x => x *x *x *x *x; } }
	
	public static FuncX PowN( int n ) {
		return (x) => {
			var result = 1f;
			for( var i = 0; i < n; i++ ) {
				result *= x;
			}
			return result;
		};
	}
	public static FuncX PowN( float n ) {
		return (x) => {
			var floor = Mathf.FloorToInt( n );
			var ceil = Mathf.CeilToInt( n );
			var localFraction = Mathf.InverseLerp( floor, ceil, n );
			
			var low = PowN( floor )( x );
			var high = PowN( ceil )( x );
			return Mathf.Lerp( low, high, localFraction );
		};
	}
	
	public static FuncX Flip { get { return x => 1 - x; } }
	
	public static FuncX Arch { get { return x => 4 *x *(1 - x); } }
	
	public static FuncX Abs { get { return Mathf.Abs; } }
	public static FuncX AbsTop { get { return KickFlip( Abs ); } }
	public static FuncX Bounce { get { return ProcExt.AbsTop( Abs ); } }
#endregion
	
	
#region Transformational
	public static FuncX KickFlip( this FuncX action ) {
		return x => Flip( action( Flip( x ) ) );
	}
	
	public static FuncX Scale( this FuncX action ) {
		return x => x *action( x );
	}
	
	public static FuncX ReverseScale( this FuncX action ) {
		return x => Flip( x ) *action( x );
	}
	
	public static FuncX Blend( this FuncX actionA, FuncX actionB, float blend ) {
		return x => Mathf.Lerp( actionA( x ), actionB( x ), blend );
	}
	
	public static FuncX Cross( this FuncX actionA, FuncX actionB ) {
		return x => Mathf.Lerp( actionA( x ), actionB( x ), x );
	}
#endregion
}


public static class ProcExt {
	
#region Utility - combinatorial
	public static FuncX P2( this FuncX action ) {
		return x => Proc.P2( action( x ) );
	}
	public static FuncX P3( this FuncX action ) {
		return x => Proc.P3( action( x ) );
	}
	public static FuncX P4( this FuncX action ) {
		return x => Proc.P4( action( x ) );
	}
	public static FuncX P5( this FuncX action ) {
		return x => Proc.P5( action( x ) );
	}
	public static FuncX PowN( this FuncX action, int n ) {
		return x => Proc.PowN( n )( action( x ) );
	}
	public static FuncX PowN( this FuncX action, float n ) {
		return x => Proc.PowN( n )( action( x ) );
	}
	public static FuncX Flip( this FuncX action ) {
		return x => Proc.Flip( action( x ) );
	}
	public static FuncX Arch( this FuncX action ) {
		return x => Proc.Arch( action( x ) );
	}
	public static FuncX Abs( this FuncX action ) {
		return x => Proc.Abs( action( x ) );
	}
	public static FuncX AbsTop( this FuncX action ) {
		return x => Proc.AbsTop( action( x ) );
	}
	public static FuncX Bounce( this FuncX action ) {
		return x => Proc.Bounce( action( x ) );
	}
#endregion
}

// Attached to floats:
public static class ProcExtFloat {
	
#region Utility - reduced from floats
	public static float P2( this float x ) { return Proc.P2( x ); }
	public static float P3( this float x ) { return Proc.P3( x ); }
	public static float P4( this float x ) { return Proc.P4( x ); }
	public static float P5( this float x ) { return Proc.P5( x ); }
	
	public static float PowN( this float x, int n ) { return Proc.PowN( n )( x ); }
	public static float PowN( this float x, float n ) { return Proc.PowN( n )( x ); }
	
	public static float Flip( this float x ) { return Proc.Flip( x ); }
	
	public static float Arch( this float x ) { return Proc.Arch( x ); }
	
	public static int Abs( this int x ) { return Mathf.Abs( x ); }
	public static float Abs( this float x ) { return Proc.Abs( x ); }
	public static float AbsTop( this float x ) { return Proc.AbsTop( x ); }
	public static float Bounce( this float x ) { return Proc.Bounce( x ); }
	
	// Transformational:
	public static float KickFlip( this float x, FuncX action ) {
		return Proc.KickFlip( action )( x );
	}
	
	public static float Scale( this float x, FuncX action ) {
		return Proc.Scale( action )( x );
	}
	
	public static float ReverseScale( this float x, FuncX action ) {
		return Proc.ReverseScale( action )( x );
	}
	
	public static float Blend( this float x, FuncX actionA, FuncX actionB, float blend ) {
		return Proc.Blend( actionA, actionB, blend )( x );
	}
	
	public static float Cross( this float x, FuncX actionA, FuncX actionB ) {
		return Proc.Cross( actionA, actionB )( x );
	}
	
	// Bonus: convert a float to FuncX
	public static FuncX FN( this float x ) {
		return v => x;
	}
#endregion
}


// Attached to functions:
public static class ProcReductions {
	
#region Utility - reduced from functions
	// Primitives:
	public static float P2( this FuncX action, float x ) {
		return Proc.P2( action( x ) );
	}
	public static float P3( this FuncX action, float x ) {
		return Proc.P3( action( x ) );
	}
	public static float P4( this FuncX action, float x ) {
		return Proc.P4( action( x ) );
	}
	public static float P5( this FuncX action, float x ) {
		return Proc.P5( action( x ) );
	}
	public static float PowN( this FuncX action, int n, float x ) {
		return Proc.PowN( n )( action( x ) );
	}
	public static float PowN( this FuncX action, float n, float x ) {
		return Proc.PowN( n )( action( x ) );
	}
	public static float Flip( this FuncX action, float x ) {
		return Proc.Flip( action( x ) );
	}
	public static float Arch( this FuncX action, float x ) {
		return Proc.Arch( action( x ) );
	}
	public static float Abs( this FuncX action, float x ) {
		return Proc.Abs( action( x ) );
	}
	public static float AbsTop( this FuncX action, float x ) {
		return Proc.AbsTop( action( x ) );
	}
	public static float Bounce( this FuncX action, float x ) {
		return Proc.Bounce( action( x ) );
	}
	
	// Transformational:
	public static float KickFlip( this FuncX action, float x ) {
		return Proc.KickFlip( action )( x );
	}
	
	public static float Scale( this FuncX action, float x ) {
		return Proc.Scale( action )( x );
	}
	
	public static float ReverseScale( this FuncX action, float x ) {
		return Proc.ReverseScale( action )( x );
	}
	
	public static float Blend( this FuncX actionA, FuncX actionB, float blend, float x ) {
		return Proc.Blend( actionA, actionB, blend )( x );
	}
	
	public static float Cross( this FuncX actionA, FuncX actionB, float x ) {
		return Proc.Cross( actionA, actionB )( x );
	}
#endregion
}
