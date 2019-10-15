using UnityEngine;
using System.Collections.Generic;

namespace Clutter {
public class CircularCache<TKey, TValue> {
	private int m_insertionIndex = 0;
	private List<TValue> m_buffer;
	private Mapping<TKey, int> m_index;
	
	public int Capacity { get; private set; }
	
	
#region Implementation
	public CircularCache( int capacity ) {
		ReInitialize( capacity );
	}
#endregion
	
	
#region Public
	public void ReInitialize() {
		ReInitialize( Capacity );
	}
	public void ReInitialize( int capacity ) {
		if( capacity < 1 ) { throw new System.Exception( "Circular cache can't have no capacity!" ); }
		
		Capacity = capacity;
		
		m_insertionIndex = 0;
		m_buffer = new List<TValue>( capacity );
		m_index = new Mapping<TKey, int>( capacity );
	}
	
	public void Clear() {
		m_insertionIndex = 0;
		m_buffer.Clear();
		m_index.Clear();
	}
	
	public void Push( TKey key, TValue item, System.Action<TValue> poppedCallback = null ) {
		if( m_index.ContainsKey( key ) ) { return; }
		
		m_insertionIndex = m_insertionIndex %Capacity;
		if( m_insertionIndex > m_buffer.Count ) {
			throw new System.Exception( "Overpushed. Buffer size: "+m_buffer+", capacity: "+Capacity+", insert index: "+m_insertionIndex );
		}
		
		if( m_insertionIndex == m_buffer.Count ) {
			m_buffer.Add( item );
		}
		else {
			if( poppedCallback != null ) {
				poppedCallback( m_buffer[m_insertionIndex] );
			}
			
			m_buffer[m_insertionIndex] = item;
			var keyToForget = m_index.GetByValue( m_insertionIndex );
			m_index.Remove( keyToForget );
		}
		m_index.Add( key, m_insertionIndex );
		
		m_insertionIndex += 1;
	}
	
	public bool Has( TKey key ) {
		return m_index.ContainsKey( key );
	}
	
	public TValue Get( TKey key ) {
		var itemIndex = m_index[key];
		return m_buffer[itemIndex];
	}
	
	public string Dump() {
		var result = new System.Text.StringBuilder();
		
		result.Append( "Circular cache, capacity: " );
		result.Append( Capacity );
		result.Append( ", currently holding: " );
		result.Append( m_buffer.Count );
		result.Append( "\n" );
		result.Append( "Next insertion at: " );
		result.Append( m_insertionIndex );
		result.Append( "\nBuffer: " );
		result.Append( m_buffer.Dump() );
		result.Append( "\nIndex: " );
		result.Append( m_index.Dump() );
		
		return result.ToString();
	}
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}
}