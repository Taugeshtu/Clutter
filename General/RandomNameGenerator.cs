using UnityEngine;
using System.Collections.Generic;

public static partial class RNG {
	public enum Format {
		PascalCase,
		camelCase,
		lowercase,
		PascalSpaced,
		camelSpaced,
		lowerspaced
	}
	
	private static string[] s_adjectives = {
		"fast",
		"slow",
		"daft",
		"racing",
		"keen",
		
		"cruel",
		"jolly",
		"tall",
		"mellow",
		"hot"
	};
	
	private static string[] s_colors = {
		"black",
		"white",
		"gray",
		"blue",
		"red",
		
		"yellow",
		"cyan",
		"teal",
		"purple",
		"violet"
	};
	
	private static string[] s_nouns = {
		"whale",
		"shark",
		"box",
		"car",
		"king",
		
		"dust",
		"trip",
		"farm",
		"gunk",
		"rune"
	};
	
	private static System.Text.StringBuilder s_builder = new System.Text.StringBuilder();
	
#region Implementation
#endregion
	
	
#region Public
	public static string Generate( System.Object obj, Format format = Format.PascalCase ) {
		return Generate( obj.GetHashCode(), format );
	}
	
	public static string Generate( int hash, Format format = Format.PascalCase ) {
		hash = Mathf.Abs( hash );
		var adjectiveCode = (hash /100) %10;
		var colorCode = (hash /10) %10;
		var nounCode = (hash) %10;
		
		var isSpaced = _IsSpaced( format );
		var isPascal = _IsPascal( format );
		var isCamel = _IsCamel( format );
		
		s_builder.Length = 0;
		s_builder.Append( s_adjectives[adjectiveCode] );
		if( isSpaced ) { s_builder.Append( ' ' ); }
		
		var colorStart = s_builder.Length;
		s_builder.Append( s_colors[colorCode] );
		if( isSpaced ) { s_builder.Append( ' ' ); }
		
		var nounStart = s_builder.Length;
		s_builder.Append( s_nouns[nounCode] );
		
		if( isPascal ) {
			s_builder[0] = char.ToUpperInvariant( s_builder[0] );
		}
		if( isPascal || isCamel ) {
			s_builder[colorStart] = char.ToUpperInvariant( s_builder[colorStart] );
			s_builder[nounStart] = char.ToUpperInvariant( s_builder[nounStart] );
		}
		
		return s_builder.ToString();
	}
#endregion
	
	
#region Private
	private static bool _IsSpaced( Format format ) {
		return (format == Format.PascalSpaced) || (format == Format.camelSpaced) || (format == Format.lowerspaced);
	}
	
	private static bool _IsPascal( Format format ) {
		return (format == Format.PascalCase) || (format == Format.PascalSpaced);
	}
	
	private static bool _IsCamel( Format format ) {
		return (format == Format.camelCase) || (format == Format.camelSpaced);
	}
#endregion
	
	
#region Temporary
#endregion
}
