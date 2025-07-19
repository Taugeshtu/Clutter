using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//----------------------------------------------------------------------------------------------------------------------
//							NOTE: if you don't have "Metrics" class just delete the calls.
//----------------------------------------------------------------------------------------------------------------------

public static class CoroutineManager {
	private class CoroutineNote {
		public IEnumerator Handle;
		public Stack<IEnumerator> Wrappers;
		public CoroutineBehavior Behavior;
		public int Tag;
		
		public int AliveWrappers { get; private set; }
		
		public CoroutineNote( IEnumerator handle, CoroutineBehavior behavior, int tag ) {
			Handle = handle;
			Wrappers = new Stack<IEnumerator>();
			Behavior = behavior;
			Tag = tag;
			AliveWrappers = 0;
		}
		
		public void RegisterWrapper( IEnumerator wrapper ) {	// I know this is far from elegant but it should be foul-proof
			Wrappers.Push( wrapper );
			AliveWrappers += 1;
		}
		
		public void TerminateWrapper() {
			AliveWrappers -= 1;
		}
		
		public override string ToString() {
			return "#"+Handle.GetHashCode()+": "+Tag+", wrappers: "+Wrappers.Count+", alive: "+AliveWrappers;
		}
	}
	
	private static Dictionary<IEnumerator, CoroutineNote> s_trackedCoroutines = new Dictionary<IEnumerator, CoroutineNote>();
	private static Dictionary<IEnumerator, CoroutineNote> s_danglingCoroutines = new Dictionary<IEnumerator, CoroutineNote>();	// May be useful one day
	
	public static int DanglingCount { get { return s_danglingCoroutines.Count; } }
	
#region Public
	public static void StartDangling( IEnumerator newCoroutine, int tag = 0 ) {
		_StartInternal( newCoroutine, CoroutineBehavior.Dangling, tag );
	}
	
	public static void StartTracked( ref IEnumerator handle, IEnumerator newCoroutine, CoroutineBehavior behavior = CoroutineBehavior.Concurrent, int tag = 0 ) {
		if( behavior == CoroutineBehavior.Dangling ) {
			Debug.LogError( "Dangling coroutines are not allowed to be tracked! Sort out your code, watch the callstack" );
			return;
		}
		
		// Note: insuring that handles are being set properly after StopByTag()
		// ..ideally we'd want to introduce a "CoroutineHandle" class that'd allow us to null the reference.
		if( (handle != null) && !s_trackedCoroutines.ContainsKey( handle ) ) {
			// maybe this needs to stop the ongoing coroutine in the reference that we're not tracking, but hoooow???
			handle = null;
		}
		
		if( behavior == CoroutineBehavior.Restarting ) {
			StopTracked( ref handle );
			
			handle = newCoroutine;
			_StartInternal( newCoroutine, behavior, tag );
		}
		else if( behavior == CoroutineBehavior.IgnoreNew ) {
			if( handle == null ) {
				handle = newCoroutine;
				_StartInternal( newCoroutine, behavior, tag );
			}
		}
		else if( behavior == CoroutineBehavior.Concurrent ) {
			if( handle == null ) {
				handle = newCoroutine;
				_StartInternal( newCoroutine, behavior, tag );
			}
			else {
				var note = s_trackedCoroutines[handle];
				var wrapper = _Wrapper( newCoroutine, note );
				note.RegisterWrapper( wrapper );
				s_coroutineManagerObj.StartCoroutine( wrapper );
			}
		}
	}
	
	public static void StopTracked( ref IEnumerator handle ) {
		_StopInternal( handle );
		handle = null;
	}
	
	public static void StopByTag( int tag ) {
		var notesToRemove = new List<CoroutineNote>();
		var allNotes = new List<CoroutineNote>( s_trackedCoroutines.Values );
		allNotes.AddRange( s_danglingCoroutines.Values );
		
		foreach( var note in allNotes ) {
			if( note.Tag == tag ) {
				_StopInternal( note.Handle );
				notesToRemove.Add( note );
			}
		}
		
		foreach( var note in notesToRemove ) {
			s_trackedCoroutines.Remove( note.Handle );
			s_danglingCoroutines.Remove( note.Handle );
		}
	}
	
	public static int CountByHandle( IEnumerator handle ) {
		if( !s_trackedCoroutines.ContainsKey( handle ) ) {
			return 0;
		}
		
		return s_trackedCoroutines[handle].AliveWrappers;
	}
	
	public static int CountByTag( int tag ) {
		var result = 0;
		foreach( var note in s_trackedCoroutines.Values ) {
			if( note.Tag == tag ) {
				result += note.AliveWrappers;
			}
		}
		
		foreach( var note in s_danglingCoroutines.Values ) {
			if( note.Tag == tag ) {
				result += note.AliveWrappers;
			}
		}
		return result;
	}
#endregion
	
	
#region Private
	private static void _StartInternal( IEnumerator coroutine, CoroutineBehavior behavior, int tag ) {
		var isDangling = (behavior == CoroutineBehavior.Dangling);
		var container = isDangling ? s_danglingCoroutines : s_trackedCoroutines;
		var note = new CoroutineNote( coroutine, behavior, tag );
		var wrapper = _Wrapper( coroutine, note );
		note.RegisterWrapper( wrapper );
		
		container.Add( coroutine, note );
		s_coroutineManagerObj.StartCoroutine( wrapper );
	}
	
	private static IEnumerator _Wrapper( IEnumerator nested, CoroutineNote note ) {
		// Metrics.Accumulate( MetricsKey.CoroutinesRunning );
		
		while( nested.MoveNext() ) {
			var result = nested.Current;
			yield return result;
		}
		
		// In case the wrapper shuts down gracefully - without forceful stop
		_OnWrapperStopped( note );
	}
	
	private static void _StopInternal( IEnumerator handle ) {
		if( handle == null ) { return; }
		if( !s_trackedCoroutines.ContainsKey( handle ) ) { return; }
		
		var note = s_trackedCoroutines[handle];
		while( note.AliveWrappers > 0 ) {
			var wrapper = note.Wrappers.Pop();
			s_coroutineManagerObj.StopCoroutine( wrapper );
			_OnWrapperStopped( note );
		}
	}
	
	private static void _OnWrapperStopped( CoroutineNote note ) {
		note.TerminateWrapper();
		
		var isDangling = (note.Behavior == CoroutineBehavior.Dangling);
		var container = isDangling ? s_danglingCoroutines : s_trackedCoroutines;
		if( note.AliveWrappers == 0 ) {
			container.Remove( note.Handle );
		}
	}
#endregion
	
	
#region Utility
	private static string s_coroutineObjName = "_CoroutineObject";
	private static MonoBehaviour s_coroutineManagerObj {
		get {
			var coroutineObj = GameObject.Find( s_coroutineObjName );
			
			if( coroutineObj == null ) {
				coroutineObj = new GameObject();
				coroutineObj.name = s_coroutineObjName;
			}
			
			var coroutineBehaviour = coroutineObj.GetAddComponent<CoroutineHolder>();
			return coroutineBehaviour;
		}
	}
	
	public enum CoroutineBehavior {
		Restarting,
		IgnoreNew,
		Concurrent,
		Dangling
	}
	
	private static void _DumpTracked() {
		var debs = "Tracked routines:";
		foreach( var pair in s_trackedCoroutines ) {
			debs += "\n"+pair.Value;
		}
		Debug.LogError( debs );
	}
#endregion
}
