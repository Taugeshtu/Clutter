using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Clutter {
public static class BlockParser {
	public struct Block {
		public string source;
		public int start;
		public int end;
		
		public string Extracted => (start == -1 || end == -1) ? null : source.Substring( start, end - start );
		
		public Block( string source ) : this( source, -1, -1 ) {}
		public Block( string source, int start, int end ) {
			this.source = source;
			this.start = start;
			this.end = end;
		}
	}
	
	public static Block Extract( string source, string blockStart, string blockEnd ) {
		var startIndex = source.IndexOf( blockStart );
		if( startIndex == -1 )
			return new Block( source );
		
		var endIndex = source.LastIndexOf( blockEnd, source.Length - 1, source.Length - (startIndex + blockStart.Length) );
		if( endIndex == -1 )
			return new Block( source );
		
		var contentStartIndex = startIndex + blockStart.Length;
		return new Block( source, contentStartIndex, endIndex );
	}
	
	public static IEnumerable<Block> ExtractAll( string source, string blockStart, string blockEnd ) {
		var starts = source.AllIndicesOf( blockStart );
		var ends = source.AllIndicesOf( blockEnd );
		
		// Reconciliation
		var markers = new List<(int index, bool isStart)>();
		markers.AddRange( starts.Select( index => (index, true) ) );
		markers.AddRange( ends.Select( index => (index, false) ) );
		markers.Sort( (a, b) => a.index.CompareTo( b.index ) );
		
		// Filtering markers to produce biggest possible top-level spans
		var filteredBlocks = new List<int>();	// alternating start, end indexes
		var previousIsStart = false;
		foreach( var marker in markers ) {
			if( marker.isStart ) {
				if( !previousIsStart ) {
					filteredBlocks.Add( marker.index );
					previousIsStart = true;
				}
			}
			else {
				if( filteredBlocks.Count == 0 ) continue;
				if( previousIsStart ) {
					filteredBlocks.Add( marker.index );
				}
				else {
					filteredBlocks[filteredBlocks.Count - 1] = marker.index;
				}
				previousIsStart = false;
			}
		}
		
		// Producing Blocks
		for( var i = 0; i < filteredBlocks.Count - 1; i += 2 ) {
			var startIndex = filteredBlocks[i] + blockStart.Length;
			var endIndex = filteredBlocks[i + 1];
			yield return new Block( source, startIndex, endIndex );
		}
	}
}
}
