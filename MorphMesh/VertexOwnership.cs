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
	
	public long Generation;
	public int Index;
	
	public int OwnersCount {
		get { return m_mesh.m_ownersCount[Index]; }
		private set { m_mesh.m_ownersCount[Index] = value; }
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
				yield return m_mesh.m_ownersFast[fastIndex + ownerIndex];
			}
			else {
				break;
			}
		}
		
		if( m_mesh.m_ownersExt.ContainsKey( Index ) ) {
			var extOwners = m_mesh.m_ownersExt[Index];
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
		
		var ownersCount = m_mesh.m_ownersCount[Index];
		
		if( ownersCount < c_ownersFast ) {
			m_mesh.m_ownersFast[_fastIndex + ownersCount] = triangleID;
		}
		else {
			_AddToExt( triangleID );
		}
		
		m_mesh.m_ownersCount[Index] = ownersCount + 1;
	}
	
	internal void RemoveOwner( int triangleID ) {
		if( Index == MorphMesh.c_invalidID ) { return; }
		if( triangleID == MorphMesh.c_invalidID ) { return; }
		
		var ownersCount = OwnersCount;
		var fastLimit = Mathf.Min( ownersCount, c_ownersFast );
		var fastIndex = _fastIndex;
		
		for( var ownerIndex = 0; ownerIndex < fastLimit; ownerIndex++ ) {
			var candidateID = m_mesh.m_ownersFast[fastIndex + ownerIndex];
			if( candidateID == triangleID ) {
				m_mesh.m_ownersFast.HalfSwap( fastIndex + ownerIndex, fastIndex + c_ownersFast - 1 );
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
	
	internal void RemapOwners( Dictionary<int, int> ownersMapping ) {
		t_newOwners.Clear();
		var hasChanges = false;
		
		foreach( var ownerID in this ) {
			var newID = ownersMapping[ownerID];
			if( newID != ownerID ) {
				hasChanges = true;
			}
			if( newID != MorphMesh.c_invalidID ) {
				t_newOwners.Add( newID );
			}
		}
		
		if( !hasChanges ) { return; }
		
		OwnersCount = 0;
		m_mesh.m_ownersExt.Remove( Index );
		
		foreach( var newOwner in t_newOwners ) {
			AddOwner( newOwner );
		}
	}
	
	internal void MoveOwnershipFrom( ref VertexOwnership other ) {
		var destIndex = Index;
		var sourceIndex = other.Index;
		
		m_mesh.m_ownersCount.HalfSwap( destIndex, sourceIndex );
		m_mesh.m_ownersFast.HalfSwap( _fastIndex, other._fastIndex, c_ownersFast );
		
		if( m_mesh.m_ownersExt.ContainsKey( sourceIndex ) ) {
			m_mesh.m_ownersExt[destIndex] = m_mesh.m_ownersExt[sourceIndex];
			m_mesh.m_ownersExt.Remove( sourceIndex );
		}
		else {
			m_mesh.m_ownersExt.Remove( destIndex );
		}
	}
#endregion
	
	
#region Private
	private void _AddToExt( int triangleID ) {
		HashSet<int> extOwners;
		if( !m_mesh.m_ownersExt.ContainsKey( Index ) ) {
			extOwners = new HashSet<int>();
			m_mesh.m_ownersExt[Index] = extOwners;
		}
		else {
			extOwners = m_mesh.m_ownersExt[Index];
		}
		
		extOwners.Add( triangleID );
	}
	
	private bool _RemoveFromExt( int triangleID ) {
		var removed = false;
		if( m_mesh.m_ownersExt.ContainsKey( Index ) ) {
			var extOwners = m_mesh.m_ownersExt[Index];
			removed = extOwners.Remove( triangleID );
			if( removed ) {
				if( extOwners.Count == 0 ) {
					m_mesh.m_ownersExt.Remove( Index );
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
