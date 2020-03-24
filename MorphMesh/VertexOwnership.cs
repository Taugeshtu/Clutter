using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Clutter.Mesh {
// Transient internal structure: get it, mod it & forget it!
internal struct VertexOwnership : IEnumerable<int>, IEnumerable, IEquatable<VertexOwnership> {
	public const int c_ownersFast = 10;
	
	private static List<int> t_newOwners = new List<int>( c_ownersFast *2 );	// reusable utility container
	
	private MorphMesh m_mesh;
	
	private int _fastIndex { get { return Index *c_ownersFast; } }
	private List<int> _ownersCount { get { return m_mesh.m_ownersCount; } }
	private List<int> _ownersFast { get { return m_mesh.m_ownersFast; } }
	private Dictionary<int, HashSet<int>> _ownersExt { get { return m_mesh.m_ownersExt; } }
	private HashSet<int> _myOwnersExt { get { return _ownersExt.GetAt( Index, null ); } }
	private HashSet<int> _myOwnersExtSpawned {
		get {
			var myOwnersExt = _myOwnersExt;
			if( myOwnersExt == null ) {
				myOwnersExt = new HashSet<int>();
				_ownersExt[Index] = myOwnersExt;
			}
			return myOwnersExt;
		}
	}
	
	public long Generation;
	public int Index;
	
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
		
		var extOwners = _myOwnersExt;
		if( extOwners != null ) {
			foreach( var extOwner in extOwners ) {
				yield return extOwner;
			}
		}
	}
	
	public override bool Equals( object other ) {
		if( other is VertexOwnership ) {
			return Equals( (VertexOwnership) other );
		}
		return false;
	}
	public bool Equals( VertexOwnership other ) {
		return (Generation == other.Generation) && (Index == other.Index) && (m_mesh == other.m_mesh);
	}
	public override int GetHashCode() {
		return (int) Generation + 23 *Index;
	}
	
	public static bool operator ==( VertexOwnership a, VertexOwnership b ) {
		return a.Equals( b );
	}
	public static bool operator !=( VertexOwnership a, VertexOwnership b ) {
		return !a.Equals( b );
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
		if( Index == MorphMesh.c_invalidID ) { return; }
		
		var ownersCount = OwnersCount;
		
		if( ownersCount < c_ownersFast ) {
			_ownersFast[_fastIndex + ownersCount] = triangleID;
		}
		else {
			_myOwnersExtSpawned.Add( triangleID );
		}
		
		OwnersCount = ownersCount + 1;
	}
	
	internal void RemapOwner( int oldIndex, int newIndex ) {
		if( Index == MorphMesh.c_invalidID ) { return; }
		
		t_newOwners.Clear();
		foreach( var owner in this ) {
			if( owner == oldIndex ) {
				if( newIndex != MorphMesh.c_invalidID ) {
					t_newOwners.Add( newIndex );
				}
			}
			else if( owner == newIndex ) {
				// do nothing; this is a dead owner and shouldn't be here
			}
			else {
				t_newOwners.Add( owner );
			}
		}
		
		var ownersCount = t_newOwners.Count;
		var fastLimit = Mathf.Min( ownersCount, c_ownersFast );
		var fastIndex = _fastIndex;
		
		for( var ownerIndex = 0; ownerIndex < fastLimit; ownerIndex++ ) {
			_ownersFast[fastIndex + ownerIndex] = t_newOwners[ownerIndex];
		}
		
		if( ownersCount <= c_ownersFast ) {
			_ownersExt.Remove( Index );
		}
		else {
			var myOwnersExt = _myOwnersExtSpawned;
			myOwnersExt.Clear();
			for( var ownerIndex = fastLimit; ownerIndex < ownersCount; ownerIndex++ ) {
				myOwnersExt.Add( t_newOwners[ownerIndex] );
			}
		}
		
		OwnersCount = t_newOwners.Count;
	}
	
	internal void MoveOwnershipFrom( ref VertexOwnership other ) {
		var destIndex = Index;
		var sourceIndex = other.Index;
		
		_ownersCount.HalfSwap( destIndex, sourceIndex );
		_ownersFast.HalfSwap( _fastIndex, other._fastIndex, c_ownersFast );
		
		if( _ownersExt.ContainsKey( sourceIndex ) ) {
			_ownersExt[destIndex] = _ownersExt[sourceIndex];
			_ownersExt.Remove( sourceIndex );
		}
		else {
			_ownersExt.Remove( destIndex );
		}
	}
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}
}
