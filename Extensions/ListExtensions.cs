using UnityEngine;
using System.Collections.Generic;
using System.Text;

public static class ListExtensions {
	
#region List padding
	public static void PadUpTo<T>( this IList<T> list, int totalCount, T template = default( T ) ) {
		var padAmount = totalCount - list.Count;
		if( padAmount > 0 ) {
			list.Pad( padAmount, template );
		}
	}
	
	public static void Pad<T>( this IList<T> list, int count, T template = default( T ) ) {
		for( var i = 0; i < count; i++ ) {
			list.Add( template );
		}
	}
#endregion
	
	
#region At-s
	public static T GetAt<T>( this IList<T> list, int index, T template = default( T ) ) {
		if( index < 0 ) { return template; }
		if( index >= list.Count ) { return template; }
		return list[index];
	}
	
	public static void SetAt<T>( this IList<T> list, int index, T item, T paddingTemplate = default( T ) ) {
		if( index == list.Count ) {
			list.Add( item );
			return;
		}
		
		list.PadUpTo( index + 1, paddingTemplate );
		list[index] = item;
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
	
	
#region Adds
	// Oh I'm a clever boi...
	public static void Add<T>( this ICollection<T> collection, params T[] items ) {
		foreach( var item in items ) {
			collection.Add( item );
		}
	}
#endregion
	
	
#region Removes
	public static void Remove<T>( this ICollection<T> collection, params T[] items ) {
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
	
	
#region Temporary
#endregion
}
