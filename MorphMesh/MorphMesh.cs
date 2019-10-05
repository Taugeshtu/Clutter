using UnityEngine;
using System.Collections.Generic;

namespace Clutter.Mesh {


/* Implementation Notes:
 - vertices are addressed via ID. NOT INDEX
 - This ought to help with sorting and stuff?

First thing that's needed:
 - Store positions in a sensical manner
 - Store triangles data - which vertices make which triangles
 - Make that data double-linked with a big-ass (x6 per vertex) "static" matrix of triangles-for-vertex
 - Make additional "expansion" sparse storage for vertices that do participate in more than 6 triangles
*/


public class MorphMesh {
	private List<int> m_vertexIDs = new List<int>( 300 );
	private List<Vector3> m_vertices = new List<Vector3>( 300 );
	
	public MeshFilter Target { get; private set; }
	
#region Implementation
#endregion
	
	
#region Mesh ops
	public void Read( GameObject target ) {
		Read( target.transform );
	}
	
	public void Read( Component target = null ) {
		var filterTarget = _GetFilterTarget( target );
		// TODO: implement actual reading from the mesh
	}
	
	public void Write( GameObject target ) {
		Write( target.transform );
	}
	
	public void Write( Component target = null ) {
		var filterTarget = _GetFilterTarget( target );
		// TODO: implement actual writing to the mesh
	}
	
	public void Clear() {
		
	}
#endregion
	
	
#region Vertex ops
#endregion
	
	
#region Triangle ops
#endregion
	
	
#region Private
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
	
	
#region Temporary
#endregion
}
}
