Shader "Hol/PointRenderer" {
	Properties{
		//_Color("Diffuse Material Color", Color) = (1,1,1,1)
		//_SpecColor("Specular Material Color", Color) = (1,1,1,1)
		_Shininess("Shininess", Float) = 20
		_AmbientFactor("Ambient Factor",Float) = 0.2
		_DiffuseFactor("Diffuse Factor",Float) = 1
		_SpecularFactor("Specular Factor",Float) = 1
		_PointSize("Point Render Size",Float) = 1
	}
		SubShader{
		   Pass {
			  Tags { "LightMode" = "ForwardBase" } // pass for ambient light 
				 // and first directional light source without cookie

			  CGPROGRAM

			  #pragma vertex vert  
			  #pragma fragment frag 

			  #include "UnityCG.cginc"
			  uniform float4 _LightColor0;
	// color of light source (from "Lighting.cginc")

 // User-specified properties
 //uniform float4 _Color;
 //uniform float4 _SpecColor;
 uniform float _Shininess;
 uniform float _DiffuseFactor;
 uniform float _AmbientFactor;
 uniform float _SpecularFactor;
 uniform float _PointSize;

 struct vertexInput {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 color : COLOR;
 };
 struct vertexOutput {
	float4 pos : SV_POSITION;
	float4 posWorld : TEXCOORD0;
	float3 normalDir : TEXCOORD1;
	float4 color : COLOR;
	float size : PSIZE;
 };

 vertexOutput vert(vertexInput input)
 {
	vertexOutput output;

	float4x4 modelMatrix = unity_ObjectToWorld;
	float4x4 modelMatrixInverse = unity_WorldToObject;

	output.posWorld = mul(modelMatrix, input.vertex);
	if (length(input.normal) < 0.0001)
		output.normalDir = float3(0,0,0);
	else
	output.normalDir = normalize(
	   mul(float4(input.normal, 0.0), modelMatrixInverse).xyz);
	output.pos = UnityObjectToClipPos(input.vertex);
	output.color = input.color;
	output.size = _PointSize;
	return output;
 }

 float4 frag(vertexOutput input) : COLOR
 {

	 //if(length(input.normalDir)!=1)
	 //	return float4(input.color.rgb, 1.0);
	 float3 normalDirection = normalize(input.normalDir);

	 float3 viewDirection = normalize(
		_WorldSpaceCameraPos - input.posWorld.xyz);
	 float3 lightDirection =
		normalize(_WorldSpaceLightPos0.xyz);

	 float3 ambientLighting =
		 //_AmbientFactor * UNITY_LIGHTMODEL_AMBIENT.rgb * input.color.rgb;
		 //_AmbientFactor * input.color.rgb;
		 _AmbientFactor * input.color.rgb * _LightColor0.rgb;
	 if (input.normalDir.x == 0 && input.normalDir.y == 0 && input.normalDir.z == 0) {
		 return float4(ambientLighting, 1.0);
	 }
	 float3 diffuseReflection =
		 _DiffuseFactor *
		_LightColor0.rgb * input.color.rgb
		* max(0.0, dot(normalDirection, lightDirection));

	 float3 specularReflection;
	 if (dot(normalDirection, lightDirection) < 0.0)
		 // light source on the wrong side?
	  {
		 specularReflection = float3(0.0, 0.0, 0.0);
		 // no specular reflection
   }
   else // light source on the right side
   {
	  specularReflection =
		  _SpecularFactor *
		  _LightColor0.rgb * 
		  pow(max(0.0, dot(
		 reflect(-lightDirection, normalDirection),
		 viewDirection)), _Shininess);
   }

   return float4(ambientLighting + diffuseReflection
	  + specularReflection, 1.0);
 }

 ENDCG
 }

 Pass {
	Tags { "LightMode" = "ForwardAdd" }
	// pass for additional light sources
 Blend One One // additive blending 

 CGPROGRAM

 #pragma multi_compile_lightpass

 #pragma vertex vert  
 #pragma fragment frag 

 #include "UnityCG.cginc"
 uniform float4 _LightColor0;
 // color of light source (from "Lighting.cginc")
 uniform float4x4 unity_WorldToLight; // transformation 
	// from world to light space (from Autolight.cginc)
 #if defined (DIRECTIONAL_COOKIE) || defined (SPOT) || defined(POINT)
	uniform sampler2D _LightTexture0;
	// cookie alpha texture map (from Autolight.cginc)
 #elif defined (POINT_COOKIE)
	uniform samplerCUBE _LightTexture0;
	// cookie alpha texture map (from Autolight.cginc)
 #endif
 //uniform sampler2D _LightTextureB0;
 // User-specified properties
 //uniform float4 _Color;
 //uniform float4 _SpecColor;
 uniform float _Shininess;
 uniform float _DiffuseFactor;
 uniform float _AmbientFactor;
 uniform float _SpecularFactor;
 struct vertexInput {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 color : COLOR;
 };
 struct vertexOutput {
	float4 pos : SV_POSITION;
	float4 posWorld : TEXCOORD0;
	// position of the vertex (and fragment) in world space 
 float4 posLight : TEXCOORD1;
 // position of the vertex (and fragment) in light space
 float3 normalDir : TEXCOORD2;
 // surface normal vector in world space
 float4 color : COLOR;
 };

 vertexOutput vert(vertexInput input)
 {
	vertexOutput output;

	float4x4 modelMatrix = unity_ObjectToWorld;
	float4x4 modelMatrixInverse = unity_WorldToObject;

	output.posWorld = mul(modelMatrix, input.vertex);
	output.posLight = mul(unity_WorldToLight, output.posWorld);
	output.normalDir = normalize(
	   mul(float4(input.normal, 0.0), modelMatrixInverse).xyz);
	output.pos = UnityObjectToClipPos(input.vertex);
	output.color = input.color;
	return output;
 }

 float4 frag(vertexOutput input) : COLOR
 {
	float3 normalDirection = normalize(input.normalDir);

	float3 viewDirection = normalize(
	   _WorldSpaceCameraPos - input.posWorld.xyz);
	float3 lightDirection;
	float attenuation = 1.0;
	// by default no attenuation with distance

 #if defined (DIRECTIONAL) || defined (DIRECTIONAL_COOKIE)
	lightDirection = normalize(_WorldSpaceLightPos0.xyz);
 #elif defined (POINT_NOATT)
	lightDirection = normalize(_WorldSpaceLightPos0.xyz - input.posWorld.xyz);
 #elif defined(POINT)||defined(POINT_COOKIE)||defined(SPOT)

	float3 vertexToLightSource = _WorldSpaceLightPos0 - input.posWorld.xyz;
	float distance = length(vertexToLightSource);
	float range = 1 / unity_WorldToLight[0][0];
	if (range < distance) {
		return float4(0.0, 0.0, 0.0, 0.0);
	}
	attenuation = 1 / (distance); // linear attenuation 

	//distance = input.posLight.z;
	//attenuation = tex2D(_LightTexture0, float2(distance, distance)).a;


	lightDirection = normalize(vertexToLightSource);

 #endif

 float3 diffuseReflection = //0.0f*
	_DiffuseFactor *
	attenuation * _LightColor0.rgb * input.color.rgb
	* max(0.0, dot(normalDirection, lightDirection));

 float3 specularReflection;
 if (dot(normalDirection, lightDirection) < 0.0)
	 // light source on the wrong side?
  {
	 specularReflection = float3(0.0, 0.0, 0.0);
	 // no specular reflection
 }
 else // light source on the right side
 {
	specularReflection =
		_SpecularFactor *
		attenuation * _LightColor0.rgb
	   * input.color.rgb * pow(max(0.0, dot(
	   reflect(-lightDirection, normalDirection),
	   viewDirection)), _Shininess);
 }

 float cookieAttenuation = 1.0;
 // by default no cookie attenuation
 #if defined (DIRECTIONAL_COOKIE)
	cookieAttenuation = tex2D(_LightTexture0,
	   input.posLight.xy).a;
 #elif defined (POINT_COOKIE)
	cookieAttenuation = texCUBE(_LightTexture0,
	   input.posLight.xyz).a;
 #elif defined (SPOT)
	cookieAttenuation = tex2D(_LightTexture0,
	   input.posLight.xy / input.posLight.w
	   + float2(0.5, 0.5)).a;
 #endif
	cookieAttenuation = cookieAttenuation * 0.4f;
	//return float4(0.0, 0.0, 0.0, 0.0);
 return float4(cookieAttenuation
	* (diffuseReflection + specularReflection), 1.0);
 }

 ENDCG
 }
	}
		Fallback "Specular"
}