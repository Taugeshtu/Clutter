using System;
using System.Collections.Generic;
using System.Linq;

// Note: I'm not sold yet on my current convention of
// "first we modify _current, then dispatch events"
// need to see in practice if it's handy or an impediment

public abstract class Selection<T> {
	protected HashSet<T> _current = new HashSet<T>();
	protected HashSet<T> _scratch = new HashSet<T>();
	
	public IEnumerable<T> Current => _current;
	
	public event Action a_selectionChanged;			// general-purpose "something changed" event
	public event Action<T, bool> a_itemSelected;	// gives item and whether it was set as selected or not
	
	public void Clear() {
		if( _current.Count == 0 )
			return;
		
		_SetScratch( _current );
		_current.Clear();
		
		_NotifyDrop( _scratch );
		_NotifyChange();
	}
	
	public abstract void Select( T item );
	public abstract void Select( IEnumerable<T> items );
	
	public abstract void Drop( T item );
	public abstract void Drop( IEnumerable<T> items );
	
	public abstract void Toggle( T item );
	public abstract void Toggle( IEnumerable<T> items );
	
#region Internals
	protected HashSet<T> _SetScratch( IEnumerable<T> items ) {
		_scratch.Clear();
		_scratch.Add( items );
		return _scratch;
	}
	
	protected void _NotifyDrop( T item ) { a_itemSelected?.Invoke( item, false ); }
	protected void _NotifyDrop( IEnumerable<T> itemsDropped ) {
		if( a_itemSelected != null )
			foreach( var item in itemsDropped ) a_itemSelected.Invoke( item, false );
	}
	protected void _NotifyPick( T item ) { a_itemSelected?.Invoke( item, true ); }
	protected void _NotifyPick( IEnumerable<T> itemsPicked ) {
		if( a_itemSelected != null )
			foreach( var item in itemsPicked ) a_itemSelected.Invoke( item, true );
	}
	protected void _NotifyChange() { a_selectionChanged?.Invoke(); }
#endregion
}

public class SingleSelection<T> : Selection<T> {
	public override void Select( T item ) {
		if( _current.Contains( item ) ) return;
		
		if( _current.Count == 0 ) {
			_current.Add( item );
		}
		else {
			var dropped = _current.First();
			_current.Clear();
			_current.Add( item );
			_NotifyDrop( dropped );
		}
		
		_NotifyPick( item );
		_NotifyChange();
	}
	
	public override void Drop( T item ) {
		if( !_current.Contains( item ) ) return;
		
		_current.Remove( item );
		
		_NotifyDrop( item );
		_NotifyChange();
	}
	
	public override void Toggle( T item ) {
		if( _current.Count == 0 ) {
			_current.Add( item );
			_NotifyPick( item );
		}
		else {
			var dropped = _current.First();
			if( dropped.Equals( item ) ) {
				_current.Clear();
				_NotifyDrop( dropped );
			}
			else {
				_current.Clear();
				_NotifyDrop( dropped );
				
				_current.Add( item );
				_NotifyPick( item );
			}
		}
		_NotifyChange();
	}
	
	public override void Select( IEnumerable<T> items ) { _ThrowUnsupported(); }
	public override void Drop( IEnumerable<T> items ) { _ThrowUnsupported(); }
	public override void Toggle( IEnumerable<T> items ) { _ThrowUnsupported(); }
	private void _ThrowUnsupported() {
		throw new System.Exception( "Bulk operations not supported for SingleSelection; either use MultiSelection or rework calling code" );
	}
}

public class MultiSelection<T> : Selection<T> {
	private HashSet<T> _scratch2 = new HashSet<T>();
	
	public override void Select( T item ) {
		if( _current.Contains( item ) ) return;
		
		_current.Add( item );
		
		_NotifyPick( item );
		_NotifyChange();
	}
	public override void Select( IEnumerable<T> items ) {
		var added = _SetScratch( items );
		added.ExceptWith( _current );
		if( added.Count == 0 ) return;
		
		_current.Add( added );
		_NotifyPick( added );
		_NotifyChange();
	}
	
	public override void Drop( T item ) {
		if( !_current.Contains( item ) ) return;
		
		_current.Remove( item );
		
		_NotifyDrop( item );
		_NotifyChange();
	}
	public override void Drop( IEnumerable<T> items ) {
		var dropped = _SetScratch( items );
		dropped.IntersectWith( _current );
		if( dropped.Count == 0 ) return;
		
		_current.Remove( dropped );
		_NotifyDrop( dropped );
		_NotifyChange();
	}
	
	public override void Toggle( T item ) {
		if( _current.Contains( item ) ) {
			_current.Remove( item );
			_NotifyDrop( item );
		}
		else {
			_current.Add( item );
			_NotifyPick( item );
		}
		_NotifyChange();
	}
	public override void Toggle( IEnumerable<T> items ) {
		var overlap = _SetScratch( items );
		overlap.IntersectWith( _current );
		
		_scratch2.Clear();
		_scratch2.Add( items );
		_scratch2.ExceptWith( overlap );
		_current.Remove( overlap );
		_NotifyDrop( overlap );
		
		_current.Add( _scratch2 );
		_NotifyPick( _scratch2 );
		
		_NotifyChange();
	}
}