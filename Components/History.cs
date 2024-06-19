using Clutter;

public class History<T> {
	private readonly SizedList<T> _items;
	private int _currentIndex = -1;
	private int _redoDepth = 0;
	
	public int UndoCount => _currentIndex;
	public int RedoCount => _redoDepth;
	
	public History( int maxHistorySize = 1000 ) {
		_items = new SizedList<T>( maxHistorySize );
	}
	
	public T Record( T item ) {
		_currentIndex++;
		if( _currentIndex == _items.Count )
			_items.Add( item );
		else
			_items[_currentIndex, false] = item;
		
		if( _currentIndex == _items.Capacity )
			_currentIndex = _items.Capacity - 1;
		_redoDepth = 0;
		return item;
	}
	
	public T Undo() {
		if( _currentIndex > 0 ) {
			_currentIndex--;
			_redoDepth++;
		}
		return _items[_currentIndex, false];
	}
	
	public T Redo() {
		if( _redoDepth > 0 ) {
			_redoDepth--;
			_currentIndex++;
		}
		return _items[_currentIndex, false];
	}
}