using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Clutter {
[SingularBehaviour( false, true, true )]
public class LogsDisplay : MonoSingular<LogsDisplay> {
	private static Queue<(Log.Tag tag, string message)> _history = new Queue<(Log.Tag, string)>();
	private static bool _historyDirty;
	
	[SerializeField] private ScrollRect _scrollable;
	[SerializeField] private Transform _contentRoot;
	[SerializeField] private GameObject _itemPrefab;
	[SerializeField] private int _historySize = 50;
	
	private List<GameObject> _spawned = new List<GameObject>();
	
	public static void Push( Log.Tag tag, string message ) {
		if( s_Instance == null )
			return;
		
		_historyDirty = true;
		
		_history.Enqueue( (tag, message) );
		if( _history.Count > s_Instance._historySize )
			_history.Dequeue();
	}
	
	void Update() {
		if( !_historyDirty )
			return;
		
		var logsCount = _history.Count;
		var itemsCount = Mathf.Max( _spawned.Count, logsCount );
		for( var i = 0; i < itemsCount; i++ ) {
			if( i < logsCount ) {
				if( i >= _spawned.Count ) {
					var newLine = Instantiate( _itemPrefab, _contentRoot );
					_spawned.Add( newLine );
				}
				
				var log = _history.Dequeue();
				var display = _spawned[i];
				display.SetActive( true );
				display.GetComponent<TMP_Text>().text = _FormatLog( log );
				
				_history.Enqueue( log );
			}
			else if( i < _spawned.Count ) {
				_spawned[i].SetActive( false );
			}
		}
	}
	
	private string _FormatLog( (Log.Tag tag, string message) log ) {
		return _ToColorTag( log.tag )+log.message;
	}
	
	private string _ToColorTag( Log.Tag tag ) {
		if( tag == Log.Tag.Message ) return "<color=#FFFFFF>";
		if( tag == Log.Tag.Warning ) return "<color=#FFFF00>";
		if( tag == Log.Tag.Error ) return "<color=#FF0000>";
		return "";
	}
}
}
