using System.Linq;
using System.Collections.Generic;

public class DictList<TKey, TItem> : Dictionary<TKey, List<TItem>> {
	public int ItemsCount => Values.Sum( c => c.Count );
	
	public new List<TItem> this[TKey key] {
		get {
			if( TryGetValue( key, out var container ) ) {
				return container;
			}
			else {
				var newContainer = new List<TItem>();
				this[key] = newContainer;
				return newContainer;
			}
		}
		set {
			base[key] = value;
		}
	}
	
	public IEnumerable<TItem> AllItems {
		get {
			foreach( var container in Values )
			foreach( var item in container )
				yield return item;
		}
	}
	
	public void Add( TKey key, TItem item ) {
		this[key].Add( item );
	}
	
	public void Remove( TKey key, TItem item ) {
		this[key].Remove( item );
	}
}
