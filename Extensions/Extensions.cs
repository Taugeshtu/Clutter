using UnityEngine;
using System.Collections.Generic;

public static class Extensions {
	
	public static void SwapBack<T>( this IList<T> list, int elementIndex, bool silentFail = false ) {
		var lastIndex = list.Count - 1;
		Swap( list, elementIndex, lastIndex, silentFail );
	}
	
	public static void Swap<T>( this IList<T> list, int indexA, int indexB, bool silentFail = false ) {
		if( silentFail ) {
			var bound = list.Count - 1;
			if( indexA < 0 || indexA > bound ) { return; }
			if( indexB < 0 || indexB > bound ) { return; }
		}
		
		var held = list[indexA];
		list[indexA] = list[indexB];
		list[indexB] = held;
	}
	
	public static void FastRemove<T>( this IList<T> list, int elementIndex, bool silentFail = false ) {
		var lastIndex = list.Count - 1;
		if( silentFail ) {
			if( elementIndex < 0 || elementIndex > lastIndex ) { return; }
			if( lastIndex < 0 ) { return; }
		}
		
		list[elementIndex] = list[lastIndex];
		list.RemoveAt( lastIndex );
	}
	
#region Temporary
#endregion
}
