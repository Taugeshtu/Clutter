using System.Collections.Generic;
using System.Linq;
using System;

//===========================================================//===========================================================
public static class MathExtensions {
	private const float c_epsilon = 0.000001f;
	
#region Angles
	public static float ToAngle( this float angle ) {
		var resultAngle = angle;
		if( resultAngle > 180f ) {
			resultAngle -= 360f;
		}
		return resultAngle;
	}
	
	public static UnityEngine.Vector3 ToAngles( this UnityEngine.Vector3 eulerAngles ) {
		var result = new UnityEngine.Vector3(
			eulerAngles.x.ToAngle(),
			eulerAngles.y.ToAngle(),
			eulerAngles.z.ToAngle()
		);
		return result;
	}
	
	public static string ToAngleString( this float angle ) {
		return angle.ToAngle().ToString( "n1" );
	}
#endregion
	
	
#region Regular math
	public static float Wrap( this float a, float max ) { return Wrap( a, 0, max ); }
	public static float Wrap( this float a, float min, float max ) {
		var range = max - min;
		var diff = a - min;
		a = a - (float) System.Math.Round( diff /range ) *range;
		if( a < 0 ) {
			a = a + max - min;
		}
		return a;
	}
	public static double Wrap( this double a, double max ) { return Wrap( a, 0, max ); }
	public static double Wrap( this double a, double min, double max ) {
		var range = max - min;
		var diff = a - min;
		a -= Math.Round( diff /range ) *range;
		if( a < 0 ) {
			a = a + max - min;
		}
		return a;
	}
	
	public static int Wrap( this int x, int wrapSize ) {
		return (x %wrapSize + wrapSize) %wrapSize;
	}
	
	public static bool EpsilonEquals( this float a, int b ) {
		return b.EpsilonEquals( a );
	}
	public static bool EpsilonEquals( this int a, float b ) {
		return System.Math.Abs( a - b ) < c_epsilon*2f;
	}
	public static bool EpsilonEquals( this float a, float b ) {
		return System.Math.Abs( a - b ) < c_epsilon*2f;
	}
	public static bool EpsilonEquals( this double a, double b ) {
		return System.Math.Abs( (float) (a - b) ) < c_epsilon*2f;
	}
	
	public static float SetMagnitude( this ref float x, float mag ) {
		x = Math.Sign( x ) *mag;
		return x;
	}
	
	public static int Sign( this float x ) {
		return Math.Sign( x );
	}
	
	public static float SmoothTo( this float from, float to, float halfLife, float dt ) {
		return to + (from - to) *((float) Math.Pow( 2, -dt /halfLife ));
	}
	public static double SmoothTo( this double from, double to, double halfLife, double dt ) {
		return to + (from - to) *Math.Pow( 2, -dt /halfLife );
	}
#endregion
	
	
#region Generic math
	public static float Clamp01( this int x ) { return UnityEngine.Mathf.Clamp01( x ); }
	public static float Clamp01( this float x ) { return UnityEngine.Mathf.Clamp01( x ); }
	
	// Note: this is INCLUSIVE on both ends
	public static T Clamp<T>( this T x, T min, T max ) where T : IComparable<T> {
		if( x.CompareTo( min ) < 0 ) {
			return min;
		}
		else if( x.CompareTo( max ) > 0 ) {
			return max;
		}
		else {
			return x;
		}
	}
	public static T AtMost<T>( this T x, T maxAllowed ) where T : IComparable<T> {
		return x.CompareTo( maxAllowed ) < 0 ? x : maxAllowed;
	}
	public static T AtLeast<T>( this T x, T minAllowed ) where T : IComparable<T> {
		return x.CompareTo( minAllowed ) > 0 ? x : minAllowed;
	}
	
	public static bool IsInRange<T>( this T x, T min, T max ) where T: IComparable<T> {
		if( x.CompareTo( min ) < 0 ) {
			return false;
		}
		else if( x.CompareTo( max ) > 0 ) {
			return false;
		}
		else {
			return true;
		}
	}
	
	public static int InverseClamp( this int x, int min, int max ) {
		if( x < min ) {
			return x;
		}
		else if( x > max ) {
			return x;
		}
		else {
			var mid = (min + max) *0.5f;
			return (x < mid) ? min : max;
		}
	}
	
	public static float InverseClamp( this float x, float min, float max ) {
		if( x < min ) {
			return x;
		}
		else if( x > max ) {
			return x;
		}
		else {
			var mid = (min + max) *0.5f;
			return (x < mid) ? min : max;
		}
	}
	
	public static double InverseClamp( this double x, double min, double max ) {
		if( x < min ) {
			return x;
		}
		else if( x > max ) {
			return x;
		}
		else {
			var mid = (min + max) *0.5f;
			return (x < mid) ? min : max;
		}
	}
#endregion
	
	
#region Double expansions
	public static double Lerp( double from, double to, float t ) {
		return LerpUnclamped( from, to, t.Clamp( 0f, 1f ) );
	}
	public static double LerpUnclamped( double from, double to, float t ) {
		return from + t *(to - from);
	}
	public static double InverseLerp( double from, double to, double current ) {
		return (current - from) /(to - from);
	}
	
	public static long Abs( this long val ) {
		return val < 0 ? -val : val;
	}
#endregion
	
	
#region Iterators
	// Note: this is INCLUSIVE of the "to"!
	public static IEnumerable<int> To( this int from, int to ) {
		if( from < to ) {
			while( from <= to ) {
				yield return from++;
			}
		} else {
			while( from >= to ) {
				yield return from--;
			}
		}
	}
	
	public static IEnumerable<T> Step<T>( this IEnumerable<T> source, int step ) {
		if( step == 0 ) {
			throw new System.ArgumentOutOfRangeException( "step", "Param cannot be zero." );
		}
		return source.Where( (x, i) => (i % step) == 0 );
	}
	
	// finds an index where the element is/would be inserted in the list to retain sorted state
	// input list MUST be sorted
	public static int BSInsertionindex<T>( this IList<T> list, T value ) {
		if( list == null ) {
			throw new System.ArgumentNullException( "list" );
		}
		
		var comp = Comparer<T>.Default;
		var lo = 0;
		var hi = list.Count - 1;
		while( lo <= hi ) {
			var m = lo + (hi - lo) /2;
			
			if( comp.Compare( list[m], value ) < 0 ) {
				lo = m + 1;
			}
			else {
				hi = m - 1;
			}
		}
		
		if( lo < list.Count && comp.Compare( list[lo], value ) < 0 ) {
			lo++;
		}
		return lo;
	}
#endregion
	
	
#region Bit fuckery
	public static bool GetBit( this byte x, int bitIndex ) { return (x & (1 << bitIndex)) != 0; }
	public static bool GetBit( this ushort x, int bitIndex ) { return (x & (1 << bitIndex)) != 0; }
	public static bool GetBit( this short x, int bitIndex ) { return (x & (1 << bitIndex)) != 0; }
	public static bool GetBit( this int x, int bitIndex ) { return (x & (1 << bitIndex)) != 0; }
	public static bool GetBit( this uint x, int bitIndex ) { return (x & (1 << bitIndex)) != 0; }
	public static bool GetBit( this long x, int bitIndex ) { return (x & (1 << bitIndex)) != 0; }
	public static bool GetBit( this ulong x, int bitIndex ) { return (x & ((ulong)1 << bitIndex)) != 0; }
	
	public static byte SetBit( this byte x, int bitIndex, bool bitValue ) { return (byte) (bitValue ? (x | 1 << bitIndex) : (x & ~(1 << bitIndex))); }
	public static ushort SetBit( this ushort x, int bitIndex, bool bitValue ) { return (ushort)(bitValue ? (x | 1 << bitIndex) : (x & ~(1 << bitIndex))); }
	public static short SetBit( this short x, int bitIndex, bool bitValue ) { return (short)(bitValue ? (x | 1 << bitIndex) : (x & ~(1 << bitIndex))); }
	public static int SetBit( this int x, int bitIndex, bool bitValue ) { return bitValue ? (x | 1 << bitIndex) : (x & ~(1 << bitIndex)); }
	public static uint SetBit( this uint x, int bitIndex, bool bitValue ) { return bitValue ? (x | (uint)(1 << bitIndex)) : (x & (uint)~(1 << bitIndex)); }
	public static long SetBit( this long x, int bitIndex, bool bitValue ) { return bitValue ? (x | 1 << bitIndex) : (x & ~(1 << bitIndex)); }
	public static ulong SetBit( this ulong x, int bitIndex, bool bitValue ) { return bitValue ? (x | (ulong)(1 << bitIndex)) : (x & (ulong)~(1 << bitIndex)); }
	
	public static int CeilToPOT( this int x ) {
		var ceiledToPOT = 1;
		while( ceiledToPOT < x ) {
			ceiledToPOT <<= 1;
		}
		return ceiledToPOT;
	}
	
	public static (int, int) GetCeiledPOT( this int x ) {
		var ceiledToPOT = 1;
		var power = 0;
		while( ceiledToPOT < x ) {
			ceiledToPOT <<= 1;
			power += 1;
		}
		return (ceiledToPOT, power);
	}
	
	public static int ToInt( this bool b ) {
		return b ? 1 : 0;
	}
	
	public static bool ToBool( this int x ) {
		return x == 0 ? false : true;
	}
#endregion
}
