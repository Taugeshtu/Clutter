using UnityEngine;
using System.Collections.Generic;

namespace Clutter {
public class Mapping<TKey, TValue> : Dictionary<TKey, TValue> {
	private Dictionary<TValue, TKey> m_invertedMap;
	
	public new TValue this[TKey key] {
		get {
			return base[key];
		}
		set {
			base[key] = value;
			m_invertedMap[value] = key;
		}
	}
	
#region Implementation
	public Mapping() : base() {
		m_invertedMap = new Dictionary<TValue, TKey>();
	}
	public Mapping( IDictionary<TKey,TValue> original ) : base( original ) {
		m_invertedMap = new Dictionary<TValue, TKey>();
		foreach( var pair in original ) {
			m_invertedMap.Add( pair.Value, pair.Key );
		}
	}
	public Mapping( int capacity ) : base( capacity ) {
		m_invertedMap = new Dictionary<TValue, TKey>( capacity );
	}
#endregion
	
	
#region Public
	public TKey GetByValue( TValue x ) {
		return m_invertedMap[x];
	}
	
	public new void Add( TKey key, TValue x ) {
		base.Add( key, x );
		m_invertedMap.Add( x, key );
	}
	
	public new void Clear() {
		base.Clear();
		m_invertedMap.Clear();
	}
	
	public new bool Remove( TKey key ) {
		var x = base[key];
		return base.Remove( key ) && m_invertedMap.Remove( x );
	}
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}
}
