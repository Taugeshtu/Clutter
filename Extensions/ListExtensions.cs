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
	
	
#region Temporary
#endregion
}
