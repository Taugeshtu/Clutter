using UnityEngine;
using System.Collections.Generic;

public static class ListExtensions {
	
#region List padding
	public static void PadUpTo<T>( this IList<T> list, int totalCount ) {
		list.PadUpTo( totalCount, default( T ) );
	}
	public static void PadUpTo<T>( this IList<T> list, int totalCount, T template ) {
		var padAmount = totalCount - list.Count;
		if( padAmount > 0 ) {
			list.Pad( padAmount, template );
		}
	}
	
	public static void Pad<T>( this IList<T> list, int count ) {
		list.Pad( count, default( T ) );
	}
	public static void Pad<T>( this IList<T> list, int count, T template ) {
		for( var i = 0; i < count; i++ ) {
			list.Add( template );
		}
	}
#endregion
	
	
#region Swaps
	public static void SwapBack<T>( this IList<T> list, int elementIndex, bool silentFail = false ) {
		var lastIndex = list.Count - 1;
		Swap( list, elementIndex, lastIndex, 1, silentFail );
	}
	
	public static void Swap<T>( this IList<T> list, int indexA, int indexB, int range = 1, bool silentFail = false ) {
		if( indexA == indexB ) { return; }	// NOP
		
		if( silentFail ) {
			var bound = list.Count - range;
			if( indexA < 0 || indexA > bound ) { return; }
			if( indexB < 0 || indexB > bound ) { return; }
		}
		
		var held = list[indexA];
		list[indexA] = list[indexB];
		list[indexB] = held;
	}
	
	public static void HalfSwap<T>( this IList<T> list, int deadIndex, int aliveIndex, int range = 1, bool silentFail = false ) {
		if( deadIndex == aliveIndex ) { return; }	// NOP
		
		if( silentFail ) {
			var bound = list.Count - range;
			if( deadIndex < 0 || deadIndex > bound ) { return; }
			if( aliveIndex < 0 || aliveIndex > bound ) { return; }
		}
		
		for( var i = 0; i < range; i++ ) {
			list[deadIndex + i] = list[aliveIndex + i];
		}
	}
#endregion
	
	
#region Removes
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
		var hasPrinter = (printer != null);
		var canBeNull = (default( T ) == null);
		var counter = 0;
		
		foreach( var item in list ) {
			if( canBeNull && item.Equals( default( T ) ) ) {
				builder.Append( "-null-" );
			}
			else {
				builder.Append( hasPrinter ? printer( item ) : item.ToString() );
			}
			builder.Append( separator );
			
			counter += 1;
		}
		
		if( counter > 0 ) { builder.TrimEnd( separator.Length ); }
		return "("+counter+")"+separator+builder.ToString();
	}
	
	public static string Dump<TKey, TValue>( this Dictionary<TKey, TValue> dictionary, System.Func<TValue, string> printer, string separator = "\n" ) {
		return dictionary.Dump( separator, printer );
	}
	public static string Dump<TKey, TValue>( this Dictionary<TKey, TValue> dictionary, string separator = "\n", System.Func<TValue, string> printer = null ) {
		var builder = new System.Text.StringBuilder();
		var hasPrinter = (printer != null);
		var canBeNull = (default( TValue ) == null);
		var counter = 0;
		
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
			
			counter += 1;
		}
		
		if( counter > 0 ) { builder.TrimEnd( separator.Length ); }
		return "("+counter+")"+separator+builder.ToString();
	}
#endregion
	
	
#region Temporary
#endregion
}
