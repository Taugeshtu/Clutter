﻿using UnityEngine;
using System.Collections.Generic;

//===========================================================//===========================================================
public static class Vector {
#region Invalids
	// Prefabs:
	public static Vector2 Invalid2 {
		get {
			return new Vector2( float.NaN, float.NaN );
		}
	}
	
	public static Vector3 Invalid3 {
		get {
			return new Vector3( float.NaN, float.NaN, float.NaN );
		}
	}
	
	public static Vector4 Invalid4 {
		get {
			return new Vector4( float.NaN, float.NaN, float.NaN, float.NaN );
		}
	}
	
	// VectorInt-s
	public static Vector2Int Invalid2Int {
		get {
			return new Vector2Int( int.MinValue, int.MinValue );
		}
	}
	
	public static Vector3Int Invalid3Int {
		get {
			return new Vector3Int( int.MinValue, int.MinValue, int.MinValue );
		}
	}
	
	// I know it's not ideal, to be in Vector, but alright
	public static Plane InvalidPlane {
		get {
			return new Plane( Invalid3, Invalid3 );
		}
	}
#endregion
	
	
#region Whatever
	//===========================================================
	public static Vector2 Min( Vector2 a, Vector2 b ) {
		var x = Mathf.Min( a.x, b.x );
		var y = Mathf.Min( a.y, b.y );
		return new Vector2( x, y );
	}
	public static Vector3 Min( Vector3 a, Vector3 b ) {
		var x = Mathf.Min( a.x, b.x );
		var y = Mathf.Min( a.y, b.y );
		var z = Mathf.Min( a.z, b.z );
		return new Vector3( x, y, z );
	}
	public static Vector4 Min( Vector4 a, Vector4 b ) {
		var x = Mathf.Min( a.x, b.x );
		var y = Mathf.Min( a.y, b.y );
		var z = Mathf.Min( a.z, b.z );
		var w = Mathf.Min( a.w, b.w );
		return new Vector4( x, y, z, w );
	}
	public static Vector2Int Min( Vector2Int a, Vector2Int b ) {
		var x = Mathf.Min( a.x, b.x );
		var y = Mathf.Min( a.y, b.y );
		return new Vector2Int( x, y );
	}
	public static Vector3Int Min( Vector3Int a, Vector3Int b ) {
		var x = Mathf.Min( a.x, b.x );
		var y = Mathf.Min( a.y, b.y );
		var z = Mathf.Min( a.z, b.z );
		return new Vector3Int( x, y, z );
	}
	
	public static Vector2 Max( Vector2 a, Vector2 b ) {
		var x = Mathf.Max( a.x, b.x );
		var y = Mathf.Max( a.y, b.y );
		return new Vector2( x, y );
	}
	public static Vector3 Max( Vector3 a, Vector3 b ) {
		var x = Mathf.Max( a.x, b.x );
		var y = Mathf.Max( a.y, b.y );
		var z = Mathf.Max( a.z, b.z );
		return new Vector3( x, y, z );
	}
	public static Vector4 Max( Vector4 a, Vector4 b ) {
		var x = Mathf.Max( a.x, b.x );
		var y = Mathf.Max( a.y, b.y );
		var z = Mathf.Max( a.z, b.z );
		var w = Mathf.Max( a.w, b.w );
		return new Vector4( x, y, z, w );
	}
	public static Vector2Int Max( Vector2Int a, Vector2Int b ) {
		var x = Mathf.Max( a.x, b.x );
		var y = Mathf.Max( a.y, b.y );
		return new Vector2Int( x, y );
	}
	public static Vector3Int Max( Vector3Int a, Vector3Int b ) {
		var x = Mathf.Max( a.x, b.x );
		var y = Mathf.Max( a.y, b.y );
		var z = Mathf.Max( a.z, b.z );
		return new Vector3Int( x, y, z );
	}
	
	// https://en.wikipedia.org/wiki/Determinant
	// This is a fast method to determine on which side of "zero, a" plane does "b" fall
	// negative for clockwise side, positive for counter-clockwise side, 0 for on the plane
	// so if we have points A, B, C
	// and feed a == (B - A); b = (C - A)
	// negative result will tell us that triangle is wound clockwise
	public static float Determinant( Vector2 a, Vector2 b ) {
		return (a.x *b.y - a.y *b.x);
	}
#endregion
}

public static class VectorExtensions {
	
#region Vector2 converters
	//===========================================================
	public static Vector2 ToVector2( this (float x, float y) v ) { return new Vector2( v.x, v.y ); }
	public static (float x, float y) ToTuple( this Vector2 v ) { return (v.x, v.y); }
	
	public static Vector3 XY0( this Vector2 v ) { return new Vector3( v.x, v.y, 0 ); }
	public static Vector3 X0Y( this Vector2 v ) { return new Vector3( v.x, 0, v.y ); }
#endregion
	
	
#region Vector3 converters
	//===========================================================
	public static Vector3 ToVector3( this (float x, float y, float z) v ) { return new Vector3( v.x, v.y, v.z ); }
	public static (float x, float y, float z) ToTuple( this Vector3 v ) { return (v.x, v.y, v.z); }
	
	public static Vector2 XY( this Vector3 v ) { return new Vector2( v.x, v.y ); }
	public static Vector2 XZ( this Vector3 v ) { return new Vector2( v.x, v.z ); }
	public static Vector2 YX( this Vector3 v ) { return new Vector2( v.y, v.x ); }
	public static Vector2 YZ( this Vector3 v ) { return new Vector2( v.y, v.z ); }
	public static Vector2 ZX( this Vector3 v ) { return new Vector2( v.z, v.x ); }
	public static Vector2 ZY( this Vector3 v ) { return new Vector2( v.z, v.y ); }
#endregion
	
	
#region Vector4 converters
	//===========================================================
	public static Vector4 ToVector4( this (float x, float y, float z, float w) v ) { return new Vector4( v.x, v.y, v.z, v.w ); }
	public static (float x, float y, float z, float w) ToTuple( this Vector4 v ) { return (v.x, v.y, v.z, v.w); }
	
	public static Vector3 XYZ( this Vector4 v ) { return new Vector3( v.x, v.y, v.z ); }
	public static Vector2 XY( this Vector4 v ) { return new Vector2( v.x, v.y ); }
	public static Vector2 ZW( this Vector4 v ) { return new Vector2( v.z, v.w ); }
#endregion
	
	
#region Color converters
	//===========================================================
	public static Color ToColor( this Vector4 v ) { return new Color( v.x, v.y, v.z, v.w ); }
	public static Vector4 ToVector4( this Color c ) { return new Vector4( c.r, c.g, c.b, c.a ); }
#endregion
	
	
#region Vector2Int converters
	//===========================================================
	public static Vector2 ToVector2( this Vector2Int v ) { return new Vector2( v.x, v.y ); }
	public static Vector2Int ToVector2Int( this Vector2 v ) { return new Vector2Int( Mathf.RoundToInt( v.x ), Mathf.RoundToInt( v.y ) ); }
	public static Vector2Int ToVector2Int( this (int x, int y) v ) { return new Vector2Int( v.x, v.y ); }
	public static Vector2Int ToVector2Int( this (float x, float y) v ) { return new Vector2Int( Mathf.RoundToInt( v.x ), Mathf.RoundToInt( v.y ) ); }
	public static (int x, int y) ToTuple( this Vector2Int v ) { return (v.x, v.y); }
	
	public static Vector3Int XY0( this Vector2Int v ) { return new Vector3Int( v.x, v.y, 0 ); }
	public static Vector3Int X0Y( this Vector2Int v ) { return new Vector3Int( v.x, 0, v.y ); }
#endregion
	
	
#region Vector3Int converters
	//===========================================================
	public static Vector3 ToVector3( this Vector3Int v ) { return new Vector3( v.x, v.y, v.z ); }
	public static Vector3Int ToVector3Int( this Vector3 v ) { return new Vector3Int( Mathf.RoundToInt( v.x ), Mathf.RoundToInt( v.y ), Mathf.RoundToInt( v.z ) ); }
	public static Vector3Int ToVector3Int( this (int x, int y, int z) v ) { return new Vector3Int( v.x, v.y, v.z ); }
	public static Vector3Int ToVector3Int( this (float x, float y, float z) v ) { return new Vector3Int( Mathf.RoundToInt( v.x ), Mathf.RoundToInt( v.y ), Mathf.RoundToInt( v.z ) ); }
	public static (int x, int y, int z) ToTuple( this Vector3Int v ) { return (v.x, v.y, v.z); }
	
	public static Vector2Int XY( this Vector3Int v ) { return new Vector2Int( v.x, v.y ); }
	public static Vector2Int XZ( this Vector3Int v ) { return new Vector2Int( v.x, v.z ); }
	public static Vector2Int YX( this Vector3Int v ) { return new Vector2Int( v.y, v.x ); }
	public static Vector2Int YZ( this Vector3Int v ) { return new Vector2Int( v.y, v.z ); }
	public static Vector2Int ZX( this Vector3Int v ) { return new Vector2Int( v.z, v.x ); }
	public static Vector2Int ZY( this Vector3Int v ) { return new Vector2Int( v.z, v.y ); }
#endregion
	
	
#region Vector2 component reordering
	//===========================================================
	public static Vector2 YX( this Vector2 v ) { return new Vector2( v.y, v.x ); }
#endregion
	
	
#region Vector3 component reordering
	//===========================================================
	public static Vector3 XZY( this Vector3 v ) { return new Vector3( v.x, v.z, v.y ); }
	public static Vector3 YXZ( this Vector3 v ) { return new Vector3( v.y, v.x, v.z ); }
	public static Vector3 YZX( this Vector3 v ) { return new Vector3( v.y, v.z, v.x ); }
	public static Vector3 ZXY( this Vector3 v ) { return new Vector3( v.z, v.x, v.y ); }
	public static Vector3 ZYX( this Vector3 v ) { return new Vector3( v.z, v.y, v.x ); }
#endregion
	
	
#region Vector4 component reordering
	//===========================================================
	public static Vector4 XYZW( this Vector4 v ) { return new Vector4( v.x, v.y, v.z, v.w ); }
	public static Vector4 XYWZ( this Vector4 v ) { return new Vector4( v.x, v.y, v.w, v.z ); }
	public static Vector4 XZYW( this Vector4 v ) { return new Vector4( v.x, v.z, v.y, v.w ); }
	public static Vector4 XZWY( this Vector4 v ) { return new Vector4( v.x, v.z, v.w, v.y ); }
	public static Vector4 XWYZ( this Vector4 v ) { return new Vector4( v.x, v.w, v.y, v.z ); }
	public static Vector4 XWZY( this Vector4 v ) { return new Vector4( v.x, v.w, v.z, v.y ); }
	
	public static Vector4 YXZW( this Vector4 v ) { return new Vector4( v.y, v.x, v.z, v.w ); }
	public static Vector4 YXWZ( this Vector4 v ) { return new Vector4( v.y, v.x, v.w, v.z ); }
	public static Vector4 YZXW( this Vector4 v ) { return new Vector4( v.y, v.z, v.x, v.w ); }
	public static Vector4 YZWX( this Vector4 v ) { return new Vector4( v.y, v.z, v.w, v.x ); }
	public static Vector4 YWXZ( this Vector4 v ) { return new Vector4( v.y, v.w, v.x, v.z ); }
	public static Vector4 YWZX( this Vector4 v ) { return new Vector4( v.y, v.w, v.z, v.x ); }
	
	public static Vector4 ZXYW( this Vector4 v ) { return new Vector4( v.z, v.x, v.y, v.w ); }
	public static Vector4 ZXWY( this Vector4 v ) { return new Vector4( v.z, v.x, v.w, v.y ); }
	public static Vector4 ZYXW( this Vector4 v ) { return new Vector4( v.z, v.y, v.x, v.w ); }
	public static Vector4 ZYWX( this Vector4 v ) { return new Vector4( v.z, v.y, v.w, v.x ); }
	public static Vector4 ZWXY( this Vector4 v ) { return new Vector4( v.z, v.w, v.x, v.y ); }
	public static Vector4 ZWYX( this Vector4 v ) { return new Vector4( v.z, v.w, v.y, v.x ); }
	
	public static Vector4 WXYZ( this Vector4 v ) { return new Vector4( v.w, v.x, v.y, v.z ); }
	public static Vector4 WXZY( this Vector4 v ) { return new Vector4( v.w, v.x, v.z, v.y ); }
	public static Vector4 WYXZ( this Vector4 v ) { return new Vector4( v.w, v.y, v.x, v.z ); }
	public static Vector4 WYZX( this Vector4 v ) { return new Vector4( v.w, v.y, v.z, v.x ); }
	public static Vector4 WZXY( this Vector4 v ) { return new Vector4( v.w, v.z, v.x, v.y ); }
	public static Vector4 WZYX( this Vector4 v ) { return new Vector4( v.w, v.z, v.y, v.x ); }
#endregion
	
	
#region Vector2Int component reordering
	//===========================================================
	public static Vector2Int YX( this Vector2Int v ) { return new Vector2Int( v.y, v.x ); }
#endregion
	
	
#region Vector3Int component reordering
	//===========================================================
	public static Vector3Int XZY( this Vector3Int v ) { return new Vector3Int( v.x, v.z, v.y ); }
	public static Vector3Int YXZ( this Vector3Int v ) { return new Vector3Int( v.y, v.x, v.z ); }
	public static Vector3Int YZX( this Vector3Int v ) { return new Vector3Int( v.y, v.z, v.x ); }
	public static Vector3Int ZXY( this Vector3Int v ) { return new Vector3Int( v.z, v.x, v.y ); }
	public static Vector3Int ZYX( this Vector3Int v ) { return new Vector3Int( v.z, v.y, v.x ); }
#endregion
	
	
#region Component access
	//===========================================================
	public static float[] Components( this Vector2 v ) { return new float[] {v.x, v.y}; }
	public static float[] Components( this Vector3 v ) { return new float[] {v.x, v.y, v.z}; }
	public static float[] Components( this Vector4 v ) { return new float[] {v.x, v.y, v.z, v.w}; }
	
	public static int[] Components( this Vector2Int v ) { return new int[] {v.x, v.y}; }
	public static int[] Components( this Vector3Int v ) { return new int[] {v.x, v.y, v.z}; }
	
	//===========================================================
	public static float MinComponent( this Vector2 v ) { return Mathf.Min( v.x, v.y ); }
	public static float MinComponent( this Vector3 v ) { return Mathf.Min( v.x, v.y, v.z ); }
	public static float MinComponent( this Vector4 v ) { return Mathf.Min( v.x, v.y, v.z, v.w ); }
	
	//===========================================================
	public static float MaxComponent( this Vector2 v ) { return Mathf.Max( v.x, v.y ); }
	public static float MaxComponent( this Vector3 v ) { return Mathf.Max( v.x, v.y, v.z ); }
	public static float MaxComponent( this Vector4 v ) { return Mathf.Max( v.x, v.y, v.z, v.w ); }
	
	//===========================================================
	public static float MinComponent( this Vector2Int v ) { return Mathf.Min( v.x, v.y ); }
	public static float MinComponent( this Vector3Int v ) { return Mathf.Min( v.x, v.y, v.z ); }
	
	//===========================================================
	public static float MaxComponent( this Vector2Int v ) { return Mathf.Max( v.x, v.y ); }
	public static float MaxComponent( this Vector3Int v ) { return Mathf.Max( v.x, v.y, v.z ); }
#endregion
	
	
#region Component modification
	//===========================================================
	public static Vector2 WithX( this Vector2 v, float x ) { return new Vector2( x, v.y ); }
	public static Vector3 WithX( this Vector3 v, float x ) { return new Vector3( x, v.y, v.z ); }
	public static Vector4 WithX( this Vector4 v, float x ) { return new Vector4( x, v.y, v.z, v.w ); }
	
	public static Vector2 WithY( this Vector2 v, float y ) { return new Vector2( v.x, y ); }
	public static Vector3 WithY( this Vector3 v, float y ) { return new Vector3( v.x, y, v.z ); }
	public static Vector4 WithY( this Vector4 v, float y ) { return new Vector4( v.x, y, v.z, v.w ); }
	
	public static Vector3 WithZ( this Vector3 v, float z ) { return new Vector3( v.x, v.y, z ); }
	public static Vector4 WithZ( this Vector4 v, float z ) { return new Vector4( v.x, v.y, z, v.w ); }
	
	public static Vector4 WithW( this Vector4 v, float w ) { return new Vector4( v.x, v.y, v.z, w ); }
	
	public static Vector2 WithMagnitude( this Vector2 v, float magnitude ) { return v.normalized *magnitude; }
	public static Vector3 WithMagnitude( this Vector3 v, float magnitude ) { return v.normalized *magnitude; }
	public static Vector4 WithMagnitude( this Vector4 v, float magnitude ) { return v.normalized *magnitude; }
#endregion
	
	
#region Component math
	//===========================================================
	public static Vector2Int Abs( this Vector2Int a ) {
		return new Vector2Int( Mathf.Abs( a.x ), Mathf.Abs( a.y ) );
	}
	public static Vector3Int Abs( this Vector3Int a ) {
		return new Vector3Int( Mathf.Abs( a.x ), Mathf.Abs( a.y ), Mathf.Abs( a.z ) );
	}
	
	public static Vector2 Abs( this Vector2 a ) {
		return new Vector2( Mathf.Abs( a.x ), Mathf.Abs( a.y ) );;
	}
	public static Vector3 Abs( this Vector3 a ) {
		return new Vector3( Mathf.Abs( a.x ), Mathf.Abs( a.y ), Mathf.Abs( a.z ) );
	}
	public static Vector4 Abs( this Vector4 a ) {
		return new Vector4( Mathf.Abs( a.x ), Mathf.Abs( a.y ), Mathf.Abs( a.z ), Mathf.Abs( a.w ) );
	}
	
	//===========================================================
	public static Vector2Int Sign( this Vector2Int a ) {
		return new Vector2Int( System.Math.Sign( a.x ), System.Math.Sign( a.y ) );
	}
	public static Vector3Int Sign( this Vector3Int a ) {
		return new Vector3Int( System.Math.Sign( a.x ), System.Math.Sign( a.y ), System.Math.Sign( a.z ) );
	}
	
	public static Vector2 Sign( this Vector2 a ) {
		return new Vector2( Mathf.Sign( a.x ), Mathf.Sign( a.y ) );;
	}
	public static Vector3 Sign( this Vector3 a ) {
		return new Vector3( Mathf.Sign( a.x ), Mathf.Sign( a.y ), Mathf.Sign( a.z ) );
	}
	public static Vector4 Sign( this Vector4 a ) {
		return new Vector4( Mathf.Sign( a.x ), Mathf.Sign( a.y ), Mathf.Sign( a.z ), Mathf.Sign( a.w ) );
	}
	
	//===========================================================
	public static Vector2Int Clamped( this Vector2Int a, Vector2Int min, Vector2Int max ) {
		return new Vector2Int( a.x.Clamp( min.x, max.x ), a.y.Clamp( min.y, max.y ) );
	}
	public static Vector3Int Clamped( this Vector3Int a, Vector3Int min, Vector3Int max ) {
		return new Vector3Int( a.x.Clamp( min.x, max.x ), a.y.Clamp( min.y, max.y ), a.z.Clamp( min.z, max.z ) );
	}
	
	public static Vector2 Clamped( this Vector2 a, Vector2 min, Vector2 max ) {
		return new Vector2( a.x.Clamp( min.x, max.x ), a.y.Clamp( min.y, max.y ) );
	}
	public static Vector3 Clamped( this Vector3 a, Vector3 min, Vector3 max ) {
		return new Vector3( a.x.Clamp( min.x, max.x ), a.y.Clamp( min.y, max.y ), a.z.Clamp( min.z, max.z ) );
	}
	public static Vector4 Clamped( this Vector4 a, Vector4 min, Vector4 max ) {
		return new Vector4( a.x.Clamp( min.x, max.x ), a.y.Clamp( min.y, max.y ), a.z.Clamp( min.z, max.z ), a.w.Clamp( min.w, max.w ) );
	}
	
	//===========================================================
	public static Vector2 MagnitudeClamped( this Vector2 v, float magnitude ) {
		return v.WithMagnitude( Mathf.Min( v.magnitude, magnitude ) );
	}
	public static Vector3 MagnitudeClamped( this Vector3 v, float magnitude ) {
		return v.WithMagnitude( Mathf.Min( v.magnitude, magnitude ) );
	}
	public static Vector4 MagnitudeClamped( this Vector4 v, float magnitude ) {
		return v.WithMagnitude( Mathf.Min( v.magnitude, magnitude ) );
	}
	
	public static Vector2Int MagnitudeClamped( this Vector2Int v, int magnitude ) {
		if( v == Vector2Int.zero ) return v;
		
		var factor = (magnitude /(float)v.Manhattan()).AtMost( 1f );
		var idealMahnattan = v.Manhattan().AtMost( magnitude );
		
		var allocations = v.ToVector2() *factor;
		var result = new Vector2Int( (int)allocations.x, (int)allocations.y );
		var error = (idealMahnattan - result.Manhattan()).Abs();
		var fracRemainders = allocations - result;
		var criteria = fracRemainders.Abs();
		if( error > 0 ) {
			if( criteria.x >= criteria.y ) {
				result.x += System.Math.Sign( fracRemainders.x );
			}
			else {
				result.y += System.Math.Sign( fracRemainders.y );
			}
		}
		return result;
	}
	public static Vector3Int MagnitudeClamped( this Vector3Int v, int magnitude ) {
		if( v == Vector3Int.zero ) return v;
		
		var factor = (magnitude /(float)v.Manhattan()).AtMost( 1f );
		var idealMahnattan = v.Manhattan().AtMost( magnitude );
		
		var allocations = v.ToVector3() *factor;
		var result = new Vector3Int( (int)allocations.x, (int)allocations.y, (int)allocations.z );
		var error = (idealMahnattan - result.Manhattan()).Abs();
		var fracRemainders = allocations - result;
		var criteria = fracRemainders.Abs();
		if( error > 0 ) {
			if( criteria.x >= criteria.y && criteria.x >= criteria.z ) {
				result.x += System.Math.Sign( fracRemainders.x );
				criteria.x -= 1;
			}
			else if( criteria.y >= criteria.z ) {
				result.y += System.Math.Sign( fracRemainders.y );
				criteria.y -= 1;
			}
			else {
				result.z += System.Math.Sign( fracRemainders.z );
				criteria.z -= 1;
			}
		}
		if( error == 2 ) {
			if( criteria.x >= criteria.y && criteria.x >= criteria.z ) {
				result.x += System.Math.Sign( fracRemainders.x );
			}
			else if( criteria.y >= criteria.z ) {
				result.y += System.Math.Sign( fracRemainders.y );
			}
			else {
				result.z += System.Math.Sign( fracRemainders.z );
			}
		}
		return result;
	}
	
	//===========================================================
	public static Vector2Int Wrapped( this Vector2Int a, Vector2Int limits ) {
		return new Vector2Int( a.x %limits.x, a.y %limits.y );
	}
	public static Vector3Int Wrapped( this Vector3Int a, Vector3Int limits ) {
		return new Vector3Int( a.x %limits.x, a.y %limits.y, a.z %limits.z );
	}
	
	public static Vector2 Wrapped( this Vector2 a, Vector2 limits ) {
		return new Vector2( Mathf.Repeat( a.x, limits.x ), Mathf.Repeat( a.y, limits.y ) );
	}
	public static Vector3 Wrapped( this Vector3 a, Vector3 limits ) {
		return new Vector3( Mathf.Repeat( a.x, limits.x ), Mathf.Repeat( a.y, limits.y ), Mathf.Repeat( a.z, limits.z ) );
	}
	public static Vector4 Wrapped( this Vector4 a, Vector4 limits ) {
		return new Vector4( Mathf.Repeat( a.x, limits.x ), Mathf.Repeat( a.y, limits.y ), Mathf.Repeat( a.z, limits.z ), Mathf.Repeat( a.w, limits.w ) );
	}
	
	//===========================================================
	public static Vector2Int Clamped01( this Vector2Int a ) {
		return a.Clamped( Vector2Int.zero, Vector2Int.one );
	}
	public static Vector3Int Clamped01( this Vector3Int a ) {
		return a.Clamped( Vector3Int.zero, Vector3Int.one );
	}
	
	public static Vector2 Clamped01( this Vector2 a ) {
		return a.Clamped( Vector2.zero, Vector2.one );
	}
	public static Vector3 Clamped01( this Vector3 a ) {
		return a.Clamped( Vector3.zero, Vector3.one );
	}
	public static Vector4 Clamped01( this Vector4 a ) {
		return a.Clamped( Vector4.zero, Vector4.one );
	}
	
	//===========================================================
	public static Vector2 Round( this Vector2 a ) {
		return new Vector2( Mathf.Round( a.x ), Mathf.Round( a.y ) );
	}
	public static Vector3 Round( this Vector3 a ) {
		return new Vector3( Mathf.Round( a.x ), Mathf.Round( a.y ), Mathf.Round( a.z ) );
	}
	public static Vector4 Round( this Vector4 a ) {
		return new Vector4( Mathf.Round( a.x ), Mathf.Round( a.y ), Mathf.Round( a.z ), Mathf.Round( a.w ) );
	}
	
	//===========================================================
	public static Vector2Int ComponentMul( this Vector2Int a, Vector2Int b ) {
		return a *b;
	}
	public static Vector3Int ComponentMul( this Vector3Int a, Vector3Int b ) {
		return a *b;
	}
	
	public static Vector2 ComponentMul( this Vector2 a, Vector2 b ) {
		return new Vector2( a.x *b.x, a.y *b.y );
	}
	public static Vector3 ComponentMul( this Vector3 a, Vector3 b ) {
		return new Vector3( a.x *b.x, a.y *b.y, a.z *b.z );
	}
	public static Vector4 ComponentMul( this Vector4 a, Vector4 b ) {
		return new Vector4( a.x *b.x, a.y *b.y, a.z *b.z, a.w *b.w );
	}
	
	public static Vector2 ComponentMul( this Vector2 a, float x, float y ) {
		return new Vector2( a.x *x, a.y *y );
	}
	public static Vector3 ComponentMul( this Vector3 a, float x, float y, float z ) {
		return new Vector3( a.x *x, a.y *y, a.z *z );
	}
	public static Vector4 ComponentMul( this Vector4 a, float x, float y, float z, float w ) {
		return new Vector4( a.x *x, a.y *y, a.z *z, a.w *w );
	}
	
	//===========================================================
	public static Vector2 ComponentDiv( this Vector2 a, Vector2 b ) {
		return new Vector2( a.x /b.x, a.y /b.y );
	}
	public static Vector3 ComponentDiv( this Vector3 a, Vector3 b ) {
		return new Vector3( a.x /b.x, a.y /b.y, a.z /b.z );
	}
	public static Vector4 ComponentDiv( this Vector4 a, Vector4 b ) {
		return new Vector4( a.x /b.x, a.y /b.y, a.z /b.z, a.w /b.w );
	}
	
	public static Vector2 ComponentDiv( this Vector2 a, float x, float y ) {
		return new Vector2( a.x /x, a.y /y );
	}
	public static Vector3 ComponentDiv( this Vector3 a, float x, float y, float z ) {
		return new Vector3( a.x /x, a.y /y, a.z /z );
	}
	public static Vector4 ComponentDiv( this Vector4 a, float x, float y, float z, float w ) {
		return new Vector4( a.x /x, a.y /y, a.z /z, a.w /w );
	}
	
#endregion
	
	
#region 3D math
	//===========================================================
	public static Vector2 Flat( this Vector2 original, Vector2 normal ) {
		return original - Vector3.Project( original, normal ).XY();
	}
	
	public static Vector3 Flat( this Vector3 original, Vector3 normal ) {
		return original - Vector3.Project( original, normal );
	}
	
	public static Vector4 Flat( this Vector4 original, Vector4 normal ) {
		return original - Vector4.Project( original, normal );
	}
	
	//===========================================================
	public static Vector2 ProjectedOn( this Vector2 original, Vector2 normal ) {
		return Vector3.Project( original, normal ).XY();
	}
	
	public static Vector3 ProjectedOn( this Vector3 original, Vector3 normal ) {
		return Vector3.Project( original, normal );
	}
	
	public static Vector4 ProjectedOn( this Vector4 original, Vector4 normal ) {
		return Vector4.Project( original, normal );
	}
	
	public static Vector3 ProjectedOn( this Vector3 point, Plane plane ) {
		var inPlaneDiff = point.Flat( plane.normal );
		return inPlaneDiff - plane.normal *plane.distance;
	}
	
	//===========================================================
	public static float CoAligness( this Vector3 a, Vector3 b ) {
		var projection = Vector3.Project( a, b );
		var result = projection.magnitude /b.magnitude;
		if( Vector3.Angle( a, b ) > 90 ) {
			result *= -1;
		}
		return result;
	}
	public static Vector2 CoAlignedComponent( this Vector2 candidate, Vector2 normal ) {
		if( Vector2.Angle( candidate, normal ) > 90.0f ) {
			return Vector2.zero;
		}
		return Vector3.Project( candidate, normal );
	}
	public static Vector3 CoAlignedComponent( this Vector3 candidate, Vector3 normal ) {
		if( Vector3.Angle( candidate, normal ) > 90.0f ) {
			return Vector3.zero;
		}
		return Vector3.Project( candidate, normal );
	}
	
	//===========================================================
	public static Vector3 Rotate( this Vector3 a, Vector3 axis, float angle ) {
		var rotation = Quaternion.AngleAxis( angle, axis );
		return (rotation *a);
	}
	
	public static Vector3 RotateAround( this Vector3 a, Ray axis, float angle ) {
		var rotation = Quaternion.AngleAxis( angle, axis.direction );
		
		var diff = a - axis.origin;
		diff = rotation *diff;
		return axis.origin + diff;
	}
	
	//===========================================================
	public static float Dot( this Vector3 a, Vector3 b ) {
		return Vector3.Dot( a, b );
	}
	public static int Dot( this Vector2Int a, Vector2Int b ) {
		var mul = a.ComponentMul( b );
		return mul.x + mul.y;
	}
	public static int Dot( this Vector3Int a, Vector3Int b ) {
		var mul = a.ComponentMul( b );
		return mul.x + mul.y + mul.z;
	}
	
	public static Vector3 Cross( this Vector3 a, Vector3 b ) {
		return Vector3.Cross( a, b );
	}
	
	public static float Cross( this Vector2 a, Vector2 b ) {
		return Vector.Determinant( a, b );
	}
	
	//===========================================================
	public static Quaternion RotateTowards( this Quaternion from, Quaternion to, float maxChangeDegrees ) {
		return from.RotateTowards( to, maxChangeDegrees, Vector3.forward );
	}
	
	public static Quaternion RotateTowards( this Quaternion from, Quaternion to, float maxChangeDegrees, Vector3 orientationHint ) {
		var fromOrientation = from *orientationHint;
		var toOrientation = to *orientationHint;
		var angleDiff = Vector3.Angle( fromOrientation, toOrientation );
		var factor = Mathf.Clamp01( maxChangeDegrees /angleDiff );
		return Quaternion.Slerp( from, to, factor );
	}
#endregion
	
	
#region Manhattan
	public static int Manhattan( this Vector2Int v ) { return v.x.Abs() + v.y.Abs(); }
	public static int Manhattan( this Vector3Int v ) { return v.x.Abs() + v.y.Abs() + v.z.Abs(); }
	
	public static float Manhattan( this Vector2 v ) { return v.x.Abs() + v.y.Abs(); }
	public static float Manhattan( this Vector3 v ) { return v.x.Abs() + v.y.Abs() + v.z.Abs(); }
	public static float Manhattan( this Vector4 v ) { return v.x.Abs() + v.y.Abs() + v.z.Abs() + v.w.Abs(); }
#endregion
	
	
#region Comparison
	//===========================================================
	public static bool EpsilonEquals( this Vector2 a, Vector2 b ) {
		if( !a.x.EpsilonEquals( b.x ) ) { return false; }
		if( !a.y.EpsilonEquals( b.y ) ) { return false; }
		return true;
	}
	public static bool EpsilonEquals( this Vector3 a, Vector3 b ) {
		if( !a.x.EpsilonEquals( b.x ) ) { return false; }
		if( !a.y.EpsilonEquals( b.y ) ) { return false; }
		if( !a.z.EpsilonEquals( b.z ) ) { return false; }
		return true;
	}
	public static bool EpsilonEquals( this Vector4 a, Vector4 b ) {
		if( !a.x.EpsilonEquals( b.x ) ) { return false; }
		if( !a.y.EpsilonEquals( b.y ) ) { return false; }
		if( !a.z.EpsilonEquals( b.z ) ) { return false; }
		if( !a.w.EpsilonEquals( b.w ) ) { return false; }
		return true;
	}
#endregion
	
	
#region Validity
	private static bool _IsAnyNaN( params float[] items ) {
		for( var i = 0; i < items.Length; i++ ) {
			if( float.IsNaN( items[i] ) ) {
				return true;
			}
		}
		return false;
	}
	private static bool _IsAnyMinValue( params int[] items ) {
		for( var i = 0; i < items.Length; i++ ) {
			if( items[i] == int.MinValue ) {
				return true;
			}
		}
		return false;
	}
	
	public static bool IsValid( this Vector2 v ) { return !_IsAnyNaN( v.x, v.y ); }
	public static bool IsValid( this Vector3 v ) { return !_IsAnyNaN( v.x, v.y, v.z ); }
	public static bool IsValid( this Vector4 v ) { return !_IsAnyNaN( v.x, v.y, v.z, v.w ); }
	
	public static bool IsValid( this Vector2Int v ) { return !_IsAnyMinValue( v.x, v.y ); }
	public static bool IsValid( this Vector3Int v ) { return !_IsAnyMinValue( v.x, v.y, v.z ); }
#endregion
	
	
#region Logging
	//===========================================================
	public static string LogExact( this Vector2 a ) {
		return "("+a.x+", "+a.y+")";
	}
	public static string LogExact( this Vector3 a ) {
		return "("+a.x+", "+a.y+", "+a.z+")";
	}
	public static string LogExact( this Vector4 a ) {
		return "("+a.x+", "+a.y+", "+a.z+", "+a.w+")";
	}
	
	public static string LogExact( this Quaternion q ) {
		return "("+q.x+", "+q.y+", "+q.z+", "+q.w+")";
	}
	
	//===========================================================
	public static string LogFormat( this Vector2 a, string format ) {
		return "("+a.x.ToString( format )+", "+a.y.ToString( format )+")";
	}
	public static string LogFormat( this Vector3 a, string format ) {
		return "("+a.x.ToString( format )+", "+a.y.ToString( format )+", "+a.z.ToString( format )+")";
	}
	public static string LogFormat( this Vector4 a, string format ) {
		return "("+a.x.ToString( format )+", "+a.y.ToString( format )+", "+a.z.ToString( format )+", "+a.w.ToString( format )+")";
	}
#endregion
	
	
#region Ergonomics
	public static Vector2 Lerp( this Vector2 from, Vector2 to, float t ) { return Vector2.Lerp( from, to, t ); }
	public static Vector3 Lerp( this Vector3 from, Vector3 to, float t ) { return Vector3.Lerp( from, to, t ); }
	public static Vector4 Lerp( this Vector4 from, Vector4 to, float t ) { return Vector4.Lerp( from, to, t ); }
	public static Color Lerp( this Color from, Color to, float t ) { return Color.Lerp( from, to, t ); }
	
	public static Vector2 LerpUnclamped( this Vector2 from, Vector2 to, float t ) { return Vector2.LerpUnclamped( from, to, t ); }
	public static Vector3 LerpUnclamped( this Vector3 from, Vector3 to, float t ) { return Vector3.LerpUnclamped( from, to, t ); }
	public static Vector4 LerpUnclamped( this Vector4 from, Vector4 to, float t ) { return Vector4.LerpUnclamped( from, to, t ); }
	public static Color LerpUnclamped( this Color from, Color to, float t ) { return Color.LerpUnclamped( from, to, t ); }
	
	public static Vector2 SmoothTo( this Vector2 from, Vector2 to, float halfLife, float dt ) {
		return to + (from - to) *Mathf.Pow( 2, -dt /halfLife );
	}
	public static Vector3 SmoothTo( this Vector3 from, Vector3 to, float halfLife, float dt ) {
		return to + (from - to) *Mathf.Pow( 2, -dt /halfLife );
	}
	public static Vector4 SmoothTo( this Vector4 from, Vector4 to, float halfLife, float dt ) {
		return to + (from - to) *Mathf.Pow( 2, -dt /halfLife );
	}
	public static Color SmoothTo( this Color from, Color to, float halfLife, float dt ) {
		return to + (from - to) *Mathf.Pow( 2, -dt /halfLife );
	}
#endregion
	
	
#region Temporary
#endregion
}
