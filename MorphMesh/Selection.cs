using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Clutter.Mesh {
public abstract class Selection<T> : IEnumerable<T>, IEnumerable, ICollection<T>, ISet<T> {
	protected enum Kind {
		Invalid,
		Vertex,
		Triangle
	}
	
	protected MorphMesh m_mesh;
	protected Kind m_kind = Kind.Invalid;
	protected bool m_outlineDirty = false;
	protected HashSet<T> m_selection = new HashSet<T>();
	protected HashSet<T> m_outline = new HashSet<T>();
	
	public long Generation;
	
	public HashSet<T> Outline {
		get {
			_UpdateOutline();
			return m_outline;
		}
	}
	
#region Implementation
	public Selection( MorphMesh mesh ) {
		m_mesh = mesh;
		Generation = mesh.m_generation;
		
		if( typeof( T ) == typeof( Vertex ) ) {
			m_kind = Kind.Vertex;
		}
		if( typeof( T ) == typeof( Triangle ) ) {
			m_kind = Kind.Triangle;
		}
		
		if( m_kind == Kind.Invalid ) {
			throw new System.Exception( "Unsupported selection typing! Expected 'Vertex' or 'Triangle', got: "+typeof(T) );
		}
	}
	
	// IEnumerable
	System.Collections.IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
	public IEnumerator<T> GetEnumerator() {
		return m_selection.GetEnumerator();
	}
	
	// ICollection
	public int Count { get { return m_selection.Count; } }
	public bool IsReadOnly { get { return false; } }
	
	void ICollection<T>.Add( T item ) {
		m_selection.Add( item );
	}
	
	public bool Remove( T item ) {
		return m_selection.Remove( item );
	}
	
	public void Clear() {
		m_selection.Clear();
		m_outline.Clear();
	}
	
	public bool Contains( T item ) { return m_selection.Contains( item ); }
	public void CopyTo( T[] target, int startIndex ) { m_selection.CopyTo( target, startIndex ); }
	
	// ISet
	public bool Add( T item ) {
		return m_selection.Add( item );
	}
	
	public void ExceptWith( IEnumerable<T> items ) {
		m_selection.ExceptWith( items );
	}
	
	public void IntersectWith( IEnumerable<T> items ) {
		m_selection.IntersectWith( items );
	}
	
	public void SymmetricExceptWith( IEnumerable<T> items ) {
		m_selection.SymmetricExceptWith( items );
	}
	
	public void UnionWith( IEnumerable<T> items ) {
		m_selection.UnionWith( items );
	}
	
	public bool IsSubsetOf( IEnumerable<T> items ) { return m_selection.IsSubsetOf( items ); }
	public bool IsSupersetOf( IEnumerable<T> items ) { return m_selection.IsSupersetOf( items ); }
	public bool IsProperSubsetOf( IEnumerable<T> items ) { return m_selection.IsProperSubsetOf( items ); }
	public bool IsProperSupersetOf( IEnumerable<T> items ) { return m_selection.IsProperSupersetOf( items ); }
	public bool Overlaps( IEnumerable<T> items ) { return m_selection.Overlaps( items ); }
	public bool SetEquals( IEnumerable<T> items ) { return m_selection.SetEquals( items ); }
#endregion
	
	
#region Public
	public void Expand() {
		if( m_kind == Kind.Vertex ) {
			_ExpandVertices();
		}
		if( m_kind == Kind.Triangle ) {
			_ExpandTriangles();
		}
	}
#endregion
	
	
#region Private
	private void _ExpandVertices() {
		foreach( var item in m_outline ) {
			var outlineVertex = (Vertex)(object)item;
			
			foreach( var triangle in outlineVertex ) {
				foreach( var vertex in triangle ) {
					var genericVertex = 
					if( !m_selection.Contains( (T) vertex ) ) {
						
					}
				}
			}
		}
	}
	
	private void _ExpandTriangles() {
		
	}
	
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
