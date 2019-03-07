using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

public static class LineCounter {
	private struct SourceInfo {
		public int TotalLines;
		public int CodeLines;
		public int CommentLines;
		public int EmptyLines;
		
		public override string ToString() {
			return "Lines total: "+TotalLines+", code: "+CodeLines+", //: "+CommentLines+", empty: "+EmptyLines;
		}
	}
	
	private static int s_bloatedTreshold = 500;
	
	[MenuItem( "Assets/Dump code metrics" )]
	public static void MeasureCodebase() {
		var log = "Project root: "+Application.dataPath;
		
		var sourceFiles = _GetScriptFiles( Application.dataPath );
		log += "    Source files: "+sourceFiles.Count;
		
		var overallInfo = new SourceInfo();
		
		var bloatedFilesLog = new System.Text.StringBuilder();
		var bloatedFilesCount = 0;
		foreach( var file in sourceFiles ) {
			var fileMetrics = _MeasureFile( file );
			
			overallInfo.TotalLines += fileMetrics.TotalLines;
			overallInfo.CodeLines += fileMetrics.CodeLines;
			overallInfo.CommentLines += fileMetrics.CommentLines;
			overallInfo.EmptyLines += fileMetrics.EmptyLines;
			
			if( fileMetrics.CodeLines > s_bloatedTreshold ) {
				bloatedFilesCount += 1;
				
				bloatedFilesLog.Append( file );
				bloatedFilesLog.Append( "    " );
				bloatedFilesLog.AppendLine( fileMetrics.ToString() );
			}
		}
		
		log += "; "+overallInfo;
		log += "\n"+bloatedFilesCount+" bloated source files (more than "+s_bloatedTreshold+" code lines):\n";
		log += bloatedFilesLog.ToString();
		
		Debug.LogError( log );
	}
	
	private static List<string> _GetScriptFiles( string rootPath ) {
		var result = new List<string>();
		
		var info = new DirectoryInfo( rootPath );
		var csFiles = info.GetFiles( "*.cs", SearchOption.AllDirectories );
		foreach( var file in csFiles ) {
			result.Add( file.FullName );
		}
		
		return result;
	}
	
	private static CompareInfo s_compareInfo = CultureInfo.InvariantCulture.CompareInfo;
	
	private static SourceInfo _MeasureFile( string filePath ) {
		var lines = File.ReadAllLines( filePath );
		var result = new SourceInfo();
		
		foreach( var line in lines ) {
			result.TotalLines += 1;
			
			var trimmed = Regex.Replace( line, @"\s+", "" );
			if( string.IsNullOrEmpty( trimmed ) ) {
				result.EmptyLines += 1;
			}
			else {
				if( s_compareInfo.IsPrefix( trimmed, "//" ) ) {
					result.CommentLines += 1;
				}
				else {
					result.CodeLines += 1;
				}
			}
		}
		
		return result;
	}
}
