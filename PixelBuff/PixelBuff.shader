Shader "Hidden/PixelBuff" {
	Properties {
		_MainTex( "Texture", 2D ) = "white" {}
	}
	
	SubShader {
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		
		Pass {
CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#include "UnityCG.cginc"
	
	sampler2D _MainTex;
	float _XBuff;
	float _YBuff;
	
	struct appdata {
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};
	
	struct v2f {
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
	};
	
	v2f vert( appdata v ) {
		v2f o;
		o.vertex = UnityObjectToClipPos( v.vertex );
		o.uv = v.uv;
		return o;
	}
	
	fixed4 frag( v2f i ) : SV_Target {
		float2 _new_uv = i.uv;
		fixed2 offset = float2(
			-0.4999,
			-0.4999
		);
		
		_new_uv.x = _new_uv.x - fmod( offset.x + _new_uv.x *_XBuff, 1 ) /_XBuff;
		_new_uv.y = _new_uv.y - fmod( offset.y + _new_uv.y *_YBuff, 1 ) /_YBuff;
		
		fixed4 col = tex2D( _MainTex, _new_uv );
		return col;
	}
ENDCG
		}
	}
}
