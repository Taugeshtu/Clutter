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
	
	// - - - - TRIANGLE DATA - - -
	internal List<int> m_indeces = new List<int>( c_initialVertexCapacity );	// index buffer, basically
	// - - - - - - - - - - - - - -
	
	// - - - - UTILITY  DATA - - -
	internal long m_generation = 0;	// should never be reset!
	private int m_topVertexIndex = c_invalidID;
	private int m_topTriangleIndex = c_invalidID;
	private bool m_verticesSolid = true;
	private bool m_trianglesSolid = true;
	// - - - - - - - - - - - - - -
	
	// - - - - UTILITY  DATA - - -
	private List<int> m_deadVertices = new List<int>( c_initialVertexCapacity /10 );
	private List<int> m_deadTriangles = new List<int>( c_initialVertexCapacity /30 );
	
	// reusable utility containers
	private static Dictionary<int, int> t_vertexMapping = new Dictionary<int, int>( c_initialVertexCapacity );
	private static Dictionary<int, int> t_triangleMapping = new Dictionary<int, int>( c_initialVertexCapacity /3 );
	// - - - - - - - - - - - - - -
	
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
		
		m_indeces.Clear();
		
		// Note: not clearing out generation in here!!
		m_topVertexIndex = c_invalidID;
		m_topTriangleIndex = c_invalidID;
		
		m_verticesSolid = true;
		m_trianglesSolid = true;
		
		m_deadVertices.Clear();
		m_deadTriangles.Clear();
	}
	
	public void CompactifyVertices() {
		_CompactifyVertices();
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
			var ownershipData = new VertexOwnership( this, i );
			result.Append( "\n" );
			result.Append( ownershipData.ToString() );
		}
		
		result.Append( "\n" );
		result.Append( "Indeces: " );
		result.Append( "\n" );
		result.Append( m_indeces.Dump() );
		
		result.Append( "\n" );
		result.Append( "Latest Verts remap: " );
		result.Append( t_vertexMapping.Dump() );
		
		result.Append( "\n" );
		result.Append( "Latest Tris remap: " );
		result.Append( t_triangleMapping.Dump() );
		
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
		
		_RebuildOwnershipData();
	}
	
	public void Write( GameObject target ) {
		Write( target.transform );
	}
	
	public void Write( Component target = null ) {
		var filterTarget = _GetFilterTarget( target );
		if( filterTarget == null ) { return; }
		
		_CompactifyTriangles();
		// TODO: linerialize IDs and indeces!
		
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
	
	// Immediate delete ops are wrong.
	// At least until there are IDs for vertices
	public void DeleteVertex( int index ) {
		var vertex = new Vertex( this, index );
		_DeleteVertex( vertex );
	}
	public void DeleteVertex( Vertex vertex ) {
		_DeleteVertex( vertex );
	}
	public void DeleteVertex( ref Vertex vertex ) {
		_DeleteVertex( vertex );
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
		m_topTriangleIndex += 1;
		
		var triangleIndex = m_indeces.Count /3;
		m_indeces.Add( vA.Index, vB.Index, vC.Index );
		
		var triangle = new Triangle( this, m_topTriangleIndex );
		vA.Ownership.AddOwner( m_topTriangleIndex );
		vB.Ownership.AddOwner( m_topTriangleIndex );
		vC.Ownership.AddOwner( m_topTriangleIndex );
		return triangle;
	}
	
	public Triangle GetTriangle( int id ) {
		return new Triangle( this, id );
	}
	
	public void DeleteTriangle( int id, bool destroyVertices = false ) {
		var triangle = new Triangle( this, id );
		_DeleteTriangle( ref triangle, destroyVertices );
	}
	public void DeleteTriangle( ref Triangle triangle, bool destroyVertices = false ) {
		_DeleteTriangle( ref triangle, destroyVertices );
	}
#endregion
	
	
#region Private
	private void _RebuildOwnershipData() {
		m_ownersCount.PadUpTo( m_positions.Count );
		m_ownersFast.PadUpTo( m_positions.Count *VertexOwnership.c_ownersFast );
		
		var trianglesCount = m_indeces.Count /3;
		for( var triangleIndex = 0; triangleIndex < trianglesCount; triangleIndex++ ) {
			var indexShift = triangleIndex *3;
			_MakeOwnershipData( m_indeces[indexShift + 0] ).AddOwner( triangleIndex );
			_MakeOwnershipData( m_indeces[indexShift + 1] ).AddOwner( triangleIndex );
			_MakeOwnershipData( m_indeces[indexShift + 2] ).AddOwner( triangleIndex );
		}
	}
	
	private void _CompactifyVertices() {
		t_vertexMapping.Clear();
		m_deadVertices.Sort();
		
		// Cleaning ownership over dead vertices; building index mapping
		var deadIndex = 0;
		for( var i = 0; i < m_positions.Count; i++ ) {
			var deadVertexIndex = m_deadVertices[deadIndex];
			if( deadVertexIndex == i ) {
				deadIndex += 1;
				t_vertexMapping.Add( i, c_invalidID );
				
				m_ownersCount[i] = 0;
				m_ownersExt.Remove( deadVertexIndex );
			}
			else {
				t_vertexMapping.Add( i, i - deadIndex );
			}
		}
		
		// Shifting dead vertices to the back of the containers
		var vertexCount = m_positions.Count;
		var lastAliveIndex = vertexCount - 1;
		var index = 0;
		while( index <= lastAliveIndex ) {
			var isDead = (m_ownersCount[index] == 0);
			if( isDead ) {
				_MoveVertexData( index, lastAliveIndex );
				lastAliveIndex -= 1;
			}
			else {
				index += 1;
			}
		}
		
		// Cleaning vertex containers
		var firstDeadIndex = lastAliveIndex + 1;
		var itemsToRemove = vertexCount - firstDeadIndex;
		m_positions.RemoveRange( firstDeadIndex, itemsToRemove );
		m_ownersCount.RemoveRange( firstDeadIndex, itemsToRemove );
		m_ownersFast.RemoveRange( firstDeadIndex *VertexOwnership.c_ownersFast, itemsToRemove *VertexOwnership.c_ownersFast );
		
		// Rebuilding indeces
		for( var i = 0; i < m_indeces.Count; i++ ) {
			var vertexIndex = m_indeces[i];
			m_indeces[i] = t_vertexMapping[vertexIndex];
		}
		
		m_deadVertices.Clear();
		m_topVertexIndex = m_positions.Count - 1;
		m_generation += 1;
	}
	
	private void _CompactifyTriangles() {
		var trianglesCount = m_indeces.Count /3;
		t_triangleMapping.Clear();
		m_deadTriangles.Sort();
		
		// Building triangle index mapping
		var deadIndex = 0;
		for( var i = 0; i < trianglesCount; i++ ) {
			var deadTriangleIndex = m_deadTriangles[deadIndex];
			if( deadTriangleIndex == i ) {
				deadIndex += 1;
				t_triangleMapping.Add( i, c_invalidID );
			}
			else {
				t_triangleMapping.Add( i, i - deadIndex );
			}
		}
		
		// Shifting dead triangles to the back of the containers
		var lastAliveIndex = trianglesCount - 1;
		var index = 0;
		while( index <= lastAliveIndex ) {
			var indexIndex = index *3;
			var isDead = (t_triangleMapping[index] == c_invalidID);
			isDead = isDead || (m_indeces[indexIndex + 0] == c_invalidID) || (m_indeces[indexIndex + 1] == c_invalidID) || (m_indeces[indexIndex + 2] == c_invalidID);
			
			if( isDead ) {
				_MoveTriangleData( index, lastAliveIndex );
				lastAliveIndex -= 1;
			}
			else {
				index += 1;
			}
		}
		
		// Cleaning trinagle containers
		var firstDeadIndex = lastAliveIndex + 1;
		var itemsToRemove = trianglesCount - firstDeadIndex;
		m_indeces.RemoveRange( firstDeadIndex *3, itemsToRemove *3 );
		
		// Updating ownership
		for( var vertexIndex = 0; vertexIndex < m_positions.Count; vertexIndex++ ) {
			var ownership = new VertexOwnership( this, vertexIndex );
			ownership.RemapOwners( t_triangleMapping );
		}
		
		m_deadTriangles.Clear();
		m_topTriangleIndex = (m_indeces.Count /3) + 1;
		m_generation += 1;
	}
	
	private void _MoveVertexData( int destIndex, int sourceIndex ) {
		m_positions.HalfSwap( destIndex, sourceIndex );
		// TODO: also other data, should it arise!
		
		var destOwner = new VertexOwnership( this, destIndex );
		var sourceOwner = new VertexOwnership( this, sourceIndex );
		destOwner.MoveOwnershipFrom( ref sourceOwner );
	}
	
	private void _MoveTriangleData( int destIndex, int sourceIndex ) {
		m_indeces.HalfSwap( destIndex *3, sourceIndex *3, 3 );
	}
	
	private void _DeleteVertex( Vertex vertex ) {
		foreach( var ownerID in vertex.Ownership ) {
			var tris = GetTriangle( ownerID );
			_DeleteTriangle( ref tris, false );
		}
		
		var lastAliveIndex = m_positions.Count - 1;
		_MoveVertexData( vertex.Index, lastAliveIndex );
		
		m_positions.RemoveAt( lastAliveIndex );
		m_ownersCount.RemoveAt( lastAliveIndex );
		m_ownersFast.RemoveRange( lastAliveIndex, VertexOwnership.c_ownersFast );
	}
	
	private void _DeleteTriangle( ref Triangle triangle, bool deleteVertices ) {
		if( deleteVertices ) {
			var verts = new List<Vertex>( triangle );
			verts.Sort(
				(a, b) => {
					return 1 - a.Index.CompareTo( b.Index );	// reverse sorting, bigger indexes first not to fuck up 
				}
			);
			foreach( var vertex in verts ) {
				_DeleteVertex( vertex );
			}
			return;
		}
		
		var destIndex = m_trianglesMap[triangle.Index];
		var lastAliveIndex = (m_indeces.Count /3) - 1;
		_MoveTriangleData( destIndex, lastAliveIndex );
		
		m_indeces.RemoveRange( lastAliveIndex *3, 3 );
		// Note: m_trianglesMap mangling already happened in _MoveTriangleData()
		
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
