using UnityEngine;
using System.Collections.Generic;

namespace Clutter {
public class Pool<T> where T : UnityEngine.Object {
	private bool m_isGOPool = false;
	private T m_prefab;
	private Transform m_root;
	
	private HashSet<T> m_aliveItems = new HashSet<T>();
	private Stack<T> m_deadItems = new Stack<T>( 100 );
	
#region Implementation
	public Pool( T prefab, Transform root = null, bool autospawnRoot = true ) {
		m_isGOPool = (typeof( T ) == typeof( GameObject ));
		m_prefab = prefab;
		m_root = root;
		
		if( (m_root == null) && autospawnRoot ) {
			m_root = new GameObject( "Root_"+typeof( T ) ).transform;
		}
	}
#endregion
	
	
#region Public
	public void Pump( int amount ) {
		for( var i = 0; i < amount; i++ ) {
			var spawned = _Spawn();
			_MakeDead( spawned );
		}
	}
	
	public T Get() {
		var item = (m_deadItems.Count > 0) ? m_deadItems.Pop() : _Spawn();
		_MakeAlive( item );
		return item;
	}
	
	public void Release( T item ) {
		_MakeDead( item );
	}
#endregion
	
	
#region Private
	private void _MakeAlive( T item ) {
		m_aliveItems.Add( item );
		
		var itemTransform = m_isGOPool ? (item as GameObject).transform : (item as Component).transform;
		itemTransform.gameObject.SetActive( true );
	}
	
	private void _MakeDead( T item ) {
		m_aliveItems.Remove( item );
		m_deadItems.Push( item );
		
		var itemTransform = m_isGOPool ? (item as GameObject).transform : (item as Component).transform;
		itemTransform.gameObject.SetActive( false );
		itemTransform.SetAsLastSibling();
	}
	
	private T _Spawn() {
		var item = GameObject.Instantiate( m_prefab );
		var itemTransform = m_isGOPool ? (item as GameObject).transform : (item as Component).transform;
		
		if( m_root != null ) {
			itemTransform.SetParent( m_root );
		}
		itemTransform.localPosition = Vector3.zero;
		itemTransform.localRotation = Quaternion.identity;
		
		return item;
	}
#endregion
	
	
#region Temporary
#endregion
}
}
