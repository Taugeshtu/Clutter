using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Clutter.Mesh {
// Transient internal structure: get it, mod it & forget it!
internal struct VertexOwnership : IEnumerable<int>, IEnumerable {
	public const int c_ownersFast = 5;
	
	public long Generation;
	public int Index;
	
	private MorphMesh m_mesh;
	
	private int _fastIndex { get { return Index *c_ownersFast; } }
	private List<int> _ownersCount { get { return m_mesh.m_ownersCount; } }
	private List<int> _ownersFast { get { return m_mesh.m_ownersFast; } }
	private Dictionary<int, HashSet<int>> _ownersExt { get { return m_mesh.m_ownersExt; } }
	
	public int OwnersCount {
		get { return _ownersCount[Index]; }
		private set { _ownersCount[Index] = value; }
	}
	
	public bool IsValid {
		get {
			return (Index != MorphMesh.c_invalidID) && (Generation == m_mesh.m_generation);
		}
	}
	
#region Implementation
	internal VertexOwnership( MorphMesh mesh, ref Vertex vertex ) : this( mesh, vertex.Index ) {}
	internal VertexOwnership( MorphMesh mesh, int vertexIndex ) {
		m_mesh = mesh;
		Generation = mesh.m_generation;
		
		Index = vertexIndex;
	}
	
	System.Collections.IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
	public IEnumerator<int> GetEnumerator() {
		var ownersCount = OwnersCount;
		var fastIndex = _fastIndex;
		for( var ownerIndex = 0; ownerIndex < ownersCount; ownerIndex++ ) {
			if( ownerIndex < c_ownersFast ) {
				yield return _ownersFast[fastIndex + ownerIndex];
			}
			else {
				break;
			}
		}
		
		if( _ownersExt.ContainsKey( Index ) ) {
			var extOwners = _ownersExt[Index];
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
	internal void AddOwner( int triangleID ) {
		var ownersCount = OwnersCount;
		
		if( ownersCount < c_ownersFast ) {
			_ownersFast[_fastIndex + ownersCount] = triangleID;
		}
		else {
			_AddToExt( triangleID );
		}
		
		OwnersCount = ownersCount + 1;
	}
	
	internal void RemoveOwner( int triangleID ) {
		if( triangleID == MorphMesh.c_invalidID ) { return; }
		
		var ownersCount = OwnersCount;
		var fastLimit = Mathf.Min( ownersCount, c_ownersFast );
		var fastIndex = _fastIndex;
		
		for( var ownerIndex = 0; ownerIndex < fastLimit; ownerIndex++ ) {
			var candidateID = _ownersFast[fastIndex + ownerIndex];
			if( candidateID == triangleID ) {
				_ownersFast.HalfSwap( fastIndex + ownerIndex, fastIndex + c_ownersFast - 1 );
				OwnersCount = ownersCount - 1;
				return;
			}
		}
		
		if( ownersCount > c_ownersFast ) {
			var removed = _RemoveFromExt( triangleID );
			if( removed ) {
				OwnersCount = ownersCount - 1;
			}
		}
	}
	
	internal void CopyOwnershipFrom( ref VertexOwnership other ) {
		var deadIndex = Index;
		var aliveIndex = other.Index;
		
		_ownersCount.HalfSwap( deadIndex, aliveIndex );
		_ownersFast.HalfSwap( deadIndex *c_ownersFast, aliveIndex *c_ownersFast, c_ownersFast );
		
		if( _ownersExt.ContainsKey( aliveIndex ) ) {
			_ownersExt[deadIndex] = _ownersExt[aliveIndex];
			_ownersExt.Remove( aliveIndex );
		}
		else {
			_ownersExt.Remove( deadIndex );
		}
	}
#endregion
	
	
#region Private
	private void _AddToExt( int triangleID ) {
		HashSet<int> extOwners;
		if( !_ownersExt.ContainsKey( Index ) ) {
			extOwners = new HashSet<int>();
			_ownersExt[Index] = extOwners;
		}
		else {
			extOwners = _ownersExt[Index];
		}
		
		extOwners.Add( triangleID );
	}
	
	private bool _RemoveFromExt( int triangleID ) {
		var removed = false;
		if( _ownersExt.ContainsKey( Index ) ) {
			var extOwners = _ownersExt[Index];
			removed = extOwners.Remove( triangleID );
			if( removed ) {
				if( extOwners.Count == 0 ) {
					_ownersExt.Remove( Index );
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
