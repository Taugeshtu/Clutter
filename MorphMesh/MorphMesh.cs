using UnityEngine;
using System.Collections.Generic;

using DRAW = Draw;

namespace Clutter.Mesh {

public class MorphMesh {
	public const int c_invalidID = -1;
	private const int c_initialVertexCapacity = 300;
	
	// - - - - VERTEX DATA - - - -
	internal List<Vector3> m_positions = new List<Vector3>( c_initialVertexCapacity );
	internal List<Color> m_colors = new List<Color>( c_initialVertexCapacity );	// todo: don't allocate if you don't need
	
	internal List<int> m_ownersCount = new List<int>( c_initialVertexCapacity );
	internal List<int> m_ownersFast = new List<int>( c_initialVertexCapacity *VertexOwnership.c_ownersFast );	// Note: not sure how worth it this optimization is
	internal Dictionary<int, HashSet<int>> m_ownersExt = new Dictionary<int, HashSet<int>>();
	// - - - - - - - - - - - - - -
	
	// - - - - TRIANGLE DATA - - -
	internal List<int> m_indeces = new List<int>( c_initialVertexCapacity );	// index buffer, basically
	private HashSet<int> m_deadTriangles = new HashSet<int>();
	// - - - - - - - - - - - - - -
	
	// - - - - UTILITY  DATA - - -
	internal long m_generation = 1;	// should never be reset! Starting from "1" makes default( Vertex ) invalid
	protected int m_topVertexIndex = c_invalidID;
	protected int m_topTriangleIndex = c_invalidID;
	protected bool m_trianglesSolid = true;
	// - - - - - - - - - - - - - -
	
	// reusable utility containers
	private static Dictionary<int, int> t_vertexMapping = new Dictionary<int, int>( c_initialVertexCapacity );
	private static Dictionary<int, HashSet<int>> t_weldMap = new Dictionary<int, HashSet<int>>( 10 );	// vertex -> weldGroup
	private static HashSet<HashSet<int>> t_weldGroups = new HashSet<HashSet<int>>();
	private static List<Vector3> t_weldPoints = new List<Vector3>( 10 );
	// - - - - - - - - - - - - - -
	
	public MeshFilter Target { get; private set; }
	
	public bool HasColors {
		get {
			return (m_positions.Count > 0) && (m_colors.Count > 0);
		}
	}
	
	public int VertexCount {
		get {
			return m_topVertexIndex + 1;
		}
	}
	
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
		m_colors.Clear();
		
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
		
		m_ownersCount.Resize( verticesCount );
		m_ownersFast.Resize( verticesCount *VertexOwnership.c_ownersFast, c_invalidID );
		for( var i = 0; i < verticesCount; i++ ) {
			var ownershipData = new VertexOwnership( this, i );
			result.Append( "\n" );
			result.Append( ownershipData.ToString() );
			result.Append( "; " );
			result.Append( m_positions[i].LogFormat( "f2" ) );
		}
		
		result.Append( "\n" );
		result.Append( "Indeces: " );
		result.Append( "\n" );
		var indexIndex = -1;
		result.Append(
			m_indeces.Dump(
				(index) => {
					indexIndex += 1;
					if( indexIndex %3 == 0 ) {
						return "("+index;
					}
					else if( indexIndex %3 == 2 ) {
						return index+")";
					}
					else {
						return index.ToString();
					}
				},
				" "
			)
		);
		
		result.Append( "\n" );
		result.Append( "Dead triangles: " );
		result.Append( m_deadTriangles.Dump() );
		
		result.Append( "\n" );
		result.Append( "Latest Verts remap: " );
		result.Append( t_vertexMapping.Dump() );
		
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
		mesh.GetColors( m_colors );
		
		m_topVertexIndex = m_positions.Count - 1;
		m_topTriangleIndex = (m_indeces.Count /3) - 1;
		
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
		
		_SyncPropertiesSizes();
		
		var mesh = filterTarget.mesh;
		mesh.Clear();
		mesh.SetVertices( m_positions );
		mesh.SetColors( HasColors ? m_colors : null );
		mesh.SetTriangles( m_indeces, 0 );
		mesh.RecalculateNormals();
		
		var colliderTarget = filterTarget.GetComponent<MeshCollider>();
		if( colliderTarget != null ) {
			colliderTarget.sharedMesh = mesh;
		}
	}
#endregion
	
	
#region Access ops
	public Vertex GetVertex( int index ) {
		return new Vertex( this, index );
	}
	
	public Triangle GetTriangle( int id ) {
		return new Triangle( this, id );
	}
	
	public List<Vertex> GetAllVertices( bool includeDead = true ) {
		var result = new List<Vertex>();
		
		// Feeling pretty clever right about now
		// TODO: profile the shit outta that bitch. Maybe I'm not optimizing at all
		System.Action<int> filler = (vertexIndex) => {
			result.Add( GetVertex( vertexIndex ) );
		};
		if( !includeDead ) {
			filler = (vertexIndex) => {
				if( m_ownersCount[vertexIndex] > 0 ) {
					result.Add( GetVertex( vertexIndex ) );
				}
			};
		}
		
		for( var i = 0; i <= m_topVertexIndex; i++ ) {
			filler( i );
		}
		
		return result;
	}
	
	public List<Triangle> GetAllTriangles( bool includeDead = true ) {
		var result = new List<Triangle>();
		
		// Feeling pretty clever right about now
		System.Action<int> filler = (trisIndex) => {
			result.Add( GetTriangle( trisIndex ) );
		};
		if( !includeDead ) {
			filler = (trisIndex) => {
				if( m_deadTriangles.Contains( trisIndex ) == false ) {
					result.Add( GetTriangle( trisIndex ) );
				}
			};
		}
		
		for( var i = 0; i <= m_topTriangleIndex; i++ ) {
			filler( i );
		}
		
		return result;
	}
#endregion
	
	
#region Vertex ops
	public Vertex EmitVertex( Vector3 position ) {
		return EmitVertex( position, Color.white );
	}
	public Vertex EmitVertex( Vector3 position, Color color ) {
		m_topVertexIndex += 1;
		
		m_positions.Add( position );
		m_colors.Add( color );
		// TODO: other porperties
		m_ownersCount.Add( 0 );
		m_ownersFast.Resize( (m_topVertexIndex + 1) *VertexOwnership.c_ownersFast, c_invalidID );
		
		var vertex = new Vertex( this, m_topVertexIndex );
		return vertex;
	}
	
	// Delete ops are only for verts that are present!
	public void DeleteVertex( int index ) {
		var vertex = new Vertex( this, index );
		_DeleteVertex( vertex, c_invalidID );
	}
	public void DeleteVertex( Vertex vertex ) {
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
	
	public Triangle EmitTriangle( Vertex vA, Vertex vB, Vertex vC ) {
		return EmitTriangle( ref vA, ref vB, ref vC );
	}
	
	public Triangle EmitTriangle( ref Vertex vA, ref Vertex vB, ref Vertex vC ) {
		m_topTriangleIndex += 1;
		
		m_indeces.Add( vA.Index, vB.Index, vC.Index );
		
		var triangle = new Triangle( this, m_topTriangleIndex );
		vA.m_ownership.AddOwner( m_topTriangleIndex );
		vB.m_ownership.AddOwner( m_topTriangleIndex );
		vC.m_ownership.AddOwner( m_topTriangleIndex );
		return triangle;
	}
	
	// Delete ops are only for tris that are present! Make sure there are no duplicates tho
	public void DeleteTriangle( int index, bool destroyVertices = false ) {
		var triangle = new Triangle( this, index );
		_DeleteTriangle( triangle, destroyVertices );
	}
	public void DeleteTriangle( Triangle triangle, bool destroyVertices = false ) {
		_DeleteTriangle( triangle, destroyVertices );
	}
#endregion
	
	
#region Quad + Cube
	// with identity rotation it'll be quad lying flat in X-Z plane
	public void EmitQuad() { EmitQuad( Vector3.zero, Quaternion.identity, Vector2.one ); }
	public void EmitQuad( Vector3 position ) { EmitQuad( position, Quaternion.identity, Vector2.one ); }
	public void EmitQuad( Vector3 position, Quaternion rotation ) { EmitQuad( position, rotation, Vector2.one ); }
	public void EmitQuad( Vector3 position, Vector2 scale ) { EmitQuad( position, Quaternion.identity, scale ); }
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
	public void EmitCube( Vector3 position, Vector3 scale ) { EmitCube( position, Quaternion.identity, scale ); }
	public void EmitCube( Vector3 position, Quaternion rotation, Vector3 scale ) {
		var right = rotation *(Vector3.right *scale.x) *0.5f;
		var up = rotation *(Vector3.up *scale.y) *0.5f;
		var forward = rotation *(Vector3.forward *scale.z) *0.5f;
		
		// separate quads with unique vertices because we ought to keep them flat
		EmitQuad( position + up,		rotation,										scale.XZ()	);	// Y+
		EmitQuad( position - up,		rotation *Quaternion.Euler( 180f, 0f,   0f ),	scale.XZ()	);	// Y-
		EmitQuad( position + forward,	rotation *Quaternion.Euler(  90f, 0f,   0f ),	scale.XY()	);	// Z+
		EmitQuad( position - forward,	rotation *Quaternion.Euler( -90f, 0f,   0f ),	scale.XY()	);	// Z-
		EmitQuad( position + right,		rotation *Quaternion.Euler(   0f, 0f, -90f ),	scale.YZ()	);	// X+
		EmitQuad( position - right,		rotation *Quaternion.Euler(   0f, 0f,  90f ),	scale.YZ()	);	// X-
	}
#endregion
	
	
#region Geometry ops
	public void MergeVertices( float treshold = 0.001f ) {
		t_weldMap.Clear();
		t_weldGroups.Clear();
		var sqTreshold = treshold *treshold;
		
		for( var indexA = 0; indexA < m_topVertexIndex; indexA++ ) {
			if( m_ownersCount[indexA] == 0 ) { continue; }	// vertex dead
			if( t_weldMap.ContainsKey( indexA ) ) { continue; }	// already processed
			
			HashSet<int> newBundle = null;
			
			for( var indexB = indexA + 1; indexB <= m_topVertexIndex; indexB++ ) {
				if( m_ownersCount[indexB] == 0 ) { continue; }	// vertex dead
				if( t_weldMap.ContainsKey( indexB ) ) { continue; }	// already processed
				
				var positionA = m_positions[indexA];
				var positionB = m_positions[indexB];
				var diff = positionB - positionA;
				if( diff.sqrMagnitude < sqTreshold ) {
					if( newBundle == null ) { newBundle = new HashSet<int>(); }
					newBundle.Add( indexB );
				}
			}
			
			if( newBundle != null ) {
				newBundle.Add( indexA );
				foreach( var index in newBundle ) {
					t_weldMap[index] = newBundle;
				}
				t_weldGroups.Add( newBundle );
			}
		}
		
		foreach( var group in t_weldGroups ) {
			_MergeVertices( group );
		}
	}
	
	public void WeldVertices( float treshold = 0.001f, bool mergeVertices = false ) {
		t_weldMap.Clear();
		var sqTreshold = treshold *treshold;
		
		for( var indexA = 0; indexA <= m_topVertexIndex; indexA++ ) {
			if( m_ownersCount[indexA] == 0 ) { continue; }	// vertex dead
			
			HashSet<int> newBundle = null;
			
			for( var indexB = 0; indexB <= m_topVertexIndex; indexB++ ) {
				if( indexB == indexA ) { continue; }
				if( m_ownersCount[indexB] == 0 ) { continue; }	// vertex dead
				
				var positionA = m_positions[indexA];
				var positionB = m_positions[indexB];
				var diff = positionB - positionA;
				if( diff.sqrMagnitude < sqTreshold ) {
					if( newBundle == null ) { newBundle = new HashSet<int>(); }
					newBundle.Add( indexB );
				}
			}
			
			if( newBundle != null ) {
				newBundle.Add( indexA );
				
				var oldBundle = t_weldMap.ContainsKey( indexA ) ? t_weldMap[indexA] : null;
				if( oldBundle != null ) {
					if( newBundle.Count > oldBundle.Count ) {
						oldBundle.ExceptWith( newBundle );
					}
					else {
						newBundle.ExceptWith( oldBundle );
					}
				}
				
				foreach( var index in newBundle ) {
					t_weldMap[index] = newBundle;
				}
			}
		}
		
		t_weldGroups.Clear();
		foreach( var group in t_weldMap.Values ) {
			if( group.Count < 2 ) { continue; }
			t_weldGroups.Add( group );
		}
		
		foreach( var group in t_weldGroups ) {
			_WeldVertices( group, mergeVertices );
		}
	}
	
	public void MakeVerticesUnique() {
		var allVerts = GetAllVertices();	// what the hell was I smoking then?.. THESE WILL STILL HAVE TRANSIENT STRUCTS
		foreach( var vertex in allVerts ) {
			if( vertex.m_ownership.OwnersCount <= 1 ) {
				continue;
			}
			
			
		}
	}
	
	public Selection Slice( Vector3 point, Vector3 normal, bool directOnly = true ) {
		var plane = new Plane( normal, point );
		
		var frontTriangles = new List<Triangle>( m_topTriangleIndex + 1 );
		var backTriangles = new List<Triangle>( m_topTriangleIndex + 1 );
		var intersectTriangles = new List<Triangle>( m_topTriangleIndex + 1 );
		for( var i = 0; i <= m_topTriangleIndex; i++ ) {
			var tris = GetTriangle( i );
			var side = tris.GetPlaneSide( plane );
			if( side == PlaneSide.Front ) { frontTriangles.Add( tris ); }
			if( side == PlaneSide.Back ) { backTriangles.Add( tris ); }
			if( side == PlaneSide.Intersecting ) { intersectTriangles.Add( tris ); }
		}
		
		if( intersectTriangles.Count == 0 ) {
			return null;
		}
		
		/*
		foreach( var tris in frontTriangles ) { tris.Draw( Color.blue, 0.5f, 5 ); }
		foreach( var tris in backTriangles ) { tris.Draw( Color.yellow, 0.5f, 5 ); }
		foreach( var tris in intersectTriangles ) { tris.Draw( Color.red, 0.5f, 5 ); }
		*/
		
		var trisToSlice = new Selection( this, intersectTriangles );
		var trisToDrift = new Selection( this, frontTriangles );
		if( directOnly ) {
			var slicedParts = trisToSlice.BreakApart( false );
			var frontParts = trisToDrift.BreakApart( false );
			
			// TODO: determine which one of those has a triangle that's closest to slice point
			if( slicedParts.Count > 1 ) {
				var minDistance = float.MaxValue;
				foreach( var group in slicedParts ) {
					foreach( var vertex in group.Vertices ) {
						var distance = Vector3.SqrMagnitude( vertex.Position - point );
						if( distance < minDistance ) {
							minDistance = distance;
							trisToSlice = group;
						}
					}
				}
			}
			
			if( frontParts.Count > 1 ) {
				var minDistance = float.MaxValue;
				foreach( var group in frontParts ) {
					foreach( var vertex in group.Vertices ) {
						var distance = Vector3.SqrMagnitude( vertex.Position - point );
						if( distance < minDistance ) {
							minDistance = distance;
							trisToDrift = group;
						}
					}
				}
			}
		}
		
		foreach( var tris in trisToSlice ) {
			for( var i = 0; i < 3; i++ ) {
				var vA = tris[i];
				var vB = tris[i + 1];
				var vC = tris[i + 2];
				
				var a = vA.Position;
				var b = vB.Position;
				var c = vC.Position;
				
				var aSide = plane.GetSide( a );
				var bSide = plane.GetSide( b );
				var cSide = plane.GetSide( c );
				
				if( (aSide == bSide) || (aSide == cSide) ) { continue; }	// we are not a "lone" vertex
				
				var bMid = plane.Cast( new Ray( a, b - a ) );
				var cMid = plane.Cast( new Ray( a, c - a ) );
				var vAB = EmitVertex( bMid );
				var vAC = EmitVertex( cMid );
				
				if( aSide == false ) {	// lone corner is on the back
					// this is staying part
					EmitTriangle( ref vA, ref vAB, ref vAC );
					
					// this is drifting:
					trisToDrift.Add( EmitTriangle( ref vAB, ref vB, ref vAC ) );
					trisToDrift.Add( EmitTriangle( ref vAC, ref vB, ref vC ) );
				}
				else {
					// this is staying part
					EmitTriangle( ref vAB, ref vB, ref vAC );
					EmitTriangle( ref vAC, ref vB, ref vC );
					
					// this is drifting:
					trisToDrift.Add( EmitTriangle( ref vA, ref vAB, ref vAC ) );
				}
				
				break;
			}
			
			DeleteTriangle( tris );
		}
		
		return trisToDrift;
	}
#endregion
	
	
#region Private
	private void _RebuildOwnershipData() {
		m_ownersCount.Resize( m_positions.Count );
		m_ownersFast.Resize( m_positions.Count *VertexOwnership.c_ownersFast, c_invalidID );
		
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
		
		// Shifting dead vertices to the back of the containers, filling mapping, clearing ownership
		var vertexCount = m_positions.Count;
		var lastAliveIndex = vertexCount - 1;
		var index = 0;
		var wasMoved = false;
		var verticesSaved = 0;
		while( index <= lastAliveIndex ) {
			var actualIndex = wasMoved ? (lastAliveIndex + 1) : index;
			var isDead = (m_ownersCount[index] == 0);
			if( isDead ) {
				t_vertexMapping[actualIndex] = c_invalidID;
				
				m_ownersExt.Remove( actualIndex );
				_MoveVertexData( lastAliveIndex, index );
				lastAliveIndex -= 1;
				wasMoved = true;
			}
			else {
				t_vertexMapping.Add( actualIndex, verticesSaved );
				verticesSaved += 1;
				
				index += 1;
				wasMoved = false;
			}
		}
		
		// Cleaning vertex containers
		var firstDeadIndex = lastAliveIndex + 1;
		var itemsToRemove = vertexCount - firstDeadIndex;
		m_positions.RemoveRange( firstDeadIndex, itemsToRemove );
		m_colors.RemoveRange( firstDeadIndex, itemsToRemove );
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
		
		// Shifting dead triangles to the back of the containers
		var lastAliveIndex = trianglesCount - 1;
		var index = 0;
		while( index <= lastAliveIndex ) {
			var indexIndex = index *3;
			var deadByList = m_deadTriangles.Contains( index );
			var deadByVerts = (m_indeces[indexIndex + 0] == c_invalidID) || (m_indeces[indexIndex + 1] == c_invalidID) || (m_indeces[indexIndex + 2] == c_invalidID);
			var isDead = deadByList || deadByVerts;
			
			if( isDead ) {
				_MoveTriangleData( lastAliveIndex, index );
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
		
		m_deadTriangles.Clear();
		m_topTriangleIndex = (m_indeces.Count /3) - 1;
		m_trianglesSolid = true;
		m_generation += 1;
	}
	
	private void _MoveVertexData( int sourceIndex, int destIndex ) {
		m_positions.HalfSwap( destIndex, sourceIndex );
		m_colors.HalfSwap( destIndex, sourceIndex, 1, true );
		// TODO: also other data, should it arise!
		
		var destOwner = new VertexOwnership( this, destIndex );
		var sourceOwner = new VertexOwnership( this, sourceIndex );
		destOwner.MoveOwnershipFrom( ref sourceOwner );
	}
	
	private void _MoveTriangleData( int sourceIndex, int destIndex ) {
		// ownership:
		foreach( var vert in GetTriangle( destIndex ) ) {
			vert.m_ownership.RemapOwner( destIndex, c_invalidID );
		}
		foreach( var vert in GetTriangle( sourceIndex ) ) {
			vert.m_ownership.RemapOwner( sourceIndex, destIndex );
		}
		
		m_indeces.HalfSwap( destIndex *3, sourceIndex *3, 3 );
		
		// handling death info (so consequtive tris handling will work):
		var sourceDead = m_deadTriangles.Remove( sourceIndex );
		if( sourceDead ) {
			m_deadTriangles.Add( destIndex );
		}
		else {
			m_deadTriangles.Remove( destIndex );
		}
	}
	
	private void _DeleteVertex( Vertex vertex, int triangleToIgnore ) {
		foreach( var ownerID in vertex.m_ownership ) {
			if( ownerID == triangleToIgnore ) { continue; }
			var tris = GetTriangle( ownerID );
			_DeleteTriangle( tris, false );
		}
		
		m_ownersCount[vertex.Index] = 0;
		m_ownersExt.Remove( vertex.Index );
	}
	
	private void _DeleteTriangle( Triangle triangle, bool deleteVertices ) {
		m_trianglesSolid = false;
		
		if( deleteVertices ) {
			foreach( var vertex in triangle ) {
				_DeleteVertex( vertex, triangle.Index );
			}
		}
		
		m_deadTriangles.Add( triangle.Index );
	}
	
	private void _MergeVertices( IEnumerable<int> vertices ) {
		var firstIndex = c_invalidID;
		foreach( var vertexIndex in vertices ) {
			if( firstIndex == c_invalidID ) {
				firstIndex = vertexIndex;
			}
			else {
				var ownership = new VertexOwnership( this, vertexIndex );
				var firstOwnership = new VertexOwnership( this, firstIndex );
				foreach( var ownerID in ownership ) {
					var indexIndex = ownerID *3;
					for( var i = 0; i < 3; i++ ) {
						if( m_indeces[indexIndex + i] == vertexIndex ) {
							m_indeces[indexIndex + i] = firstIndex;
						}
					}
					
					var alreadyRegistered = false;
					foreach( var firstOwner in firstOwnership ) {
						if( firstOwner == ownerID ) {
							alreadyRegistered = true;
						}
					}
					
					if( !alreadyRegistered ) {
						firstOwnership.AddOwner( ownerID );
					}
				}
				
				m_ownersCount[vertexIndex] = 0;
				m_ownersExt.Remove( vertexIndex );
			}
		}
	}
	
	private void _WeldVertices( IEnumerable<int> vertices, bool mergeVertices ) {
		// TODO: add logic that will make it so same-position verts don't overpull
		t_weldPoints.Clear();
		
		foreach( var index in vertices ) {
			var position = m_positions[index];
			
			var isUnique = true;
			foreach( var weldPoint in t_weldPoints ) {
				var diff = weldPoint - position;
				if( diff.sqrMagnitude < Mathf.Epsilon ) {
					isUnique = false;
					break;
				}
			}
			
			if( isUnique ) {
				t_weldPoints.Add( position );
			}
		}
		
		var newPosition = Vector3.zero;
		foreach( var weldPoint in t_weldPoints ) {
			newPosition += weldPoint;
		}
		newPosition /= t_weldPoints.Count;
		
		foreach( var index in vertices ) {
			m_positions[index] = newPosition;
		}
		
		if( mergeVertices ) {
			_MergeVertices( vertices );
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
	
	private void _SyncPropertiesSizes() {
		if( HasColors ) {
			m_colors.Resize( m_positions.Count, Color.white );
		}
	}
	
	private VertexOwnership _MakeOwnershipData( int vertexIndex ) {
		return new VertexOwnership( this, vertexIndex );
	}
	
	public void Draw() {
		for( var i = 0; i <= m_topVertexIndex; i++ ) {
			var position = m_positions[i];
			
			var ownership = new VertexOwnership( this, i );
			DRAW.Text( position, ownership.ToString() );
		}
		
		foreach( var tris in GetAllTriangles( true ) ) {
			DRAW.Text( tris.Center, tris.ToString() );
		}
	}
#endregion
}
}
