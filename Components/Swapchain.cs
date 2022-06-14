using System;

public class Swapchain<T> {
	private readonly int _depth;
	private int _counter;
	private T[] _items;
	
	public T A { get { return this[0]; } }
	public T B { get { return this[1]; } }
	public T C { get { return this[2]; } }
	public T D { get { return this[3]; } }
	
	public T this[int index] { get { return _items[(index + _counter) %_depth]; } }
	
	public Swapchain( int depth, Func<T> spawner ) {
		_depth = depth;
		_items = new T[_depth];
		for( var i = 0; i < _depth; i++ )
			_items[i] = spawner();
	}
	
	public void Tick() {
		_counter = (_counter + 1) %_depth;
	}
}
