using UnityEngine;
using System.Collections.Generic;

namespace Clutter {
public class TwoKeyDictionary<TKey1, TKey2, TValue> : Dictionary<TKey1, Dictionary<TKey2, TValue>> {
	
	public TValue this[TKey1 key1, TKey2 key2] {
		get {
			Dictionary<TKey2, TValue> container;
			if( TryGetValue( key1, out container ) ) {
				return container[key2];
			}
			else {
				var newContainer = new Dictionary<TKey2, TValue>();
				this[key1] = newContainer;
				return newContainer[key2];
			}
		}
		set {
			Dictionary<TKey2, TValue> container;
			if( TryGetValue( key1, out container ) ) {
				container[key2] = value;
			}
			else {
				var newContainer = new Dictionary<TKey2, TValue>();
				this[key1] = newContainer;
				newContainer[key2] = value;
			}
		}
	}
	
	public IEnumerable<(TKey1, TKey2)> AllKeys {
		get {
			foreach( var pair in this ) {
				foreach( var key2 in pair.Value.Keys ) {
					yield return (pair.Key, key2);
				}
			}
		}
	}
	
	public IEnumerable<TValue> AllValues {
		get {
			foreach( var container in Values ) {
				foreach( var x in container.Values ) {
					yield return x;
				}
			}
		}
	}
	
#region Implementation
#endregion
	
	
#region Public
	public IEnumerable<(TKey2 key2, TValue item)> GetByFirst( TKey1 key1 ) {
		if( TryGetValue( key1, out var container ) ) {
			foreach( var pair in container )
				yield return (pair.Key, pair.Value);
		}
	}
	
	public IEnumerable<(TKey1 key1, TValue item)> GetBySecond( TKey2 key2 ) {
		foreach( var pair in this ) {
			var key1 = pair.Key;
			var container = pair.Value;
			if( container.TryGetValue( key2, out var item ) )
				yield return (key1, item);
		}
	}
	
	public TValue GetAdd( TKey1 key1, TKey2 key2, TValue template = default( TValue ) ) {
		if( TryGetValue( key1, out var container ) ) {
			return container.GetAdd( key2, template );
		}
		else {
			var newContainer = new Dictionary<TKey2, TValue>();
			this[key1] = newContainer;
			newContainer[key2] = template;
			return template;
		}
	}
	
	public void Add( TKey1 key1, TKey2 key2, TValue x ) {
		if( TryGetValue( key1, out var container ) ) {
			container.Add( key2, x );
		}
		else {
			var newContainer = new Dictionary<TKey2, TValue>();
			this[key1] = newContainer;
			newContainer[key2] = x;
		}
	}
	
	public bool Remove( TKey1 key1, TKey2 key2 ) {
		if( TryGetValue( key1, out var container ) ) {
			return container.Remove( key2 );
		}
		else {
			return false;
		}
	}
	
	public bool ContainsKeys( TKey1 key1, TKey2 key2 ) {
		if( TryGetValue( key1, out var container ) ) {
			return container.ContainsKey( key2 );
		}
		else {
			return false;
		}
	}
	
	public bool ContainsValue( TValue x ) {
		foreach( var container in Values ) {
			if( container.ContainsValue( x ) ) {
				return true;
			}
		}
		return false;
	}
	
	public bool TryGetValue( TKey1 key1, TKey2 key2, out TValue x ) {
		if( TryGetValue( key1, out var container ) ) {
			return container.TryGetValue( key2, out x );
		}
		else {
			x = default( TValue );
			return false;
		}
	}
	
	public void ClearValues() {
		foreach( var container in Values ) {
			container.Clear();
		}
	}
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}
}
