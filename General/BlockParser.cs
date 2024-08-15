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
		
		public bool IsValid => (start >=0 && end >= 0);
		public string Extracted => (IsValid) ? source.Substring( start, end - start ) : null;
		public int Length => (end - start);
		
		public static Block Invalid => new Block( null, -1, -1 );
		public Block( string source ) : this( source, 0, source.Length ) {}
		public Block( string source, int start, int end ) {
			this.source = source;
			this.start = start;
			this.end = end;
		}
		
		public override string ToString() {
			return $"[{start}:{end}] {Extracted}";
		}
	}
	
	public static Block Extract( string source, string blockStart, string blockEnd ) {
		return new Block( source ).Extract( blockStart, blockEnd );
	}
	public static Block Extract( this Block source, string blockStart, string blockEnd ) {
		var startIndex = source.source.IndexOf( blockStart, source.start, source.end - source.start );
		if( startIndex == -1 )
			return Block.Invalid;
		
		var endIndex = source.source.LastIndexOf( blockEnd, source.end - 1, source.end - (startIndex + blockStart.Length) );
		if( endIndex == -1 )
			return Block.Invalid;
		
		var contentStartIndex = startIndex + blockStart.Length;
		return new Block( source.source, contentStartIndex, endIndex );
	}
	
	public static IEnumerable<Block> ExtractAll( string source, string blockStart, string blockEnd ) {
		return new Block( source ).ExtractAll( blockStart, blockEnd );
	}
	public static IEnumerable<Block> ExtractAll( this Block source, string blockStart, string blockEnd ) {
		var sourceSTR = source.source;
		var starts = sourceSTR.AllIndicesOf( blockStart, source.start, source.end );
		var ends = sourceSTR.AllIndicesOf( blockEnd, source.start, source.end );
		
		// Reconciliation
		var markers = new List<(int index, bool isStart)>();
		markers.AddRange( starts.Select( index => (index, true) ) );
		markers.AddRange( ends.Select( index => (index, false) ) );
		markers.Sort( (a, b) => a.index.CompareTo( b.index ) );
		
		// Filtering markers to produce biggest possible top-level spans
		var filteredBlocks = new List<int>();   // alternating start, end indexes
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
			yield return new Block( sourceSTR, startIndex, endIndex );
		}
	}
}
}
