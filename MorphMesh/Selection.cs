using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Clutter.Mesh {
public abstract class Selection : IEnumerable<Triangle>, IEnumerable, ICollection<Triangle> {
	private MorphMesh m_mesh;
	private bool m_outlineDirty = false;
	private HashSet<Triangle> m_selection;
	private HashSet<Triangle> m_outline = new HashSet<Triangle>();
	
	public long Generation;
	
	public HashSet<Triangle> Outline {
		get {
			_UpdateOutline();
			return m_outline;
		}
	}
	
#region Implementation
	public Selection( MorphMesh mesh, IEnumerable<Vertex> vertices ) : this( mesh, null, vertices ) {}
	public Selection( MorphMesh mesh, IEnumerable<Triangle> triangles = null, IEnumerable<Vertex> vertices = null ) {
		m_mesh = mesh;
		Generation = mesh.m_generation;
		
		m_selection = new HashSet<Triangle>( triangles );
	}
	
	// IEnumerable
	System.Collections.IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
	public IEnumerator<Triangle> GetEnumerator() {
		return m_selection.GetEnumerator();
	}
	
	// ICollection
	public int Count { get { return m_selection.Count; } }
	public bool IsReadOnly { get { return false; } }
	
	void ICollection<Triangle>.Add( Triangle item ) {
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
	
	// Note: will re-consider these ops later, when selection is working
	/*
	public void ExceptWith( IEnumerable<Triangle> items ) { m_selection.ExceptWith( items ); }
	public void IntersectWith( IEnumerable<Triangle> items ) { m_selection.IntersectWith( items ); }
	public void SymmetricExceptWith( IEnumerable<Triangle> items ) { m_selection.SymmetricExceptWith( items ); }
	public void UnionWith( IEnumerable<Triangle> items ) { m_selection.UnionWith( items ); }
	public bool Overlaps( IEnumerable<Triangle> items ) { return m_selection.Overlaps( items ); }
	*/
#endregion
	
	
#region Public
	public void Expand( bool byVertices = false ) {
		_UpdateOutline();
		
		var addedTriangles = new HashSet<Triangle>();
		
		m_outlineDirty = true;
		foreach( var trisA in m_outline ) {
			var neighbours = byVertices ? trisA.GetVertexNeighbours() : trisA.GetEdgeNeighbours();
			foreach( var trisB in neighbours ) {
				if( trisB.Index == trisA.Index ) { continue; }	// fast escape
				if( m_selection.Contains( trisB ) ) { continue; }
				
				addedTriangles.Add( trisB );
			}
		}
		
		// TODO: update the outline
	}
#endregion
	
	
#region Private
	private void _UpdateOutline() {
		if( m_outlineDirty == false ) { return; }
		m_outlineDirty = false;
		
		// TODO: seeking the outline!
	}
#endregion
	
	
#region Temporary
#endregion
}
}
