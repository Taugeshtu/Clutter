using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

public class WatchDog {
	private Stopwatch m_watch;
	
	public string Name { get; private set; }
	public long MS {
		get {
			return m_watch.ElapsedMilliseconds;
		}
	}
	
	public long MCS {
		get {
			return m_watch.ElapsedTicks /10;
		}
	}
	
#region Implementation
	public WatchDog( string name, bool autoStart = true ) {
		m_watch = new Stopwatch();
		Name = name;
		
		if( autoStart ) {
			Start();
		}
	}
#endregion
	
	
#region Public
	public void Start( bool reset = true ) {
		if( reset ) {
			m_watch.Reset();
		}
		m_watch.Start();
	}
	
	public void Stop( bool autoReport = true ) {
		m_watch.Stop();
		
		if( autoReport ) {
			Log();
		}
	}
	
	public void Pause() {
		Stop( false );
	}
	
	public void Resume() {
		Start( false );
	}
	
	public string GetReport() {
		return Name+": "+MS+" ms, "+m_watch.ElapsedTicks+" ticks";
	}
	
	public void Log() {
		Clutter.Log.Error( GetReport() );
	}
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}
