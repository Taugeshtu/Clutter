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
	
	public static string ToAngleString( this float angle ) {
		return angle.ToAngle().ToString( "n1" );
	}
#endregion
	
	
#region Regular math
	public static float Wrap( this float a, float min, float max ) {
		var range = max - min;
		var diff = a - min;
		a = a - (float) System.Math.Round( diff /range ) *range;
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
#endregion
	
	
#region Generic math
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
	
	public static int BinarySearch<T>( this IList<T> list, T value ) {
		if( list == null ) {
			throw new System.ArgumentNullException( "list" );
		}
		
		var comp = Comparer<T>.Default;
		var lo = 0;
		var hi = list.Count - 1;
		while( lo < hi ) {
			int m = (hi + lo) / 2;		// this might overflow; be careful.
			
			if( comp.Compare( list[m], value ) < 0 ) {
				lo = m + 1;
			}
			else {
				hi = m - 1;
			}
		}
		
		if( comp.Compare( list[lo], value ) < 0 ) {
			lo++;
		}
		return lo;
	}
#endregion
}
