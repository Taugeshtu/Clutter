using UnityEngine;
using System.Collections.Generic;

namespace Clutter.Mesh {

/* Notes:
 - Vertices are addressed via Index
 - Triangles - via ID; internally treated via indexes

*/

public class MorphMesh {
	public const int c_invalidID = -1;
	private const int c_initialVertexCapacity = 300;
	
	// - - - - VERTEX DATA - - - -
	internal List<Vector3> m_positions = new List<Vector3>( c_initialVertexCapacity );
	
	internal List<int> m_ownersCount = new List<int>( c_initialVertexCapacity );
	internal List<int> m_ownersFast = new List<int>( c_initialVertexCapacity *VertexOwnership.c_ownersFast );	// Note: not sure how worth it this optimization is
	internal Dictionary<int, HashSet<int>> m_ownersExt = new Dictionary<int, HashSet<int>>();
	// - - - - - - - - - - - - - -
	
	// - - - - TRIANGLE DATA - - - -
	internal Mapping<int, int> m_trianglesMap = new Mapping<int, int>( c_initialVertexCapacity /3 );	// ID <-> triangleIndex
	internal List<int> m_indeces = new List<int>( c_initialVertexCapacity );	// index buffer, basically
	// - - - - - - - - - - - - - - -
	
	internal long m_generation = 0;	// should never be reset!
	private int m_topVertexIndex = c_invalidID;
	private int m_topTriangleID = c_invalidID;
	
	public MeshFilter Target { get; private set; }
	
#region Implementation
	public MorphMesh() {}
	
	public MorphMesh( GameObject target ) : this( target.transform ) {}
	
	public MorphMesh( Component target ) {
		Target = _GetFilterTarget( target );
	}
#endregion
	
	
#region General
	public void Clear() {
		m_positions.Clear();
		
		m_ownersCount.Clear();
		m_ownersFast.Clear();
		m_ownersExt.Clear();
		
		m_trianglesMap.Clear();
		m_indeces.Clear();
		
		// Note: not clearing out generation in here!!
		m_topVertexIndex = c_invalidID;
		m_topTriangleID = c_invalidID;
	}
	
	public string Dump() {
		var result = new System.Text.StringBuilder();
		result.Append( "Mesh data" );
		result.Append( "\n" );
		result.Append( "Vertices: " );
		result.Append( m_positions.Count );
		
		m_ownersCount.PadUpTo( m_positions.Count );
		m_ownersFast.PadUpTo( (m_positions.Count + 1) *VertexOwnership.c_ownersFast, -1 );
		for( var i = 0; i < m_ownersCount.Count; i++ ) {
			var ownershipData = _MakeOwnershipData( i );
			result.Append( "\n" );
			result.Append( ownershipData.ToString() );
		}
		
		result.Append( "\n" );
		result.Append( "Triangles: " );
		result.Append( m_trianglesMap.Dump() );
		
		result.Append( "\n" );
		result.Append( "Indeces: " );
		result.Append( "\n" );
		result.Append( m_indeces.Dump() );
		
		return result.ToString();
	}
	
	public void Log() {
		Debug.Log( Dump() );
	}
#endregion
	
	
#region Mesh ops
	public void Read( GameObject target ) {
		Read( target.transform );
	}
	
	public void Read( Component target = null ) {
		var filterTarget = _GetFilterTarget( target );
		if( filterTarget == null ) { return; }
		
		Clear();
		var mesh = filterTarget.mesh;
		mesh.GetTriangles( m_indeces, 0 );
		mesh.GetVertices( m_positions );
		
		_RebuildTriangleData();
	}
	
	public void Write( GameObject target ) {
		Write( target.transform );
	}
	
	public void Write( Component target = null ) {
		var filterTarget = _GetFilterTarget( target );
		if( filterTarget == null ) { return; }
		
		_Compactify();
		var mesh = filterTarget.mesh;
		mesh.Clear();
		mesh.SetVertices( m_positions );
		mesh.SetTriangles( m_indeces, 0 );
		
		// Debug.LogError( "Vertices: "+m_positions.Count+", triangles: "+m_triangles.Count+", in map: "+m_trianglesMap.Count );
		mesh.RecalculateNormals();
	}
#endregion
	
	
#region Vertex ops
	public Vertex EmitVertex( Vector3 position ) {
		m_topVertexIndex += 1;
		
		m_positions.Add( position );
		// TODO: other porperties
		m_ownersCount.Add( 0 );
		m_ownersFast.PadUpTo( (m_topVertexIndex + 1) *VertexOwnership.c_ownersFast, c_invalidID );
		
		var vertex = new Vertex( this, m_topVertexIndex );
		return vertex;
	}
	
	public Vertex GetVertex( int index ) {
		return new Vertex( this, index );
	}
#endregion
	
	
#region Triangle ops
	public Triangle EmitTriangle( Vector3 a, Vector3 b, Vector3 c ) {
		var vA = EmitVertex( a );
		var vB = EmitVertex( b );
		var vC = EmitVertex( c );
		
		return EmitTriangle( ref vA, ref vB, ref vC );
	}
	
	public Triangle EmitTriangle( ref Vertex vA, ref Vertex vB, ref Vertex vC ) {
		m_topTriangleID += 1;
		
		var triangleIndex = m_indeces.Count /3;
		m_trianglesMap.Add( m_topTriangleID, triangleIndex );
		m_indeces.Add( vA.Index, vB.Index, vC.Index );
		
		var triangle = new Triangle( this, m_topTriangleID );
		triangle.SetVertices( ref vA, ref vB, ref vC );	// this op registers ownership over vertices
		return triangle;
	}
	
	public Triangle GetTriangle( int id ) {
		return new Triangle( this, id );
	}
#endregion
	
	
#region Private
	private void _RebuildTriangleData() {
		// Note: here we KNOW FOR CERTAIN our m_trianglesMap is linear
		var trianglesCount = m_indeces.Count /3;
		for( var i = 0; i < trianglesCount; i++ ) {
			m_trianglesMap.Add( i, i );
		}
		
		m_ownersCount.PadUpTo( m_positions.Count );
		m_ownersFast.PadUpTo( m_positions.Count *VertexOwnership.c_ownersFast );
		
		for( var triangleIndex = 0; triangleIndex < trianglesCount; triangleIndex++ ) {
			var indexShift = triangleIndex *3;
			_MakeOwnershipData( m_indeces[indexShift + 0] ).AddOwner( triangleIndex );
			_MakeOwnershipData( m_indeces[indexShift + 1] ).AddOwner( triangleIndex );
			_MakeOwnershipData( m_indeces[indexShift + 2] ).AddOwner( triangleIndex );
		}
	}
	
	private void _Compactify() {
		// Log();
		
		_CompactifyTriangles();
		_CompactifyVertices();
		
		// TODO: linerialize IDs and indeces!
		
		m_generation += 1;
		// TODO: m_topVertexID, m_topTriangleID
	}
	
	private void _CompactifyTriangles() {
		var trianglesCount = m_indeces.Count /3;
		var lastAliveIndex = trianglesCount - 1;
		var index = 0;
		while( index <= lastAliveIndex ) {
			var indexIndex = index *3;
			var isDead = (m_indeces[indexIndex] == c_invalidID);
			isDead = (m_indeces[indexIndex + 1] == c_invalidID) || isDead;
			isDead = (m_indeces[indexIndex + 2] == c_invalidID) || isDead;
			
			if( isDead ) {
				_DestroyTriangle( index, lastAliveIndex );
				lastAliveIndex -= 1;
			}
			else {
				index += 1;
			}
		}
		
		var firstDeadIndex = lastAliveIndex + 1;
		var itemsToRemove = trianglesCount - firstDeadIndex;
		m_indeces.RemoveRange( firstDeadIndex *3, itemsToRemove *3 );
	}
	
	private void _DestroyTriangle( int deadIndex, int aliveIndex ) {
		var deadID = m_trianglesMap.GetByValue( deadIndex );
		var aliveID = m_trianglesMap.GetByValue( aliveIndex );
		
		for( var i = 0; i < 3; i++ ) {
			var vertexIndex = m_indeces[deadIndex *3 + i];
			var ownershipData = _MakeOwnershipData( vertexIndex );
			ownershipData.RemoveOwner( deadID );
		}
		
		m_indeces.HalfSwap( deadIndex *3, aliveIndex *3, 3 );
		m_trianglesMap[deadID] = aliveIndex;
		m_trianglesMap[aliveID] = deadIndex;
	}
	
	private void _CompactifyVertices() {
		var vertexCount = m_positions.Count;
		var lastAliveIndex = vertexCount - 1;
		var index = 0;
		while( index <= lastAliveIndex ) {
			var isDead = (m_ownersCount[index] == 0);
			if( isDead ) {
				_DestroyVertex( index, lastAliveIndex );
				lastAliveIndex -= 1;
			}
			else {
				index += 1;
			}
		}
		
		var firstDeadIndex = lastAliveIndex + 1;
		var itemsToRemove = vertexCount - firstDeadIndex;
		m_positions.RemoveRange( firstDeadIndex, itemsToRemove );
		m_ownersFast.RemoveRange( firstDeadIndex *VertexOwnership.c_ownersFast, itemsToRemove *VertexOwnership.c_ownersFast );
	}
	
	private void _DestroyVertex( int deadIndex, int aliveIndex ) {
		m_positions.HalfSwap( deadIndex, aliveIndex );
		// TODO: also other data, should it arise!
		
		var deadOwner = _MakeOwnershipData( deadIndex );
		var aliveOwner = _MakeOwnershipData( aliveIndex );
		deadOwner.CopyOwnershipFrom( ref aliveOwner );
		
		_MoveVertexInIndeces( aliveIndex, ref deadOwner );
	}
	
	private void _MoveVertexInIndeces( int oldIndex, ref VertexOwnership ownership ) {
		foreach( var ownerID in ownership ) {
			var triangleIndex = m_trianglesMap[ownerID];
			for( var i = 0; i < 3; i++ ) {
				var vertexIndex = m_indeces[triangleIndex *3 + i];
				if( vertexIndex == oldIndex ) {
					m_indeces[triangleIndex *3 + i] = ownership.Index;
				}
			}
		}
	}
#endregion
	
	
#region Utility
	private MeshFilter _GetFilterTarget( Component target ) {
		if( target == null ) {
			if( Target == null ) {
				Debug.LogWarning( "Tried to get filter target, but both specified and set beforehand targets are null!" );
			}
			return Target;
		}
		
		var filterTarget = target as MeshFilter;
		if( filterTarget == null ) {
			filterTarget = target.GetComponent<MeshFilter>();
		}
		
		if( filterTarget == null ) {
			Debug.LogWarning( "Tried to find filter target on '"+target.gameObject.name+"', but no MeshFilter was found there!" );
		}
		return filterTarget;
	}
	
	private VertexOwnership _MakeOwnershipData( int vertexIndex ) {
		return new VertexOwnership( this, vertexIndex );
	}
#endregion
}
}
