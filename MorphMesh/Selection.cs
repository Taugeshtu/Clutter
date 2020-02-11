using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Clutter.Mesh {
public class Selection : IEnumerable<Triangle>, IEnumerable, ICollection<Triangle> {
	private static HashSet<Triangle> t_triangles = new HashSet<Triangle>();
	
	private MorphMesh m_mesh;
	private bool m_outlineDirty = true;
	private HashSet<Triangle> m_selection;
	private HashSet<Triangle> m_outline = new HashSet<Triangle>();
	
	public long Generation;
	
	public HashSet<Triangle> Outline {
		get {
			_UpdateOutline();
			return m_outline;
		}
	}
	
	public IEnumerable<Vertex> Vertices {
		get {
			foreach( var tris in this ) {
				foreach( var vertex in tris ) {
					yield return vertex;
				}
			}
		}
	}
	
	public IEnumerable<Edge> OutlineEdges {
		get {
			foreach( var tris in Outline ) {
				foreach( var edge in tris.Edges ) {
					if( edge.OwnersCount == 1 ) {
						yield return edge;
					}
				}
			}
		}
	}
	
#region Implementation
	public Selection( MorphMesh mesh, params Triangle[] triangles ) : this( mesh, (IEnumerable<Triangle>) triangles ) {}
	public Selection( MorphMesh mesh, IEnumerable<Triangle> triangles = null ) {
		m_mesh = mesh;
		Generation = mesh.m_generation;
		
		m_selection = (triangles == null) ? new HashSet<Triangle>() : new HashSet<Triangle>( triangles );
	}
	
	// IEnumerable
	System.Collections.IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
	public IEnumerator<Triangle> GetEnumerator() {
		return m_selection.GetEnumerator();
	}
	
	// ICollection
	public int Count { get { return m_selection.Count; } }
	public bool IsReadOnly { get { return false; } }
	
	public void Add( Triangle item ) {
		m_outlineDirty = true;
		m_selection.Add( item );
	}
	
	public bool Remove( Triangle item ) {
		m_outlineDirty = true;
		return m_selection.Remove( item );
	}
	
	public void Clear() {
		m_selection.Clear();
		m_outline.Clear();
	}
	
	public bool Contains( Triangle item ) { return m_selection.Contains( item ); }
	public void CopyTo( Triangle[] target, int startIndex ) { m_selection.CopyTo( target, startIndex ); }
	
	public void UnionWith( IEnumerable<Triangle> items ) { m_selection.UnionWith( items ); }
	
	// Note: will re-consider these ops later, when selection is working
	/*
	public void ExceptWith( IEnumerable<Triangle> items ) { m_selection.ExceptWith( items ); }
	public void IntersectWith( IEnumerable<Triangle> items ) { m_selection.IntersectWith( items ); }
	public void SymmetricExceptWith( IEnumerable<Triangle> items ) { m_selection.SymmetricExceptWith( items ); }
	
	public bool Overlaps( IEnumerable<Triangle> items ) { return m_selection.Overlaps( items ); }
	*/
#endregion
	
	
#region Public
	public void Expand( bool byVertices = false ) {
		_UpdateOutline();
		
		var addedTriangles = new HashSet<Triangle>();
		
		m_outlineDirty = true;
		foreach( var trisA in m_outline ) {
			var neighbours = byVertices ? trisA.VertexNeighbours : trisA.EdgeNeighbours;
			foreach( var trisB in neighbours ) {
				if( trisB.Index == trisA.Index ) { continue; }	// fast escape
				if( m_selection.Contains( trisB ) ) { continue; }
				
				addedTriangles.Add( trisB );
			}
		}
		
		// TODO: smarter way to update an outline
		_UpdateOutline();
	}
	
	private Stack<Triangle> t_workStack = new Stack<Triangle>();
	
	public List<Selection> BreakApart( bool allowTouchingVertices ) {
		t_triangles.Clear();
		
		// TODO: make it a method that walks all the triangles and breaks the selection apart
		t_workStack.Clear();
		var result = new List<Selection>();
		result.Add( new Selection( m_mesh ) );
		result.Add( new Selection( m_mesh ) );
		
		var flip = false;
		foreach( var tris in this ) {
			if( flip ) {
				result[0].Add( tris );
			}
			else {
				result[1].Add( tris );
			}
			flip = !flip;
		}
		
		return result;
	}
#endregion
	
	
#region Private
	private void _UpdateOutline() {
		if( m_outlineDirty == false ) { return; }
		m_outlineDirty = false;
		m_outline.Clear();
		
		foreach( var tris in m_selection ) {
			foreach( var edge in tris.Edges ) {
				if( edge.OwnersCount == 1 ) {
					m_outline.Add( tris );
					break;
				}
			}
		}
	}
#endregion
	
	
#region Temporary
#endregion
}
}
