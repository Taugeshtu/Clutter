using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Clutter {
public static class Log {
	public enum Tag {
		Message,
		Warning,
		Error
	}
	
	public static string Prefix;
	public static bool PushToDisplay = false;
	
	public static void Message( string message, UnityEngine.Object context = null ) {
		Message( message, PushToDisplay, context );
	}
	public static void Warning( string message, UnityEngine.Object context = null ) {
		Warning( message, PushToDisplay, context );
	}
	public static void Error( string message, UnityEngine.Object context = null ) {
		Error( message, PushToDisplay, context );
	}
	
	public static void Message( string message, bool pushToDisplay, UnityEngine.Object context = null ) {
		Debug.Log( Prefix+message, context );
		if( pushToDisplay )
			LogsDisplay.Push( Tag.Message, message );
	}
	public static void Warning( string message, bool pushToDisplay, UnityEngine.Object context = null ) {
		Debug.LogWarning( Prefix+message, context );
		if( pushToDisplay )
			LogsDisplay.Push( Tag.Warning, message );
	}
	public static void Error( string message, bool pushToDisplay, UnityEngine.Object context = null ) {
		Debug.LogError( Prefix+message, context );
		if( pushToDisplay )
			LogsDisplay.Push( Tag.Error, message );
	}
}
}
