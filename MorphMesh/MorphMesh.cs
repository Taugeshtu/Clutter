using UnityEngine;
using System.Collections.Generic;

namespace Clutter.Mesh {

/* Notes:
 - Vertices are addressed via Index
 - Triangles - via ID; internally treated via indexes

 "m_vertexOwnership" layout: 
 - [0] == how many triangles share the vertex
 - [1-5] == triangles that take that vertex
 - excess goes into expansion structure
*/


public class MorphMesh {
	public const int c_invalidID = -1;
	private const int c_initialVertexCapacity = 300;
	
	
	// - - - - VERTEX DATA - - - -
	private List<Vector3> m_positions = new List<Vector3>( c_initialVertexCapacity );
	
	private List<int> m_vertexOwnership = new List<int>( c_initialVertexCapacity *(Vertex.c_ownersFast + 1) );	// Note: not sure how worth it this optimization is
	private Dictionary<int, List<int>> m_vertexOwnershipExt = new Dictionary<int, List<int>>();
	// - - - - - - - - - - - - - -
	
	// - - - - TRIANGLE DATA - - - -
	private Dictionary<int, int> m_trianglesMap = new Dictionary<int, int>( c_initialVertexCapacity /3 );
	private List<int> m_triangles = new List<int>( c_initialVertexCapacity );	// index buffer, basically
	// - - - - - - - - - - - - - - -
	
	private long m_generation = 0;	// should never be reset!
	private int m_topVertexID = c_invalidID;
	private int m_topTriangleID = c_invalidID;
	
	public MeshFilter Target { get; private set; }
	
	private int _ownershipDataSize { get { return Vertex.c_ownersFast + 1; } }
	
#region Implementation
	public MorphMesh() {}
	
	public MorphMesh( GameObject target ) : this( target.transform ) {}
	
	public MorphMesh( Component target ) {
		Target = _GetFilterTarget( target );
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
		
		m_vertexOwnership.PadUpTo( m_positions.Count *_ownershipDataSize, -1 );
		
		var trianglesCount = m_triangles.Count /3;
		for( var triangleIndex = 0; triangleIndex < trianglesCount; triangleIndex++ ) {
			for( var i = 0; i < 3; i++ ) {
				var vertexIndex = m_triangles[triangleIndex *3 + i];
				_RegisterOwnership( vertexIndex, triangleIndex );
			}
		}
	}
	
	public void Write( GameObject target ) {
		Write( target.transform );
	}
	
	public void Write( Component target = null ) {
		var filterTarget = _GetFilterTarget( target );
		if( filterTarget == null ) { return; }
		
		// _Compactify();
		var mesh = filterTarget.mesh;
		mesh.Clear();
		mesh.SetVertices( m_positions );
		mesh.SetTriangles( m_triangles, 0 );
		
		// Debug.LogError( "Vertices: "+m_positions.Count+", triangles: "+m_triangles.Count+", in map: "+m_trianglesMap.Count );
		mesh.RecalculateNormals();
	}
	
	public void Clear() {
		m_positions.Clear();
		m_vertexOwnership.Clear();
		m_vertexOwnershipExt.Clear();
		
		m_trianglesMap.Clear();
		m_triangles.Clear();
		
		// Note: not clearing out generation in here!!
		m_topVertexID = c_invalidID;
		m_topTriangleID = c_invalidID;
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
		
		var ownershipIndex = vertexIndex *_ownershipDataSize;
		m_vertexOwnership.PadUpTo( ownershipIndex + _ownershipDataSize );
		m_vertexOwnership[ownershipIndex] = -1;
		m_vertexOwnershipExt.Remove( vertexIndex );
		foreach( var triangleID in vertex.Triangles ) {
			_RegisterOwnership( vertexIndex, triangleID );
		}
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
	private void _RegisterOwnership( int vertexIndex, int triangleID ) {
		var ownershipIndex = vertexIndex *_ownershipDataSize;
		var currentKnownOwners = m_vertexOwnership[ownershipIndex];
		
		if( currentKnownOwners < 0 ) {
			currentKnownOwners = 0;
		}
		
		if( currentKnownOwners < Vertex.c_ownersFast ) {
			m_vertexOwnership[ownershipIndex + currentKnownOwners + 1] = triangleID;
		}
		else {
			if( !m_vertexOwnershipExt.ContainsKey( vertexIndex ) ) {
				m_vertexOwnershipExt[vertexIndex] = new List<int>();
			}
			m_vertexOwnershipExt[vertexIndex].Add( triangleID );
		}
	}
	
	private void _Compactify() {
		foreach( var triangleNote in m_trianglesMap ) {
			var indexIndex = triangleNote.Value *3;
			if( m_triangles[indexIndex] == c_invalidID ) {
				// this is dead!
			}
		}
		
		var lastAliveIndex = m_positions.Count - 1;
		var index = 0;
		while( index <= lastAliveIndex ) {
			var ownershipIndex = index *_ownershipDataSize;
			var ownersCount = m_vertexOwnership[ownershipIndex];
			if( ownersCount != 0 ) {
				index += 1;
			}
			else {
				// it is dead! We need swaps
				_DestroyVertex( index, lastAliveIndex );
				lastAliveIndex -= 1;
			}
		}
		
		var firstDeadIndex = lastAliveIndex + 1;
		var itemsToRemove = m_positions.Count - firstDeadIndex;
		m_positions.RemoveRange( firstDeadIndex, itemsToRemove );
		m_vertexOwnership.RemoveRange( firstDeadIndex * _ownershipDataSize, itemsToRemove * _ownershipDataSize);
		// TODO: also other data, should it arise!
		
		m_generation += 1;
		// TODO: m_topVertexID, m_topTriangleID
	}
	
	private void _DestroyTriangle( int deadIndex, int aliveIndex ) {
		
	}
	
	private void _DestroyVertex( int deadIndex, int aliveIndex ) {
		m_positions.HalfSwap( deadIndex, aliveIndex );
		m_vertexOwnership.HalfSwap( deadIndex, aliveIndex, _ownershipDataSize );
		// TODO: also other data, should it arise!
		
		if( m_vertexOwnershipExt.ContainsKey( deadIndex ) ) {
			m_vertexOwnershipExt.Remove( deadIndex );
		}
		if( m_vertexOwnershipExt.ContainsKey( aliveIndex ) ) {
			m_vertexOwnershipExt[deadIndex] = m_vertexOwnershipExt[aliveIndex];
			m_vertexOwnershipExt.Remove( aliveIndex );
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
