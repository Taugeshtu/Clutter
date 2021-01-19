using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

public static class FuncRecorder {
	public static bool s_enabled = true;
	
#region Implementation
#endregion
	
	
#region Public
	public static void Record() {
		#if UNITY_EDITOR && !INSTRUMENTATION_OFF
		if( !s_enabled ) {
			return;
		}
		
		var frame = new StackFrame( 1 );
		var methodName = frame.GetMethod();
		
		
		#endif
	}
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}
