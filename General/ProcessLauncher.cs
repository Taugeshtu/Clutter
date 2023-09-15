using System;
using System.IO;
using System.Diagnostics;

public static class ProcessLauncher {
	public static Process Launch(
	string path, string args = "", bool printOutput = true, bool printErrors = true,
	Action<string> onOutput = null, Action<string> onError = null, Action onDone = null ) {
		var exeName = new FileInfo( path ).Name;
		var process = new Process();
		process.StartInfo.FileName = path;
		process.StartInfo.Arguments = args;
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		process.EnableRaisingEvents = true;
		
		Action<string> Log = (message) => Clutter.Log.Message( "("+exeName+"): "+message );
		Action<string> LogError = (message) => Clutter.Log.Error( "("+exeName+"): "+message );
		
		process.OutputDataReceived += (sender, e) => {
			if( !string.IsNullOrEmpty( e.Data ) && printOutput ) {
				Log( e.Data );
				onOutput?.Invoke( e.Data );
			}
		};
		
		process.ErrorDataReceived += (sender, e) => {
			if( !string.IsNullOrEmpty( e.Data ) && printErrors ) {
				LogError( e.Data );
				onError?.Invoke( e.Data );
			}
		};
		
		process.Exited += (sender, e) => {
			if( printOutput )
				Log( "Process finished!\n"
				   +$"Exit time    : {process.ExitTime}\n"
				   +$"Exit code    : {process.ExitCode}\n"
				   +$"Elapsed time : {Math.Round((process.ExitTime - process.StartTime).TotalMilliseconds)}"
				   );
			onDone?.Invoke();
		};
		
		Log( "Launching:\n"+path+" "+args );
		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();
		return process;
	}
	
	public static string FindExecutablePath( string executableName ) {
		var allPaths = Environment.GetEnvironmentVariable( "PATH", EnvironmentVariableTarget.Machine )
					 + Path.PathSeparator
					 + Environment.GetEnvironmentVariable( "PATH", EnvironmentVariableTarget.Process )
					 + Path.PathSeparator
					 + Environment.GetEnvironmentVariable( "PATH", EnvironmentVariableTarget.User );
		var executablePaths = allPaths.Split( Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries );
		
		foreach( var path in executablePaths ) {
			var fullPath = Path.Combine( path, executableName );
			if( File.Exists( fullPath ) ) {
				return fullPath;
			}
		}
		return null;
	}
}
