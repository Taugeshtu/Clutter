using System;
using System.Collections.Generic;
using System.Linq;

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
	
	public Swapchain( T a, T b ) : this( new T[] { a, b } ) {}
	public Swapchain( T a, T b, T c ) : this( new T[] { a, b, c } ) {}
	public Swapchain( T a, T b, T c, T d ) : this( new T[] { a, b, c, d } ) {}
	public Swapchain( T[] items ) {
		_depth = items.Length;
		_items = items;
	}
	
	public Swapchain( IEnumerable<T> items ) {
		_depth = items.Count();
		_items = new T[_depth];
		var index = 0;
		foreach( var item in items ) {
			_items[index] = item;
			index += 1;
		}
	}
	
	public void Tick() {
		_counter = (_counter + 1) %_depth;
	}
}
