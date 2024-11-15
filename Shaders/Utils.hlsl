#ifndef ComputeUtilsIncluded
#define ComputeUtilsIncluded

#define OS2WS( osPosition ) mul( unity_ObjectToWorld, osPosition )

// Missing "constructors"
float2 _float2( float x ) {
	return float2( x, x );
}

float3 _float3( float x ) {
	return float3( x, x, x );
}

float4 _float4( float x ) {
	return float4( x, x, x, x );
}

int2 _int2( int x ) {
	return int2( x, x );
}

int3 _int3( int x ) {
	return int3( x, x, x );
}

int4 _int4( int x ) {
	return int4( x, x, x, x );
}

uint2 _uint2( uint x ) {
	return uint2( x, x );
}

uint3 _uint3( uint x ) {
	return uint3( x, x, x );
}

uint4 _uint4( uint x ) {
	return uint4( x, x, x, x );
}

// Grid index roll/unroll
uint To1D( uint2 id, uint2 mapSize ) {
	return id.y *mapSize.x + id.x;
}

uint2 To2D( uint id, uint2 mapSize ) {
	uint x = id %mapSize.x;
	uint y = (id - x) /mapSize.x;
	return uint2( x, y );
}

float unlerp( float x, float min, float max ) {
	float result = (x - min) /(max - min);
	return result;
}

// Math
// Convention: positive angle == counter-clockwise rotation
float2 Rotate( float2 v, float angleRadians ) {
	/*
	new = (1, 0) * (1)
	      (0, 1)   (0)
	
	new = (newXAxis.x, newYAxis.x) * (1)
	      (newXAxis.y, newYAxis.y)   (0)
	
	newXAxis = (cos(angle), sin(angle))
	newYAxis = (cos(angle + 90), sin(angle + 90))
	
	cos(x + 90) = -sin(x)
	sin(x + 90) = cos(x)
	
	newXAxis = (cos(angle), sin(angle))
	newYAxis = (-sin(angle), cos(angle))
	
	new = newXAxis *old.x + newYAxis *old.y
	*/
	float s = sin( angleRadians );
	float c = cos( angleRadians );
	float2 basisX = float2(  c, s );
	float2 basisY = float2( -s, c );
	return basisX *v.x + basisY *v.y;
}

// Bad randoms:
float rand_1_05( in float2 uv ) {
	float2 noise = frac( sin( dot( uv ,float2( 12.9898, 78.233 ) *2.0 ) ) *43758.5453 );
	return abs(noise.x + noise.y) * 0.5;
}

float2 rand_2_10( in float2 uv ) {
	float noiseX = frac( sin( dot( uv, float2( 12.9898, 78.233 ) *2.0 ) ) *43758.5453 );
	float noiseY = sqrt( 1 - noiseX *noiseX );
	return float2( noiseX, noiseY );
}

float2 rand_2_0004( in float2 uv ) {
	float noiseX = frac( sin( dot( uv, float2( 12.9898, 78.233 )      ) ) *43758.5453 );
	float noiseY = frac( sin( dot( uv, float2( 12.9898, 78.233 ) *2.0 ) ) *43758.5453 );
	return float2( noiseX, noiseY ) *0.004;
}

int GetBit( uint x, int bitIndex ) {
	return (x & (1 << bitIndex)) != 0;
}

uint SetBit( uint x, int bitIndex, bool bitValue ) {
	if( bitValue ) {
		x |= 1 << bitIndex;
	}
	else {
		x &= ~(1 << bitIndex);
	}
	return x;
}

float remap( float x, float inputMin, float inputMax, float outMin, float outMax ) {
	float factor = unlerp( x, inputMin, inputMax );
	return lerp( outMin, outMax, factor );
}

#endif
