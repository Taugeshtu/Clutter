using UnityEngine;
using System;

[Serializable]
public struct Vector3b : IEquatable<Vector3b> {
	public bool x;
	public bool y;
	public bool z;
	
#region Prefabs
	public static Vector3b False { get { return new Vector3b( false, false, false ); } }
	public static Vector3b True { get { return new Vector3b( true, true, true ); } }
#endregion
	
	
#region Constructors
	public Vector3b( bool x, bool y, bool z = false ) {
		this.x = x;
		this.y = y;
		this.z = z;
	}
	public Vector3b( Vector2Int original ) {
		this.x = (original.x != 0);
		this.y = (original.y != 0);
		this.z = false;
	}
	public Vector3b( Vector3Int original ) {
		this.x = (original.x != 0);
		this.y = (original.y != 0);
		this.z = (original.z != 0);
	}
#endregion
	
	
#region Equality
	public override bool Equals( object other ) {
		if( other is Vector3b ) {
			return Equals( (Vector3b) other );
		}
		else if( other is Vector3Int ) {
			return Equals( (Vector3Int) other );
		}
		
		return false;
	}
	public bool Equals( Vector3b other ) {
		return (x == other.x) && (y == other.y) && (z == other.z);
	}
	public bool Equals( Vector3Int other ) {
		return ((other.x != 0) == x) && ((other.y != 0) == y) && ((other.z != 0) == z);
	}
	
	public override int GetHashCode() {
		var result = (x ? 1 : 0) + (y ? 2 : 0) + (z ? 4 : 0);
		return result;
	}
	
	public static bool operator ==( Vector3b a, Vector3b b ) {
		return a.Equals( b );
	}
	public static bool operator !=( Vector3b a, Vector3b b ) {
		return !a.Equals( b );
	}
#endregion
	
	
#region Math
	public static Vector3b operator !( Vector3b a ) {
		return new Vector3b( !a.x, !a.y, !a.z );
	}
	
	public static Vector3b operator &( Vector3b a, Vector3b b ) {
		return new Vector3b( a.x & b.x, a.y & b.y, a.z & b.z );
	}
	public static Vector3b operator |( Vector3b a, Vector3b b ) {
		return new Vector3b( a.x | b.x, a.y | b.y, a.z | b.z );
	}
	public static Vector3b operator ^( Vector3b a, Vector3b b ) {
		return new Vector3b( a.x ^ b.x, a.y ^ b.y, a.z ^ b.z );
	}
	
	public static Vector3b operator +( Vector3b a, Vector3b b ) {
		return a | b;
	}
	public static Vector3b operator -( Vector3b a, Vector3b b ) {
		return a & !b;
	}
	public static Vector3b operator *( Vector3b a, Vector3b b ) {
		return a & b;
	}
	
	public static Vector3b operator +( Vector3Int a, Vector3b b ) { return (b + a); }
	public static Vector3b operator +( Vector3b a, Vector3Int b ) {
		return a + new Vector3b( b );
	}
	public static Vector3b operator -( Vector3Int a, Vector3b b ) { return (b - a); }
	public static Vector3b operator -( Vector3b a, Vector3Int b ) {
		return a - new Vector3b( b );
	}
	
	public static Vector3b operator -( Vector3b a ) {
		return !a;
	}
	
	public static Vector3Int operator *( Vector3Int a, Vector3b b ) {
		return new Vector3Int( (b.x ? a.x : 0), (b.y ? a.y : 0), (b.z ? a.z : 0) );
	}
	public static Vector3b operator *( Vector3b a, Vector3Int b ) {
		return a & new Vector3b( b );
	}
#endregion
	
	
#region Misc
	public override string ToString() {
		return "("+x+", "+y+", "+z+")";
	}
	
	public Vector3Int ToVector3Int() {
		return new Vector3Int( (x ? 1 : 0), (y ? 1 : 0), (z ? 1 : 0) );
	}
#endregion
}