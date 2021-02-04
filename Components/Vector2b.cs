using UnityEngine;
using System;

[Serializable]
public struct Vector2b : IEquatable<Vector2b> {
	public bool x;
	public bool y;
	
#region Prefabs
	public static Vector2b False { get { return new Vector2b( false, false ); } }
	public static Vector2b True { get { return new Vector2b( true, true ); } }
#endregion
	
	
#region Constructors
	public Vector2b( bool x, bool y, bool z = false ) {
		this.x = x;
		this.y = y;
	}
	public Vector2b( Vector2Int original ) {
		this.x = (original.x != 0);
		this.y = (original.y != 0);
	}
	public Vector2b( Vector3Int original ) {
		this.x = (original.x != 0);
		this.y = (original.y != 0);
	}
#endregion
	
	
#region Equality
	public override bool Equals( object other ) {
		if( other is Vector2b ) {
			return Equals( (Vector2b) other );
		}
		else if( other is Vector3Int ) {
			return Equals( (Vector3Int) other );
		}
		
		return false;
	}
	public bool Equals( Vector2b other ) {
		return (x == other.x) && (y == other.y);
	}
	public bool Equals( Vector3Int other ) {
		return ((other.x != 0) == x) && ((other.y != 0) == y);
	}
	
	public override int GetHashCode() {
		var result = (x ? 1 : 0) + (y ? 2 : 0);
		return result;
	}
	
	public static bool operator ==( Vector2b a, Vector2b b ) {
		return a.Equals( b );
	}
	public static bool operator !=( Vector2b a, Vector2b b ) {
		return !a.Equals( b );
	}
#endregion
	
	
#region Math
	public static Vector2b operator !( Vector2b a ) {
		return new Vector2b( !a.x, !a.y );
	}
	
	public static Vector2b operator &( Vector2b a, Vector2b b ) {
		return new Vector2b( a.x & b.x, a.y & b.y );
	}
	public static Vector2b operator |( Vector2b a, Vector2b b ) {
		return new Vector2b( a.x | b.x, a.y | b.y );
	}
	public static Vector2b operator ^( Vector2b a, Vector2b b ) {
		return new Vector2b( a.x ^ b.x, a.y ^ b.y );
	}
	
	public static Vector2b operator +( Vector2b a, Vector2b b ) {
		return a | b;
	}
	public static Vector2b operator -( Vector2b a, Vector2b b ) {
		return a & !b;
	}
	public static Vector2b operator *( Vector2b a, Vector2b b ) {
		return a & b;
	}
	
	public static Vector2b operator +( Vector2Int a, Vector2b b ) { return (b + a); }
	public static Vector2b operator +( Vector2b a, Vector2Int b ) {
		return a + new Vector2b( b );
	}
	public static Vector2b operator -( Vector2Int a, Vector2b b ) { return (b - a); }
	public static Vector2b operator -( Vector2b a, Vector2Int b ) {
		return a - new Vector2b( b );
	}
	
	public static Vector2b operator -( Vector2b a ) {
		return !a;
	}
	
	public static Vector2Int operator *( Vector2Int a, Vector2b b ) {
		return new Vector2Int( (b.x ? a.x : 0), (b.y ? a.y : 0) );
	}
	public static Vector2b operator *( Vector2b a, Vector2Int b ) {
		return a & new Vector2b( b );
	}
#endregion
	
	
#region Misc
	public override string ToString() {
		return "("+x+", "+y+")";
	}
	
	public Vector2Int ToVector2Int() {
		return new Vector2Int( (x ? 1 : 0), (y ? 1 : 0) );
	}
#endregion
}