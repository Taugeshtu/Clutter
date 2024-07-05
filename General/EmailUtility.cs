using UnityEngine;
using System;

public static class EmailUtility {
	public static void SendEmail( string to, string subject = "", string body = "" ) {
		var mailtoUri = $"mailto:{Uri.EscapeDataString(to)}";
		
		var hasSubject = !string.IsNullOrEmpty( subject );
		var hasBody = !string.IsNullOrEmpty( body );
		
		if( hasSubject || hasBody )
			mailtoUri += "?";
		if( hasSubject )
			mailtoUri += $"subject={Uri.EscapeDataString(subject)}";
		if( hasSubject && hasBody )
			mailtoUri += "&";
		if( hasBody )
			mailtoUri += $"body={Uri.EscapeDataString(body)}";
		
		Application.OpenURL( mailtoUri );
	}
}