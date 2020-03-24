using UnityEngine;
using System.Collections.Generic;

namespace Clutter.Mesh {
public struct Ownership {
	private MorphMesh m_mesh;
	private Dictionary<int, HashSet<int>> m_data;
	
	/*
	public HashSet<int> this[int vertexIndex] {
		get {
			if( m_data.ContainsKey( vertexIndex ) ) {
				return m_data[vertexIndex];
			}
			else {
				var owners = new HashSet<int>();
				m_data[vertexIndex] = owners;
				return owners;
			}
		}
	}
	*/
	
#region Implementation
	public Ownership( MorphMesh mesh, IEnumerable<Vertex> vertices ) : this( mesh, _Unwrap( vertices ) ) {}
	public Ownership( MorphMesh mesh, IEnumerable<int> vertexIndeces ) {
		m_mesh = mesh;
		m_data = new Dictionary<int, HashSet<int>>();
		
		var dog = new WatchDog( "Building ownership data" );
		
		foreach( var vertexIndex in vertexIndeces ) {
			var owners = new HashSet<int>();
			m_data[vertexIndex] = owners;
		}
		
		var trianglesCount = m_mesh.m_indeces.Count /3;
		for( var triangleIndex = 0; triangleIndex < trianglesCount; triangleIndex++ ) {
			var indexIndex = triangleIndex *3;
			for( var i = 0; i < 3; i++ ) {
				var vertexIndex = m_mesh.m_indeces[indexIndex + i];
				if( m_data.ContainsKey( vertexIndex ) ) {
					m_data[vertexIndex].Add( triangleIndex );
				}
			}
		}
		dog.Stop();
	}
	
	public override string ToString() {
		const string separator = ", ";
		
		var result = new System.Text.StringBuilder( 100 );
		foreach( var pair in m_data ) {
			result.Append( "\nv" ).Append( pair.Key ).Append( " owners: " );
			result.Append( "(" ).Append( pair.Value.Count ).Append( ") " );
			foreach( var owner in pair.Value ) {
				result.Append( owner ).Append( separator );
			}
			if( pair.Value.Count > 0 ) { result.TrimEnd( separator.Length ); }
		}
		
		return result.ToString();
	}
#endregion
	
	
#region Public
#endregion
	
	
#region Private
	private static IEnumerable<int> _Unwrap( IEnumerable<Vertex> vertices ) {
		foreach( var vertex in vertices ) {
			yield return vertex.Index;
		}
	}
#endregion
	
	
#region Temporary
#endregion
}
}
