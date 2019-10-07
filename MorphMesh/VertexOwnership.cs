using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Clutter.Mesh {
// Transient internal structure: get it, mod it & forget it!
// Doesn't need generations since it's EN-TI-RE-LY internal!
internal struct VertexOwnership : IEnumerable<int>, IEnumerable {
	public int Index;
	
	private List<int> m_ownersCount;
	private List<int> m_ownersFast;
	private Dictionary<int, HashSet<int>> m_ownersExt;
	
	private int _fastIndex { get { return Index *Vertex.c_ownersFast; } }
	
	public int OwnersCount {
		get {
			return m_ownersCount[Index];
		}
		private set {
			m_ownersCount[Index] = value;
		}
	}
	
#region Implementation
	public VertexOwnership( ref Vertex vertex, List<int> ownersCountList, List<int> ownersFast, Dictionary<int, HashSet<int>> ownersExt )
	: this( vertex.Index, ownersCountList, ownersFast, ownersExt ) {
		
		var ownersCount = 0;
		foreach( var triangleID in vertex.Triangles ) {
			if( triangleID == MorphMesh.c_invalidID ) { continue; }
			
			if( ownersCount < Vertex.c_ownersFast ) {
				m_ownersFast[_fastIndex + ownersCount] = triangleID;
			}
			else {
				_AddToExt( triangleID );
			}
			ownersCount += 1;
		}
		OwnersCount = ownersCount;
	}
	
	public VertexOwnership( int vertexIndex, List<int> ownersCount, List<int> ownersFast, Dictionary<int, HashSet<int>> ownersExt ) {
		m_ownersCount = ownersCount;
		m_ownersFast = ownersFast;
		m_ownersExt = ownersExt;
		
		Index = vertexIndex;
	}
	
	System.Collections.IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
	public IEnumerator<int> GetEnumerator() {
		var ownersCount = OwnersCount;
		var fastIndex = _fastIndex;
		for( var ownerIndex = 0; ownerIndex < ownersCount; ownerIndex++ ) {
			if( ownerIndex < Vertex.c_ownersFast ) {
				yield return fastIndex + ownerIndex;
			}
			else {
				break;
			}
		}
		
		if( m_ownersExt.ContainsKey( Index ) ) {
			var extOwners = m_ownersExt[Index];
			foreach( var extOwner in extOwners ) {
				yield return extOwner;
			}
		}
	}
	
	public override string ToString() {
		const string separator = ", ";
		
		var result = new System.Text.StringBuilder( 100 );
		result.Append( "#" );
		result.Append( Index );
		result.Append( ", owners: (" );
		result.Append( OwnersCount );
		result.Append( ") " );
		foreach( var ownerID in this ) {
			result.Append( "t" );
			result.Append( ownerID );
			result.Append( separator );
		}
		
		if( OwnersCount > 0 ) { result.TrimEnd( separator.Length ); }
		return result.ToString();
	}
#endregion
	
	
#region Public
	public void AddOwner( int triangleID ) {
		var ownersCount = OwnersCount;
		if( ownersCount < Vertex.c_ownersFast ) {
			m_ownersFast[_fastIndex + ownersCount] = triangleID;
			OwnersCount = ownersCount + 1;
			return;
		}
		
		_AddToExt( triangleID );
		OwnersCount = ownersCount + 1;
	}
	
	public void RemoveOwner( int triangleID ) {
		if( triangleID == MorphMesh.c_invalidID ) { return; }
		
		var ownersCount = OwnersCount;
		var fastLimit = Mathf.Min( ownersCount, Vertex.c_ownersFast );
		var fastIndex = _fastIndex;
		
		for( var ownerIndex = 0; ownerIndex < fastLimit; ownerIndex++ ) {
			var candidateID = m_ownersFast[fastIndex + ownerIndex];
			if( candidateID == triangleID ) {
				m_ownersFast.HalfSwap( fastIndex + ownerIndex, fastIndex + Vertex.c_ownersFast - 1 );
				OwnersCount = ownersCount - 1;
				return;
			}
		}
		
		if( ownersCount > Vertex.c_ownersFast ) {
			var removed = _RemoveFromExt( triangleID );
			if( removed ) {
				OwnersCount = ownersCount - 1;
			}
		}
	}
	
	public void CopyOwnershipFrom( ref VertexOwnership other ) {
		var deadIndex = Index;
		var aliveIndex = other.Index;
		
		m_ownersCount.HalfSwap( deadIndex, aliveIndex );
		m_ownersFast.HalfSwap( deadIndex *Vertex.c_ownersFast, aliveIndex *Vertex.c_ownersFast, Vertex.c_ownersFast );
		
		if( m_ownersExt.ContainsKey( aliveIndex ) ) {
			m_ownersExt[deadIndex] = m_ownersExt[aliveIndex];
			m_ownersExt.Remove( aliveIndex );
		}
		else {
			m_ownersExt.Remove( deadIndex );
		}
	}
#endregion
	
	
#region Private
	private void _AddToExt( int triangleID ) {
		HashSet<int> extOwners;
		if( !m_ownersExt.ContainsKey( Index ) ) {
			extOwners = new HashSet<int>();
			m_ownersExt[Index] = extOwners;
		}
		else {
			extOwners = m_ownersExt[Index];
		}
		
		extOwners.Add( triangleID );
	}
	
	private bool _RemoveFromExt( int triangleID ) {
		var removed = false;
		if( m_ownersExt.ContainsKey( Index ) ) {
			var extOwners = m_ownersExt[Index];
			removed = extOwners.Remove( triangleID );
			if( removed ) {
				if( extOwners.Count == 0 ) {
					m_ownersExt.Remove( Index );
				}
			}
		}
		return removed;
	}
#endregion
	
	
#region Temporary
#endregion
}
}
