using UnityEngine;
using System.Collections.Generic;

//===========================================================//===========================================================
public static class VectorExtensions {
	
#region Invalids
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
#endregion
	
	
#region Vector2 converters
	//===========================================================
	public static Vector3 XY0( this Vector2 value ) { return new Vector3( value.x, value.y, 0 ); }
	public static Vector3 X0Y( this Vector2 value ) { return new Vector3( value.x, 0, value.y ); }
#endregion
	
	
#region Vector3 converters
	//===========================================================
	public static Vector2 XY( this Vector3 value ) { return new Vector2( value.x, value.y ); }
	public static Vector2 XZ( this Vector3 value ) { return new Vector2( value.x, value.z ); }
	public static Vector2 YX( this Vector3 value ) { return new Vector2( value.y, value.x ); }
	public static Vector2 YZ( this Vector3 value ) { return new Vector2( value.y, value.z ); }
	public static Vector2 ZX( this Vector3 value ) { return new Vector2( value.z, value.x ); }
	public static Vector2 ZY( this Vector3 value ) { return new Vector2( value.z, value.y ); }
#endregion
	
	
#region Vector4 converters
	//===========================================================
	public static Vector3 XYZ( this Vector4 value ) { return new Vector3( value.x, value.y, value.z ); }
	public static Vector2 XY( this Vector4 value ) { return new Vector2( value.x, value.y ); }
	public static Vector2 ZW( this Vector4 value ) { return new Vector2( value.z, value.w ); }
#endregion
	
	
#region Component access
	//===========================================================
	public static float[] Components( this Vector2 a ) {
		return new float[] {a.x, a.y};
	}
	public static float[] Components( this Vector3 a ) {
		return new float[] {a.x, a.y, a.z};
	}
	public static float[] Components( this Vector4 a ) {
		return new float[] {a.x, a.y, a.z, a.w};
	}
	
	public static int[] Components( this Vector2Int a ) {
		return new int[] {a.x, a.y};
	}
	public static int[] Components( this Vector3Int a ) {
		return new int[] {a.x, a.y, a.z};
	}
	
	//===========================================================
	public static float MinComponent( this Vector2 a ) {
		return Mathf.Min( a.x, a.y );
	}
	public static float MinComponent( this Vector3 a ) {
		return Mathf.Min( a.x, a.y, a.z );
	}
	public static float MinComponent( this Vector4 a ) {
		return Mathf.Min( a.x, a.y, a.z, a.w );
	}
	
	//===========================================================
	public static float MaxComponent( this Vector2 a ) {
		return Mathf.Max( a.x, a.y );
	}
	public static float MaxComponent( this Vector3 a ) {
		return Mathf.Max( a.x, a.y, a.z );
	}
	public static float MaxComponent( this Vector4 a ) {
		return Mathf.Max( a.x, a.y, a.z, a.w );
	}
#endregion
	
	
#region Component modification
	//===========================================================
	public static Vector2 WithX( this Vector2 v, float x ) {
		return new Vector2( x, v.y );
	}
	public static Vector3 WithX( this Vector3 v, float x ) {
		return new Vector3( x, v.y, v.z );
	}
	public static Vector4 WithX( this Vector4 v, float x ) {
		return new Vector4( x, v.y, v.z, v.w );
	}
	
	public static Vector2 WithY( this Vector2 v, float y ) {
		return new Vector2( v.x, y );
	}
	public static Vector3 WithY( this Vector3 v, float y ) {
		return new Vector3( v.x, y, v.z );
	}
	public static Vector4 WithY( this Vector4 v, float y ) {
		return new Vector4( v.x, y, v.z, v.w );
	}
	
	public static Vector3 WithZ( this Vector3 v, float z ) {
		return new Vector3( v.x, v.y, z );
	}
	public static Vector4 WithZ( this Vector4 v, float z ) {
		return new Vector4( v.x, v.y, z, v.w );
	}
	
	public static Vector4 WithW( this Vector4 v, float w ) {
		return new Vector4( v.x, v.y, v.z, w );
	}
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
	public static Vector2 ComponentMul( this Vector2 a, Vector2 b ) {
		return new Vector2( a.x *b.x, a.y *b.y );
	}
	public static Vector3 ComponentMul( this Vector3 a, Vector3 b ) {
		return new Vector3( a.x *b.x, a.y *b.y, a.z *b.z );
	}
	public static Vector4 ComponentMul( this Vector4 a, Vector4 b ) {
		return new Vector4( a.x *b.x, a.y *b.y, a.z *b.z, a.w *b.w );
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
#endregion
	
	
#region Temporary
#endregion
}
