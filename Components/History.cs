using Clutter;

public class History<T> {
	private readonly SizedList<T> _items;
	private int _currentIndex = -1;
	
	public int Count => _items.Count;
	
	public History( int maxHistorySize = 1000 ) {
		_items = new SizedList<T>( maxHistorySize );
	}
	
	public T Record( T item ) {
		_items.Add( item );
		_currentIndex = _items.Count - 1;
		return _items[_currentIndex];
	}
	
	public T Undo() {
		if( _currentIndex > 0 ) {
			_currentIndex--;
		}
		return _items[_currentIndex];
	}
	
	public T Redo() {
		if( _currentIndex < _items.Count - 1 ) {
			_currentIndex++;
		}
		return _items[_currentIndex];
	}
}