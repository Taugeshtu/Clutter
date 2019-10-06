using UnityEngine;
using System.Collections.Generic;

namespace Clutter.Mesh {
internal struct VertexOwnership {
	public bool IsInitialized;
	public int Index;
	public int OwnersCount;
	
	private List<int> m_ownersFast;
	private List<int> m_ownersExt;
	
	private int _fastIndex { get { return Index *Vertex.c_ownersFast; } }
	
	public int this[int ownerIndex] {
		get {
			if( ownerIndex < 0 ) { return MorphMesh.c_invalidID; }
			if( ownerIndex < Vertex.c_ownersFast ) { return m_ownersFast[_fastIndex + ownerIndex]; }
			
			if( m_ownersExt == null ) { return MorphMesh.c_invalidID; }
			var extIndex = ownerIndex - Vertex.c_ownersFast;
			if( m_ownersExt.Count < extIndex ) { return MorphMesh.c_invalidID; }
			return m_ownersExt[extIndex];
		}
		private set {
			if( ownerIndex < 0 ) { return; }
			if( ownerIndex < Vertex.c_ownersFast ) {
				m_ownersFast[_fastIndex + ownerIndex] = value;
				return;
			}
			
			if( m_ownersExt == null ) {
				m_ownersExt = new List<int>();
			}
			var extIndex = ownerIndex - Vertex.c_ownersFast;
			m_ownersExt.PadUpTo( extIndex + 1, -1 );
			m_ownersExt[extIndex] = value;
		}
	}
	
#region Implementation
	public VertexOwnership( ref Vertex vertex, List<int> ownersFast ) : this( vertex.Index, ownersFast ) {
		foreach( var triangleID in vertex.Triangles ) {
			if( triangleID == MorphMesh.c_invalidID ) { continue; }
			RegisterOwner( triangleID );
		}
	}
	public VertexOwnership( int vertexIndex, List<int> ownersFast ) {
		IsInitialized = true;
		m_ownersFast = ownersFast;
		m_ownersExt = null;
		
		Index = vertexIndex;
		OwnersCount = 0;
	}
	
	public override string ToString() {
		const string separator = ", ";
		
		if( !IsInitialized ) {
			return "-ininitialized-";
		}
		
		var result = new System.Text.StringBuilder( 100 );
		result.Append( "#" );
		result.Append( Index );
		result.Append( ", owners: (" );
		result.Append( OwnersCount );
		result.Append( ") " );
		for( var ownerIndex = 0; ownerIndex < OwnersCount; ownerIndex++ ) {
			var ownerID = this[ownerIndex];
			result.Append( "t" );
			result.Append( ownerID );
			result.Append( separator );
		}
		
		if( OwnersCount > 0 ) { result.TrimEnd( separator.Length ); }
		return result.ToString();
	}
#endregion
	
	
#region Public
	public void RegisterOwner( int triangleID ) {
		this[OwnersCount] = triangleID;
		OwnersCount += 1;
	}
	
	public void UnRegisterOwner( int triangleID ) {
		if( triangleID == MorphMesh.c_invalidID ) { return; }
		
		for( var ownerIndex = 0; ownerIndex < OwnersCount; ownerIndex++ ) {
			if( this[ownerIndex] == triangleID ) {
				_RemoveOwner( ownerIndex );
				return;
			}
		}
	}
	
	public void UpdateIndex( int newIndex ) {
		Index = newIndex;
	}
#endregion
	
	
#region Private
	private void _RemoveOwner( int ownerIndex ) {
		if( ownerIndex < Vertex.c_ownersFast - 1 ) {
			var deadIndex = _fastIndex + ownerIndex;
			var aliveIndex = _fastIndex + Vertex.c_ownersFast - 1;
			m_ownersFast.HalfSwap( deadIndex, aliveIndex );
		}
		if( ownerIndex >= Vertex.c_ownersFast ) {
			var extIndex = ownerIndex - Vertex.c_ownersFast;
			m_ownersExt.FastRemove( extIndex );
		}
		
		OwnersCount -= 1;
	}
#endregion
	
	
#region Temporary
#endregion
}
}
