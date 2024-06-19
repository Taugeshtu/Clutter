using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Clutter {
// Circular buffer kind of storage; grows up to the size of pre-defined capacity, then starts evacuating oldest elements first
public class SizedList<T> : IEnumerable<T>, IEnumerable {
	private List<T> m_buffer;
	private int m_insertionIndex = 0;
	
	public int Count { get { return m_buffer.Count; } }
	public int Capacity { get; private set; }
	
	// "Sort by recent" means index == 0 will return the most recent element, index == 1 will return the second most recent, etc
	// sortByRecent == false means index ==0 will return the oldest element
	public T this[int index, bool sortByRecent = true] {
		get {
			var count = Count;
			var capacity = Capacity;
			if( index > count || index < 0 || count == 0 ) {
				throw new System.IndexOutOfRangeException( "Requested item #"+index+", but so far list only has "+count+" items" );
			}
			
			if( sortByRecent ) {
				// if limiting by Count is needed, that should be done in calling code
				var accessIndex = (m_insertionIndex - 1 - index + capacity) %capacity;
				return m_buffer[accessIndex];
			}
			else {
				var accessIndex = (index + m_insertionIndex) %count;
				return m_buffer[accessIndex];
			}
		}
		set {
			var count = Count;
			var capacity = Capacity;
			if( index > count || index < 0 || count == 0 ) {
				throw new System.IndexOutOfRangeException( "Wanting to set item #"+index+", but so far list only has "+count+" slots" );
			}
			
			if( sortByRecent ) {
				// if limiting by Count is needed, that should be done in calling code
				var accessIndex = (m_insertionIndex - 1 - index + capacity) %capacity;
				m_buffer[accessIndex] = value;
			}
			else {
				var accessIndex = (index + m_insertionIndex) %count;
				m_buffer[accessIndex] = value;
			}
		}
	}
	
#region Implementation
	public SizedList( int capacity ) {
		ReInitialize( capacity );
	}
	
	// These return the elements in the "most recent - first" fashion
	System.Collections.IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
	public IEnumerator<T> GetEnumerator() {
		var count = Count;
		var capacity = Capacity;
		for( var i = 0; i < count; i++ ) {
			var accessIndex = (m_insertionIndex - 1 - i + capacity) %capacity;
			yield return m_buffer[accessIndex];
		}
	}
#endregion
	
	
#region Public
	public void ReInitialize( int capacity ) {
		if( capacity < 1 ) { throw new System.Exception( "CircularList can't have no capacity!" ); }
		
		Capacity = capacity;
		
		m_insertionIndex = 0;
		m_buffer = new List<T>( capacity );
	}
	
	public void Clear() {
		m_insertionIndex = 0;
		m_buffer.Clear();
	}
	
	public void Add( T item, System.Action<T> poppedCallback = null ) {
		m_insertionIndex = m_insertionIndex %Capacity;
		
		if( m_insertionIndex == m_buffer.Count ) {
			m_buffer.Add( item );
		}
		else {
			poppedCallback?.Invoke( m_buffer[m_insertionIndex] );
			m_buffer[m_insertionIndex] = item;
		}
		
		m_insertionIndex += 1;
	}
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}
}
