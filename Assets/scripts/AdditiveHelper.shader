Shader "Custom/AdditiveHelper"
{
Properties
{
_Color ("Main Color", Color) = (1,1,1,1)
}
SubShader
{

//Alphatest Greater 0
Tags {"Queue"="Transparent"}
Lighting off
Blend SrcAlpha OneMinusSrcAlpha
//AlphaTest GEqual [_Cutoff]
Color [_Color]
Cull back
ZWrite off


Pass{
	SetTexture [_MainTex] {
		combine texture * previous DOUBLE
	}
  }
 }
 
 FallBack "VertexLit"
}