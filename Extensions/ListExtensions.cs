using System.Collections.Generic;
using System.Text;
using System;

public static class ListExtensions {
	
#region List padding
	public static void Resize<T>( this List<T> list, int newSize, T template = default( T ) ) {
		var currentSize = list.Count;
		
		if( newSize < currentSize ) {
			var diff = currentSize - newSize;
			list.RemoveRange( newSize, diff );
		}
		else {
			list.GrowBy( newSize - currentSize, template );
		}
	}
	
	public static void GrowBy<T>( this IList<T> list, int itemsToAdd, T template = default( T ) ) {
		// this will skip if growth is <= 0
		for( var i = 0; i < itemsToAdd; i++ ) {
			list.Add( template );
		}
	}
	
	public static void ClearExpand<T>( this List<T> list, int desiredCapacity ) {
		list.Clear();
		if( list.Capacity < desiredCapacity ) {
			list.Capacity = desiredCapacity;
		}
	}
#endregion
	
	
#region Access
	public static TV GetAdd<TK, TV>( this Dictionary<TK, TV> map, TK key, TV template = default( TV ) ) {
		TV result;
		if( map.TryGetValue( key, out result ) ) {
			return result;
		}
		else {
			map[key] = template;
			return template;
		}
	}
	
	public static TV GetAt<TK, TV>( this IDictionary<TK, TV> map, TK key, TV template = default( TV ) ) {
		if( map.ContainsKey( key ) ) { return map[key]; }
		return template;
	}
	
	public static T GetAt<T>( this IList<T> list, int index, T template = default( T ) ) {
		if( index < 0 ) { return template; }
		if( index >= list.Count ) { return template; }
		return list[index];
	}
	
	// returns true if list was grown
	public static bool SetAt<T>( this IList<T> list, int index, T item, T template = default( T ) ) {
		var count = list.Count;
		if( index == count ) {
			list.Add( item );
			return true;
		}
		
		// despite being logically superior to "index == count" condition, in practice this will happen approximately never
		if( index < 0 ) {
			return false;
		}
		
		if( index < count ) {
			list[index] = item;
			return false;
		}
		
		for( var i = count; i < index; i++ ) {
			list.Add( template );
		}
		list.Add( item );
		
		return true;
	}
	
	public static T RoundRobin<T>( this IList<T> list, int index ) {
		if( list.Count == 0 ) { throw new System.Exception( "can't RoundRobin an empty list" ); }
		
		var actualIndex = index.Wrap( list.Count );
		return list[actualIndex];
	}
	
	// For a list {A, B, C, D}
	// it will iterate pairs as follows:
	// (A,B) (A,C) (A,D)
	// (B,C) (B,D)
	// (C,D)
	
	public static IEnumerable<ValueTuple<T, T>> IteratePairs<T>( this IList<T> list ) {
		var count = list.Count;
		for( var iA = 0; iA < count - 1; iA++ ) {
		for( var iB = iA + 1; iB < count; iB++ ) {
			yield return new ValueTuple<T, T>( list[iA], list[iB] );
		}
		}
	}
	
	// Usage:
	// var items = new List<float>() { 0, 1, 5 };
	// var testValue = 1.5f;
	// var index = items.FindClosestMatchIndex( (x) => x.CompareTo( testValue ), (x) => (x - testValue).Abs() );
	// should return 1 - index of "1" in the list, since it's closest to testValue
	// "evaluator" is "how does an item from the list compare to our target?" return 0 if exact match, -1 if item is smaller than target
	// "diff" is "what's the difference metric between a list item and target?" - for final stage decision
	// "diff" MUST return an absolute difference, no sign
	public static int FindClosestMatchIndex<T, TDiff>( this IList<T> items, Func<T, int> evaluator, Func<T, TDiff> diff ) where TDiff:IComparable {
		int low = 0, high = items.Count - 1;
		
		while( low <= high ) {
			var mid = (low + high) /2;
			var evaluation = evaluator( items[mid] );
			if( evaluation == 0 ) {
				return mid;
			}
			else if( evaluation == -1 ) {
				low = mid + 1;
			}
			else {
				high = mid - 1;
			}
		}
		
		// If we're at the extremes of the list
		if( low >= items.Count ) return high;
		if( high < 0 ) return low;
		
		// Compare and return the closest index
		var diffLow = diff( items[low] );
		var diffHigh = diff( items[high] );
		return diffLow.CompareTo( diffHigh ) == -1 ? low : high;
	}
	
	public static T Last<T>( this IList<T> list ) {
		if( list.Count == 0 ) { throw new System.Exception( "can't get Last in an empty list" ); }
		
		return list[list.Count - 1];
	}
#endregion
	
	
#region Swaps
	public static void SwapBack<T>( this IList<T> list, int elementIndex, bool silentFail = false ) {
		var lastIndex = list.Count - 1;
		Swap( list, elementIndex, lastIndex, 1, silentFail );
	}
	
	public static void Swap<T>( this IList<T> list, int indexA, int indexB, int range = 1, bool silentFail = false ) {
		if( indexA == indexB ) { return; }	// NOP
		
		_BoundsCheck( indexA, indexB, range, list.Count, silentFail );
		
		var held = default( T );
		for( var shift= 0; shift < range; shift++ ) {
			var shiftedA = indexA + shift;
			var shiftedB = indexB + shift;
			held = list[shiftedA];
			list[shiftedA] = list[shiftedB];
			list[shiftedB] = held;
		}
	}
	
	public static void HalfSwap<T>( this IList<T> list, int deadIndex, int aliveIndex, int range = 1, bool silentFail = false ) {
		if( deadIndex == aliveIndex ) { return; }	// NOP
		
		_BoundsCheck( deadIndex, aliveIndex, range, list.Count, silentFail );
		
		for( var shift = 0; shift < range; shift++ ) {
			list[deadIndex + shift] = list[aliveIndex + shift];
		}
	}
	
	private static void _BoundsCheck( int indexA, int indexB, int range, int listCount, bool silentFail ) {
		#if UNITY_EDITOR
			var rangeA = new Range( indexA, indexA + range - 1 );	// Note: "-1" makes range tight, since it's inclusive!
			var rangeB = new Range( indexB, indexB + range - 1 );
			
			if( silentFail ) {
				var bound = listCount - range;
				if( indexA < 0 || indexA > bound ) { return; }
				if( indexB < 0 || indexB > bound ) { return; }
				if( rangeA.Intersects( rangeB ) ) { return; }
			}
			else {
				if( rangeA.Intersects( rangeB ) ) {
					var message = "Swap ranges overlap - this will confuse the operation. Expecting range <= "
						+System.Math.Abs( indexA - indexB )+" for swap marks ["+indexA+"] <-> ["+indexB+"]";
					throw new ArgumentOutOfRangeException( "range", range, message );
				}
			}
		#endif
	}
#endregion
	
	
#region Adds
	// Might look stupid, but it ain't. Tis how .NET libs themselves do it - no alloc, fast, cheap
	public static void Add<T>( this ICollection<T> collection, T item1, T item2 ) {
		collection.Add( item1 );
		collection.Add( item2 );
	}
	public static void Add<T>( this ICollection<T> collection, T item1, T item2, T item3 ) {
		collection.Add( item1 );
		collection.Add( item2 );
		collection.Add( item3 );
	}
	public static void Add<T>( this ICollection<T> collection, T item1, T item2, T item3, T item4 ) {
		collection.Add( item1 );
		collection.Add( item2 );
		collection.Add( item3 );
		collection.Add( item4 );
	}
	public static void Add<T>( this ICollection<T> collection, T item1, T item2, T item3, T item4, T item5 ) {
		collection.Add( item1 );
		collection.Add( item2 );
		collection.Add( item3 );
		collection.Add( item4 );
		collection.Add( item5 );
	}
	public static void Add<T>( this ICollection<T> collection, IEnumerable<T> items ) {
		foreach( var item in items ) {
			collection.Add( item );
		}
	}
#endregion
	
	
#region Removes
	public static void Remove<T>( this ICollection<T> collection, T item1, T item2 ) {
		collection.Remove( item1 );
		collection.Remove( item2 );
	}
	public static void Remove<T>( this ICollection<T> collection, T item1, T item2, T item3 ) {
		collection.Remove( item1 );
		collection.Remove( item2 );
		collection.Remove( item3 );
	}
	public static void Remove<T>( this ICollection<T> collection, T item1, T item2, T item3, T item4 ) {
		collection.Remove( item1 );
		collection.Remove( item2 );
		collection.Remove( item3 );
		collection.Remove( item4 );
	}
	public static void Remove<T>( this ICollection<T> collection, T item1, T item2, T item3, T item4, T item5 ) {
		collection.Remove( item1 );
		collection.Remove( item2 );
		collection.Remove( item3 );
		collection.Remove( item4 );
		collection.Remove( item5 );
	}
	public static void Remove<T>( this ICollection<T> collection, IEnumerable<T> items ) {
		foreach( var item in items ) {
			collection.Remove( item );
		}
	}
	
	public static void FastRemove<T>( this IList<T> list, int elementIndex, bool silentFail = false ) {
		var lastIndex = list.Count - 1;
		if( silentFail ) {
			if( elementIndex < 0 || elementIndex > lastIndex ) { return; }
			if( lastIndex < 0 ) { return; }
		}
		
		list.HalfSwap( elementIndex, lastIndex, 1, silentFail );
		list.RemoveAt( lastIndex );
	}
#endregion
	
	
#region Logging
	public static string Dump<T>( this IEnumerable<T> list, System.Func<T, string> printer, string separator = ", " ) {
		return list.Dump( separator, printer );
	}
	public static string Dump<T>( this IEnumerable<T> list, string separator = ", ", System.Func<T, string> printer = null ) {
		var builder = new System.Text.StringBuilder();
		var counter = list.DumpInto( builder, separator, printer );
		return "("+counter+")"+separator+builder.ToString();
	}
	public static int DumpInto<T>( this IEnumerable<T> list, StringBuilder builder, string separator = ", ", System.Func<T, string> printer = null ) {
		var hasPrinter = (printer != null);
		var canBeNull = (default( T ) == null);
		var counter = 0;
		
		foreach( var item in list ) {
			if( canBeNull && object.Equals( item, default( T ) ) ) {
				builder.Append( "-null-" );
			}
			else {
				builder.Append( hasPrinter ? printer( item ) : item.ToString() );
			}
			builder.Append( separator );
			
			counter += 1;
		}
		
		if( counter > 0 ) { builder.TrimEnd( separator.Length ); }
		return counter;
	}
	
	public static string Dump<TKey, TValue>( this Dictionary<TKey, TValue> dictionary, System.Func<TValue, string> printer, string separator = "\n" ) {
		return dictionary.Dump( separator, printer );
	}
	public static string Dump<TKey, TValue>( this Dictionary<TKey, TValue> dictionary, string separator = "\n", System.Func<TValue, string> printer = null ) {
		var builder = new StringBuilder();
		var counter = dictionary.DumpInto( builder, separator, printer );
		return "("+counter+")"+separator+builder.ToString();
	}
	public static int DumpInto<TKey, TValue>( this Dictionary<TKey, TValue> dictionary, StringBuilder builder, string separator = "\n", System.Func<TValue, string> printer = null ) {
		var hasPrinter = (printer != null);
		var canBeNull = (default( TValue ) == null);
		
		foreach( var pair in dictionary ) {
			builder.Append( "[" );
			builder.Append( pair.Key.ToString() );
			builder.Append( ":" );
			if( canBeNull && pair.Value.Equals( default( TValue ) ) ) {
				builder.Append( "-null-" );
			}
			else {
				builder.Append( hasPrinter ? printer( pair.Value ) : pair.Value.ToString() );
			}
			builder.Append( "]" );
			
			builder.Append( separator );
		}
		
		var counter = dictionary.Count;
		if( counter > 0 ) { builder.TrimEnd( separator.Length ); }
		return counter;
	}
#endregion
	
	
#region Misc
	public static List<T> ToList<T>( this IEnumerable<T> collection ) {
		return new List<T>( collection );
	}
#endregion
	
	
#region Temporary
#endregion
}
