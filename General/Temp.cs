using System.Collections.Generic;
using System;
using Clutter;

public static class Temp {
	private const string c_namelessTag = "temp";
	
	private static TwoKeyDictionary<string, Type, object> s_tempContainers = new TwoKeyDictionary<string, Type, object>();
	// private static Dictionary<(string, Type), object> s_tempContainers = new Dictionary<(string, Type), object>();
	
#region Public
	public static List<T> List<T>( int desiredCapacity ) {
		return List<T>( c_namelessTag, desiredCapacity );
	}
	public static List<T> List<T>( string tag = c_namelessTag, int desiredCapacity = -1 ) {
		var containerType = typeof( List<T> );
		object container;
		if( !s_tempContainers.TryGetValue( tag, containerType, out container ) ) {
			container = new List<T>();
			s_tempContainers[tag, containerType] = container;
		}
		var result = (List<T>) container;
		
		if( desiredCapacity > 0 ) {
			result.ClearExpand( desiredCapacity );
		}
		else {
			result.Clear();
		}
		return result;
	}
	
	public static Stack<T> Stack<T>( string tag = c_namelessTag ) {
		var containerType = typeof( Stack<T> );
		object container;
		if( !s_tempContainers.TryGetValue( tag, containerType, out container ) ) {
			container = new Stack<T>();
			s_tempContainers[tag, containerType] = container;
		}
		var result = (Stack<T>) container;
		
		result.Clear();
		return result;
	}
	
	public static Queue<T> Queue<T>( string tag = c_namelessTag ) {
		var containerType = typeof( Queue<T> );
		object container;
		if( !s_tempContainers.TryGetValue( tag, containerType, out container ) ) {
			container = new Queue<T>();
			s_tempContainers[tag, containerType] = container;
		}
		var result = (Queue<T>) container;
		
		result.Clear();
		return result;
	}
	
	public static HashSet<T> HashSet<T>( string tag = c_namelessTag ) {
		var containerType = typeof( HashSet<T> );
		object container;
		if( !s_tempContainers.TryGetValue( tag, containerType, out container ) ) {
			container = new HashSet<T>();
			s_tempContainers[tag, containerType] = container;
		}
		var result = (HashSet<T>) container;
		
		result.Clear();
		return result;
	}
	
	public static Dictionary<TK, TV> Dictionary<TK, TV>( string tag = c_namelessTag ) {
		var containerType = typeof( Dictionary<TK, TV> );
		object container;
		if( !s_tempContainers.TryGetValue( tag, containerType, out container ) ) {
			container = new Dictionary<TK, TV>();
			s_tempContainers[tag, containerType] = container;
		}
		var result = (Dictionary<TK, TV>) container;
		
		result.Clear();
		return result;
	}
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}

namespace TempDemo {
public class Usage {
	public void Method() {
		// This is braindead-simple way to get a signle-allocation temporary list of certain type
		// Note that it WILL NOT be thread-safe, since other threads can also request this exact list
		var namelessList = Temp.List<int>( 100 );
			// do some kind of logic, using our list
		
		
		// for multithreaded code it's advisable to use tags to distinguish two or more containers of the same type:
		var namedDictionary = Temp.Dictionary<int, float>();
	}
}
}