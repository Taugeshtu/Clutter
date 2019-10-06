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
	private List<Vector3> m_positions = new List<Vector3>( c_initialVertexCapacity );
	
	private List<VertexOwnership> m_vertexOwnership = new List<VertexOwnership>( c_initialVertexCapacity );
	private List<int> m_ownershipFast = new List<int>( c_initialVertexCapacity *Vertex.c_ownersFast );	// Note: not sure how worth it this optimization is
	// - - - - - - - - - - - - - -
	
	// - - - - TRIANGLE DATA - - - -
	private Mapping<int, int> m_trianglesMap = new Mapping<int, int>( c_initialVertexCapacity /3 );
	private List<int> m_triangles = new List<int>( c_initialVertexCapacity );	// index buffer, basically
	// - - - - - - - - - - - - - - -
	
	private long m_generation = 0;	// should never be reset!
	private int m_topVertexID = c_invalidID;
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
		m_vertexOwnership.Clear();
		m_ownershipFast.Clear();
		
		m_trianglesMap.Clear();
		m_triangles.Clear();
		
		// Note: not clearing out generation in here!!
		m_topVertexID = c_invalidID;
		m_topTriangleID = c_invalidID;
	}
	
	public string Dump() {
		var result = new System.Text.StringBuilder();
		result.Append( "Mesh data" );
		result.Append( "\n" );
		result.Append( "Vertices: " );
		result.Append( m_positions.Count );
		
		m_vertexOwnership.PadUpTo( m_positions.Count );
		for( var i = 0; i < m_vertexOwnership.Count; i++ ) {
			var ownershipData = m_vertexOwnership[i];
			result.Append( "\n" );
			result.Append( ownershipData.ToString() );
		}
		
		result.Append( "\n" );
		result.Append( "Triangles: " );
		result.Append( m_trianglesMap.Count );
		result.Append( "\n" );
		result.Append( m_trianglesMap.Dump() );
		
		result.Append( "\n" );
		result.Append( "Indeces: " );
		result.Append( "\n" );
		result.Append( m_triangles.Dump() );
		
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
		mesh.GetTriangles( m_triangles, 0 );
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
		mesh.SetTriangles( m_triangles, 0 );
		
		// Debug.LogError( "Vertices: "+m_positions.Count+", triangles: "+m_triangles.Count+", in map: "+m_trianglesMap.Count );
		mesh.RecalculateNormals();
	}
#endregion
	
	
#region Vertex ops
	public Vertex EmitVertex( Vector3 position ) {
		m_topVertexID += 1;
		var vertex = new Vertex( m_generation, m_topVertexID, position );
		return vertex;
	}
	
	public void PushVertex( Vertex vertex ) {
		if( vertex.Generation != m_generation ) {
			Debug.LogWarning( "Trying to add a vertex from another generation! Vertex: "+vertex.Generation+", now is "+m_generation );
			return;
		}
		
		var vertexIndex = vertex.Index;
		m_positions.PadUpTo( vertexIndex + 1 );
		m_positions[vertexIndex] = vertex.Position;
		
		m_vertexOwnership.PadUpTo( vertexIndex + 1 );
		m_vertexOwnership[vertexIndex] = new VertexOwnership( ref vertex, m_ownershipFast );
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
		var triangle = new Triangle( m_generation, m_topTriangleID, ref vA, ref vB, ref vC );
		return triangle;
	}
	
	public void PushTriangle( Triangle triangle ) {
		if( triangle.Generation != m_generation ) {
			Debug.LogWarning( "Trying to add a triangle from another generation! Triangle: "+triangle.Generation+", now is "+m_generation );
			return;
		}
		
		PushVertex( triangle.A );
		PushVertex( triangle.B );
		PushVertex( triangle.C );
		
		// This will put the data either in existing index/slot, OR create a new slot & pad up the index buffer for it
		var triangleIndex = m_triangles.Count /3;
		if( m_trianglesMap.ContainsKey( triangle.ID ) ) {
			triangleIndex = m_trianglesMap[triangle.ID];
		}
		else {
			m_trianglesMap.Add( triangle.ID, triangleIndex );
		}
		
		var indexIndex = triangleIndex *3;	// Confusing, but: index in index buffer
		m_triangles.PadUpTo( indexIndex + 3 );
		m_triangles[indexIndex + 0] = triangle.A.Index;
		m_triangles[indexIndex + 1] = triangle.B.Index;
		m_triangles[indexIndex + 2] = triangle.C.Index;
	}
#endregion
	
	
#region Private
	private void _RebuildTriangleData() {
		var trianglesCount = m_triangles.Count /3;
		for( var i = 0; i < trianglesCount; i++ ) {
			m_trianglesMap.Add( i, i );
		}
		
		m_vertexOwnership.PadUpTo( m_positions.Count );
		m_ownershipFast.PadUpTo( m_positions.Count *Vertex.c_ownersFast );
		
		for( var triangleIndex = 0; triangleIndex < trianglesCount; triangleIndex++ ) {
			var triangleID = m_trianglesMap.GetByValue( triangleIndex );
			for( var i = 0; i < 3; i++ ) {
				var vertexIndex = m_triangles[triangleIndex *3 + i];
				_UpdateOwnership( vertexIndex, triangleID, true );
			}
		}
	}
	
	private void _UpdateOwnership( int vertexIndex, int triangleID, bool isOwned ) {
		var ownershipData = m_vertexOwnership[vertexIndex];
		if( !ownershipData.IsInitialized ) {
			ownershipData = new VertexOwnership( vertexIndex, m_ownershipFast );
		}
		
		if( isOwned ) {
			ownershipData.RegisterOwner( triangleID );
		}
		else {
			ownershipData.UnRegisterOwner( triangleID );
		}
		
		m_vertexOwnership[vertexIndex] = ownershipData;
	}
	
	private void _Compactify() {
		_CompactifyTriangles();
		_CompactifyVertices();
		
		// TODO: linerialize IDs and indeces!
		
		m_generation += 1;
		// TODO: m_topVertexID, m_topTriangleID
	}
	
	private void _CompactifyTriangles() {
		var trianglesCount = m_triangles.Count /3;
		var lastAliveIndex = trianglesCount - 1;
		var index = 0;
		while( index <= lastAliveIndex ) {
			var indexIndex = index *3;
			var isDead = (m_triangles[indexIndex] == c_invalidID);
			isDead = (m_triangles[indexIndex + 1] == c_invalidID) || isDead;
			isDead = (m_triangles[indexIndex + 2] == c_invalidID) || isDead;
			
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
		m_triangles.RemoveRange( firstDeadIndex *3, itemsToRemove *3 );
	}
	
	private void _DestroyTriangle( int deadIndex, int aliveIndex ) {
		var deadID = m_trianglesMap.GetByValue( deadIndex );
		var aliveID = m_trianglesMap.GetByValue( aliveIndex );
		
		for( var i = 0; i < 3; i++ ) {
			var vertexIndex = m_triangles[deadIndex *3 + i];
			_UpdateOwnership( vertexIndex, deadID, true );
		}
		
		m_triangles.HalfSwap( deadIndex *3, aliveIndex *3, 3 );
		m_trianglesMap[deadID] = aliveIndex;
		m_trianglesMap[aliveID] = deadIndex;
	}
	
	private void _CompactifyVertices() {
		var vertexCount = m_positions.Count;
		var lastAliveIndex = vertexCount - 1;
		var index = 0;
		while( index <= lastAliveIndex ) {
			var isDead = (m_vertexOwnership[index].OwnersCount == 0);
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
		m_ownershipFast.RemoveRange( firstDeadIndex *Vertex.c_ownersFast, itemsToRemove *Vertex.c_ownersFast );
	}
	
	private void _DestroyVertex( int deadIndex, int aliveIndex ) {
		m_positions.HalfSwap( deadIndex, aliveIndex );
		m_ownershipFast.HalfSwap( deadIndex *Vertex.c_ownersFast, aliveIndex *Vertex.c_ownersFast, Vertex.c_ownersFast );
		// TODO: also other data, should it arise!
		
		var newOwnership = m_vertexOwnership[aliveIndex];
		newOwnership.UpdateIndex( deadIndex );
		_MoveVertexInIndeces( aliveIndex, ref newOwnership );
		m_vertexOwnership[deadIndex] = newOwnership;
	}
	
	private void _MoveVertexInIndeces( int oldIndex, ref VertexOwnership ownership ) {
		for( var ownerIndex = 0; ownerIndex < ownership.OwnersCount; ownerIndex++ ) {
			var triangleID = ownership[ownerIndex];
			var triangleIndex = m_trianglesMap[triangleID];
			for( var i = 0; i < 3; i++ ) {
				var vertexIndex = m_triangles[triangleIndex *3 + i];
				if( vertexIndex == oldIndex ) {
					m_triangles[triangleIndex *3 + i] = ownership.Index;
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
#endregion
}
}
