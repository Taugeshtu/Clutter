using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Clutter.Mesh {
// A transient struct to get/set vertex data
public struct Vertex : IEnumerable<Triangle>, IEnumerable, IEquatable<Vertex> {
	private MorphMesh m_mesh;
	internal VertexOwnership m_ownership;
	private List<Vector3> _positions { get { return m_mesh.m_positions; } }
	private List<Color> _colors { get { return m_mesh.m_colors; } }
	// TODO: UVs, colors, etc
	
	public long Generation;
	public int Index;
	
	public bool IsValid {
		get {
			return (Index != MorphMesh.c_invalidID) && (Generation == m_mesh.m_generation);
		}
	}
	
	public Vector3 Position {
		get { return _positions[Index]; }
		set { _positions[Index] = value; }
	}
	
	public Color Color {
		get { return _colors.GetAt( Index, Color.white ); }
		set { _colors.SetAt( Index, value, Color.white ); }
	}
	
#region Implementation
	internal Vertex( MorphMesh mesh, int index ) {
		m_mesh = mesh;
		Generation = mesh.m_generation;
		Index = index;
		m_ownership = new VertexOwnership( mesh, index );
	}
	
	System.Collections.IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
	public IEnumerator<Triangle> GetEnumerator() {
		foreach( var ownerID in m_ownership ) {
			yield return m_mesh.GetTriangle( ownerID );
		}
	}
	
	public override bool Equals( object other ) {
		if( other is Vertex ) {
			return Equals( (Vertex) other );
		}
		return false;
	}
	public bool Equals( Vertex other ) {
		return (Generation == other.Generation) && (Index == other.Index) && (m_mesh == other.m_mesh);
	}
	public override int GetHashCode() {
		return (int) Generation + 23 *Index;
	}
	
	public static bool operator ==( Vertex a, Vertex b ) {
		return a.Equals( b );
	}
	public static bool operator !=( Vertex a, Vertex b ) {
		return !a.Equals( b );
	}
#endregion
	
	
#region Public
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}
}
