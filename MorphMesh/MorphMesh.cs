﻿using UnityEngine;
using System.Collections.Generic;

using DRAW = Draw;

namespace Clutter.Mesh {

public class MorphMesh {
	public const int c_invalidID = -1;
	private const int c_initialVertexCapacity = 300;
	
	// - - - - VERTEX DATA - - - -
	internal List<Vector3> m_positions = new List<Vector3>( c_initialVertexCapacity );
	internal List<Color> m_colors = new List<Color>( c_initialVertexCapacity );	// todo: don't allocate if you don't need
	internal List<Vector2> m_uvs = new List<Vector2>( c_initialVertexCapacity );	// todo: don't allocate if you don't need
	
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
	
	public bool HasColors {
		get {
			return (m_positions.Count > 0) && (m_colors.Count > 0);
		}
	}
	
	public bool HasUVs {
		get {
			return (m_positions.Count > 0) && (m_uvs.Count > 0);
		}
	}
	
	public int VertexCount {
		get {
			return m_topVertexIndex + 1;
		}
	}
	
#region Implementation
	public MorphMesh() {}
#endregion
	
	
#region General
	public void Clear() {
		m_positions.Clear();
		m_colors.Clear();
		m_uvs.Clear();
		
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
	public void ClearDeadVertices() {
		_CompactifyVertices();
	}
	
	public void ClearDeadTriangles() {
		_CompactifyTriangles();
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
		mesh.GetUVs(0, m_uvs );
		
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
		mesh.SetUVs( 0, HasUVs ? m_uvs : null );
		mesh.SetTriangles( m_indeces, 0 );
		mesh.RecalculateNormals();
		
		var colliderTarget = filterTarget.GetComponent<MeshCollider>();
		if( colliderTarget != null ) {
			colliderTarget.sharedMesh = mesh;
		}
	}
	
	public UnityEngine.Mesh WriteMesh( UnityEngine.Mesh target = null ) {
		// Note: this is necessary because dead triangles shouldn't get into render. Dead verts is ok
		_CompactifyTriangles();
		
		_SyncPropertiesSizes();
		
		var mesh = (target == null)
					? new UnityEngine.Mesh()
					: target;
		mesh.Clear();
		mesh.SetVertices( m_positions );
		mesh.SetColors( HasColors ? m_colors : null );
		mesh.SetUVs( 0, HasUVs ? m_uvs : null );
		mesh.SetTriangles( m_indeces, 0 );
		mesh.RecalculateNormals();
		return mesh;
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
		return EmitVertex( position, Color.white, Vector2.zero );
	}
	public Vertex EmitVertex( Vector3 position, Color color, Vector2 uv ) {
		m_topVertexIndex += 1;
		
		m_positions.Add( position );
		m_colors.Add( color );
		m_uvs.Add( uv );
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
		// Layout, viewed from above:
		// A - B
		// |   |
		// D - C
		
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
	
	
#region Primitive ops
	public void EmitIcoSphere( float radius = 1f, int subdivisions = 0 ) { EmitIcoSphere( Pose.identity, Vector3.one *radius, subdivisions ); }
	public void EmitIcoSphere( Vector3 scale, int subdivisions = 0 ) { EmitIcoSphere( Pose.identity, scale, subdivisions ); }
	public void EmitIcoSphere( Vector3 position, Quaternion rotation, float radius = 1f, int subdivisions = 0 ) { EmitIcoSphere( position, rotation, Vector3.one *radius, subdivisions ); }
	public void EmitIcoSphere( Vector3 position, Quaternion rotation, Vector3 scale, int subdivisions = 0 ) {
		var pose = new Pose( position, rotation );
		EmitIcoSphere( pose, scale, subdivisions );
	}
	public void EmitIcoSphere( Pose pose, float radius = 1f, int subdivisions = 0 ) { EmitIcoSphere( pose, Vector3.one *radius, subdivisions ); }
	public void EmitIcoSphere( Pose pose, Vector3 scale, int subdivisions = 0 ) {
		var t = (1 + Mathf.Sqrt( 5 )) /2;	// no idea... wiki math
		
		var v0 = EmitVertex( pose.Transform( new Vector3( -1,  t,  0 ).normalized.ComponentMul( scale ) ) );
		var v1 = EmitVertex( pose.Transform( new Vector3(  1,  t,  0 ).normalized.ComponentMul( scale ) ) );
		var v2 = EmitVertex( pose.Transform( new Vector3( -1, -t,  0 ).normalized.ComponentMul( scale ) ) );
		var v3 = EmitVertex( pose.Transform( new Vector3(  1, -t,  0 ).normalized.ComponentMul( scale ) ) );
		var v4 = EmitVertex( pose.Transform( new Vector3(  0, -1,  t ).normalized.ComponentMul( scale ) ) );
		var v5 = EmitVertex( pose.Transform( new Vector3(  0,  1,  t ).normalized.ComponentMul( scale ) ) );
		var v6 = EmitVertex( pose.Transform( new Vector3(  0, -1, -t ).normalized.ComponentMul( scale ) ) );
		var v7 = EmitVertex( pose.Transform( new Vector3(  0,  1, -t ).normalized.ComponentMul( scale ) ) );
		var v8 = EmitVertex( pose.Transform( new Vector3(  t,  0, -1 ).normalized.ComponentMul( scale ) ) );
		var v9 = EmitVertex( pose.Transform( new Vector3(  t,  0,  1 ).normalized.ComponentMul( scale ) ) );
		var v10 = EmitVertex( pose.Transform( new Vector3( -t,  0, -1 ).normalized.ComponentMul( scale ) ) );
		var v11 = EmitVertex( pose.Transform( new Vector3( -t,  0,  1 ).normalized.ComponentMul( scale ) ) );
		
		// 5 faces around point 0
		EmitTriangle( ref v0, ref v11, ref  v5 );
		EmitTriangle( ref v0, ref  v5, ref  v1 );
		EmitTriangle( ref v0, ref  v1, ref  v7 );
		EmitTriangle( ref v0, ref  v7, ref v10 );
		EmitTriangle( ref v0, ref v10, ref v11 );
		// 5 adjacent faces
		EmitTriangle( ref  v1, ref  v5, ref v9 );
		EmitTriangle( ref  v5, ref v11, ref v4 );
		EmitTriangle( ref v11, ref v10, ref v2 );
		EmitTriangle( ref v10, ref  v7, ref v6 );
		EmitTriangle( ref  v7, ref  v1, ref v8 );
		// 5 faces around point 3
		EmitTriangle( ref v3, ref v9, ref v4 );
		EmitTriangle( ref v3, ref v4, ref v2 );
		EmitTriangle( ref v3, ref v2, ref v6 );
		EmitTriangle( ref v3, ref v6, ref v8 );
		EmitTriangle( ref v3, ref v8, ref v9 );
		// 5 adjacent faces
		EmitTriangle( ref v4, ref v9, ref  v5 );
		EmitTriangle( ref v2, ref v4, ref v11 );
		EmitTriangle( ref v6, ref v2, ref v10 );
		EmitTriangle( ref v8, ref v6, ref  v7 );
		EmitTriangle( ref v9, ref v8, ref  v1 );
		
		for( var i = 0; i < subdivisions; i++ ) {
			Subdivide();
		}
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
	
	public void UnweldVertices() {
		var allTris = GetAllTriangles( false );
		foreach( var tris in allTris ) {
			EmitTriangle( tris.A.Position, tris.B.Position, tris.C.Position );
		}
		
		foreach( var tris in allTris ) {
			DeleteTriangle( tris.Index );
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
	
	public void OptimizeEdgeVertices() {
		t_weldMap.Clear();
		
		var allVerts = new HashSet<Vertex>( GetAllVertices( false ) );
		foreach( var vA in allVerts ) {
			if( t_weldMap.ContainsKey( vA.Index ) ) { continue; }	// already processed
			
			var vertexEdges = new List<Edge>();
			foreach( var tris in vA ) {
				vertexEdges.AddRange( tris.Edges );
			}
			
			foreach( var pair in vertexEdges.IteratePairs() ) {
				var edgeA = pair.Item1;
				var edgeB = pair.Item2;
				if( edgeA.B == edgeB.A ) {
					if( Vector3.Angle( edgeA.AB, edgeB.AB ).EpsilonEquals( 0f ) ) {
						var weldGroup = new HashSet<int>();
						weldGroup.Add( edgeA.A.Index, edgeA.B.Index );
						t_weldMap[vA.Index] = weldGroup;
						break;
					}
				}
			}
		}
		
		foreach( var weldGroup in t_weldMap.Values ) {
			_MergeVertices( weldGroup );
		}
	}
	
	public void Subdivide() {
		// TODO: walk live triangles, split them; don't forget ownership data!
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
				_RemoveVertexData( lastAliveIndex, index );
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
		m_uvs.RemoveRange( firstDeadIndex, itemsToRemove );
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
		var triangleIndex = 0;
		while( triangleIndex <= lastAliveIndex ) {
			var indexIndex = triangleIndex *3;
			var isDead = m_deadTriangles.Contains( triangleIndex )
				|| (m_indeces[indexIndex + 0] == c_invalidID)
				|| (m_indeces[indexIndex + 1] == c_invalidID)
				|| (m_indeces[indexIndex + 2] == c_invalidID);
			
			if( isDead ) {
				_RemoveTriangleData( lastAliveIndex, triangleIndex );
				lastAliveIndex -= 1;
			}
			else {
				triangleIndex += 1;
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
	
	// [aliveIndex], source, gets moved to => [deadIndex], destination
	private void _RemoveVertexData( int aliveIndex, int deadIndex ) {
		m_positions.HalfSwap( deadIndex, aliveIndex );
		m_colors.HalfSwap( deadIndex, aliveIndex, 1, true );
		m_uvs.HalfSwap( deadIndex, aliveIndex, 1, true );
		// TODO: also other data, should it arise!
		
		var destOwner = new VertexOwnership( this, deadIndex );
		var sourceOwner = new VertexOwnership( this, aliveIndex );
		destOwner.MoveOwnershipFrom( ref sourceOwner );
	}
	
	private void _RemoveTriangleData( int aliveIndex, int deadIndex ) {
		// ownership:
		foreach( var vert in GetTriangle( deadIndex ) ) {
			vert.m_ownership.RemapOwner( deadIndex, c_invalidID );
		}
		foreach( var vert in GetTriangle( aliveIndex ) ) {
			vert.m_ownership.RemapOwner( aliveIndex, deadIndex );
		}
		
		m_indeces.HalfSwap( deadIndex *3, aliveIndex *3, 3 );
		
		// handling death info (so consequtive tris handling will work):
		var sourceDead = m_deadTriangles.Remove( aliveIndex );
		if( sourceDead ) {
			m_deadTriangles.Add( deadIndex );
		}
		else {
			m_deadTriangles.Remove( deadIndex );
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
		if( target == null )
			return null;
		
		var filterTarget = target as MeshFilter;
		if( filterTarget == null ) {
			filterTarget = target.GetComponent<MeshFilter>();
		}
		
		if( filterTarget == null ) {
			Log.Warning( "Tried to find filter target on '"+target.gameObject.name+"', but no MeshFilter was found there!" );
		}
		return filterTarget;
	}
	
	private void _SyncPropertiesSizes() {
		if( HasColors ) {
			m_colors.Resize( m_positions.Count, Color.white );
		}
		if( HasUVs ) {
			m_uvs.Resize( m_positions.Count, Vector2.zero );
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
