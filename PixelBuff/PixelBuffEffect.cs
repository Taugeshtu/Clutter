using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent( typeof( Camera ) )]
public class PixelBuffEffect : MonoBehaviour {
	[SerializeField] private int m_xBuff = 1;
	[SerializeField] private int m_yBuff = 1;
	private Material m_material;
	
	private Material _material {
		get {
			if( m_material == null ) {
				var shader = Shader.Find( "Hidden/PixelBuff" );
				if( shader == null ) {
					Debug.LogWarning( "Couldn't find PixelBuff shader!" );
				}
				else {
					m_material = new Material( shader );
				}
			}
			return m_material;
		}
	}
	
#region Public
#endregion
	
	
#region UICallbacks
#endregion
	
	
#region UnityCallbacks
	void OnRenderImage( RenderTexture from, RenderTexture to ) {
		var xValue = Screen.width /m_xBuff;
		var yValue = Screen.height /m_yBuff;
		
		_material.SetFloat( "_XBuff", xValue );
		_material.SetFloat( "_YBuff", yValue );
		Graphics.Blit( from, to, _material );
	}
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}
