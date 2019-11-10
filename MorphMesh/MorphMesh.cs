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
	private bool m_trianglesSolid = true;
	// - - - - - - - - - - - - - -
	
	// - - - - UTILITY  DATA - - -
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
		
		m_trianglesSolid = true;
		
		m_deadTriangles.Clear();
	}
	
	// This is potentially heavy, so extracted out
	public void CompactifyVertices() {
		_CompactifyVertices();
	}
	
	public string Dump() {
		var verticesCount = m_positions.Count;
		var result = new System.Text.StringBuilder();
		result.Append( "Mesh data" );
		result.Append( "\n" );
		result.Append( "Vertices: " );
		result.Append( verticesCount );
		
		m_ownersCount.PadUpTo( verticesCount );
		m_ownersFast.PadUpTo( (verticesCount + 1) *VertexOwnership.c_ownersFast, -1 );
		for( var i = 0; i < verticesCount; i++ ) {
			var ownershipData = new VertexOwnership( this, i );
			result.Append( "\n" );
			result.Append( ownershipData.ToString() );
			result.Append( "; " );
			result.Append( m_positions[i].LogFormat( "00.00" ) );
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
		
		// Note: this is necessary because dead triangles shouldn't get into render. Dead verts is ok
		_CompactifyTriangles();
		
		var mesh = filterTarget.mesh;
		mesh.Clear();
		mesh.SetVertices( m_positions );
		mesh.SetTriangles( m_indeces, 0 );
		Debug.LogError( "- - - Wrote into mesh, verts: "+m_positions.Count+", indeces: "+m_indeces.Count
		+"\nIn mesh now: "+mesh.vertices.Dump()+" ;; tris: "+mesh.triangles.Dump() );
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
	
	// Delete ops are only for verts that are present!
	public void DeleteVertex( int index ) {
		var vertex = new Vertex( this, index );
		_DeleteVertex( vertex, c_invalidID );
	}
	public void DeleteVertex( Vertex vertex ) {
		_DeleteVertex( vertex, c_invalidID );
	}
	public void DeleteVertex( ref Vertex vertex ) {
		_DeleteVertex( vertex, c_invalidID );
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
	
	// Delete ops are only for tris that are present! Make sure there are no duplicates tho
	public void DeleteTriangle( int id, bool destroyVertices = false ) {
		var triangle = new Triangle( this, id );
		_DeleteTriangle( ref triangle, destroyVertices );
	}
	public void DeleteTriangle( ref Triangle triangle, bool destroyVertices = false ) {
		_DeleteTriangle( ref triangle, destroyVertices );
	}
#endregion
	
	
#region Quad + Cube
	// with identity rotation it'll be quad lying flat in X-Z plane
	public void EmitQuad() { EmitQuad( Vector3.zero, Quaternion.identity, Vector2.one ); }
	public void EmitQuad( Vector3 position ) { EmitQuad( position, Quaternion.identity, Vector2.one ); }
	public void EmitQuad( Vector3 position, Quaternion rotation ) { EmitQuad( position, rotation, Vector2.one ); }
	public void EmitQuad( Vector3 position, Quaternion rotation, Vector2 scale ) {
		var right = rotation *(Vector3.right *scale.x);
		var forward = rotation *(Vector3.forward *scale.y);
		var corner = position - (right + forward) *0.5f;
		
		var a = EmitVertex( corner );
		var b = EmitVertex( corner + forward );
		var c = EmitVertex( corner + forward + right );
		var d = EmitVertex( corner + right );
		
		EmitTriangle( ref a, ref b, ref c );
		EmitTriangle( ref a, ref c, ref d );
	}
	
	public void EmitCube() { EmitCube( Vector3.zero, Quaternion.identity, Vector3.one ); }
	public void EmitCube( Vector3 position ) { EmitCube( position, Quaternion.identity, Vector3.one ); }
	public void EmitCube( Vector3 position, Quaternion rotation ) { EmitCube( position, rotation, Vector3.one ); }
	public void EmitCube( Vector3 position, Quaternion rotation, Vector3 scale ) {
		var right = rotation *(Vector3.right *scale.x) *0.5f;
		var up = rotation *(Vector3.up *scale.y) *0.5f;
		var forward = rotation *(Vector3.forward *scale.z) *0.5f;
		
		// separate quads with unique vertices because we ought to keep them flat
		EmitQuad( position + up,			rotation,										scale.XZ()	);	// Y+
		EmitQuad( position - up,			rotation *Quaternion.Euler( 180f, 0f,   0f ),	scale.XZ()	);	// Y-
		EmitQuad( position + forward,	rotation *Quaternion.Euler(  90f, 0f,   0f ),	scale.XY()	);	// Z+
		EmitQuad( position - forward,	rotation *Quaternion.Euler( -90f, 0f,   0f ),	scale.XY()	);	// Z-
		EmitQuad( position + right,		rotation *Quaternion.Euler(   0f, 0f, -90f ),	scale.YZ()	);	// X+
		EmitQuad( position - right,		rotation *Quaternion.Euler(   0f, 0f,  90f ),	scale.YZ()	);	// X-
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
		
		// Cleaning ownership over dead vertices; building index mapping
		var deadProcessed = 0;
		for( var i = 0; i < m_positions.Count; i++ ) {
			if( m_ownersCount[i] == 0 ) {
				deadProcessed += 1;
				m_ownersExt.Remove( i );
				
				t_vertexMapping.Add( i, c_invalidID );
			}
			else {
				t_vertexMapping.Add( i, i - deadProcessed );
			}
		}
		
		Debug.LogError( "Verts mapping: "+t_vertexMapping.Dump() );
		
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
		
		m_topVertexIndex = m_positions.Count - 1;
		m_generation += 1;
	}
	
	private void _CompactifyTriangles() {
		if( m_trianglesSolid ) { return; }
		
		var trianglesCount = m_indeces.Count /3;
		t_triangleMapping.Clear();
		m_deadTriangles.Sort();
		
		// Building triangle index mapping
		var deadIndex = 0;
		for( var i = 0; i < trianglesCount; i++ ) {
			var deadTriangleIndex = (deadIndex < m_deadTriangles.Count) ? m_deadTriangles[deadIndex] : c_invalidID;
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
		var originalIndex = 0;
		while( index <= lastAliveIndex ) {
			var indexIndex = index *3;
			var deadByList = (t_triangleMapping[originalIndex] == c_invalidID);
			var deadByVerts = (m_indeces[indexIndex + 0] == c_invalidID) || (m_indeces[indexIndex + 1] == c_invalidID) || (m_indeces[indexIndex + 2] == c_invalidID);
			
			Debug.LogError( "Checking tris in slot #"+index+", original index: "+originalIndex+", dead by list/verts: "+deadByList+" || "+deadByVerts );
			
			if( deadByList || deadByVerts ) {
				_MoveTriangleData( index, lastAliveIndex );
				lastAliveIndex -= 1;
			}
			else {
				index += 1;
			}
			originalIndex += 1;
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
		m_trianglesSolid = true;
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
	
	private void _DeleteVertex( Vertex vertex, int triangleToIgnore ) {
		m_ownersCount[vertex.Index] = 0;
		
		foreach( var ownerID in vertex.Ownership ) {
			if( ownerID == triangleToIgnore ) { continue; }
			var tris = GetTriangle( ownerID );
			_DeleteTriangle( ref tris, false );
		}
	}
	
	private void _DeleteTriangle( ref Triangle triangle, bool deleteVertices ) {
		m_trianglesSolid = false;
		
		if( deleteVertices ) {
			foreach( var vertex in triangle ) {
				_DeleteVertex( vertex, triangle.Index );
			}
		}
		
		m_deadTriangles.Add( triangle.Index );
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
