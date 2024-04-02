using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Clutter;
using UnityEngine;

public static class BlockParserTests {
	private static List<(string start, string end, string source, string result)> _testsSingle = new List<(string, string, string, string)>() {
		( "<top>\n", "\n</top>", "No tag present", null ),
		( "<top>\n", "\n</top>", "<top>\ncontent without closing tag", null ),
		( "<top>\n", "\n</top>", "content without opening tag\n</top>", null ),
		( "<top>\n", "\n</top>",
@"ignore this
Also pass the inline <top> tag
<top>
Content start
<top>
Nested tag included
</top>
</top>
",
@"Content start
<top>
Nested tag included
</top>" ),
	};
	
	private static List<(string start, string end, string source, List<string> results)> _testsMultiple = new List<(string, string, string, List<string>)>() {
		( "<tag>\n", "\n</tag>",
@"
ignore this
Also pass the inline <tag> tag
<tag>
Content start
<tag>
Nested tag included
</tag>
</tag>
",
new List<string>() {

@"Content start
<tag>
Nested tag included
</tag>",
		} ),
		
		
		( "<tag>\n", "\n</tag>",
@"
ignore this
Also pass the inline <tag> tag
<tag>
First block
<tag>
Nested tag included
</tag>
</tag>

<tag>
Second block
</tag>
",
new List<string>() {

@"First block
<tag>
Nested tag included
</tag>",

@"Second block",
		} ),
		
		
		( "<tag>\n", "\n</tag>",
@"
adversarial as fuck
<tag>
First block
<tag>
unclosed opening tag
</tag>

<tag>
Second block with extra tag, just to fuck with baby parsers
</tag>
</tag>
",
new List<string>() {

@"First block
<tag>
unclosed opening tag",

@"Second block with extra tag, just to fuck with baby parsers
</tag>",
		} ),
		
		// This case is gonna fail, but it's going to be so rare in production that I'm putting it at the very back of the backburner...
		( "<tag>\n", "\n</tag>",
@"
cranking up the pressure...
<tag>

<tag>
Nested 1
</tag>

<tag>
Nested 2.0
<tag>
Nested-nested
</tag>
</tag>

</tag>

<tag>
Second block with extra tag, just to fuck with baby parsers
</tag>
</tag>
",
new List<string>() {

@"
<tag>
Nested 1
</tag>

<tag>
Nested 2.0
<tag>
Nested-nested
</tag>
</tag>
",

@"Second block with extra tag, just to fuck with baby parsers
</tag>",
		} ),
	};
	
	public static void Run() {
		foreach( var test in _testsSingle ) {
			var source = test.source?.Replace( "\r\n", "\n" );
			var expectedResult = test.result?.Replace( "\r\n", "\n" );
			Debug.Log( "Testing single:\n"+source );
			
			var result = BlockParser.Extract( source, test.start, test.end ).Extracted?.Replace( "\r\n", "\n" );
			if( result != expectedResult )
				Debug.LogError( "Failure. Expected:\n"+expectedResult+"\nDelivered:\n"+result );
		}
		
		foreach( var test in _testsMultiple ) {
			var source = test.source.Replace( "\r\n", "\n" );
			Debug.Log( "Testing many:\n"+source );
			
			var results = BlockParser.ExtractAll( source, test.start, test.end ).ToList();
			for( var a = 0; a < test.results.Count; a++ ) {
				var expectedResult = test.results[a].Replace( "\r\n", "\n" );
				var foundMatch = false;
				for( var b = 0; b < results.Count; b++ ) {
					if( expectedResult == results[b].Extracted.Replace( "\r\n", "\n" ) ) {
						results.RemoveAt( b );
						foundMatch = true;
						break;
					}
				}
				if( !foundMatch ) {
					Debug.LogError( "Expected result not found:\n"+expectedResult );
				}
			}
			foreach( var leftoverResult in results ) {
				Debug.LogError( "Unmatched result produced:\n"+leftoverResult.Extracted.Replace( "\r\n", "\n" ) );
			}
		}
	}
}
