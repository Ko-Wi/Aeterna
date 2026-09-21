Shader "UI/SpriteInverseTransition" {
Properties {
[PerRendererData] _MainTex ("Mask Sprite", 2D) = "white" {}
_SpriteUV ("Sprite UV", Vector) = (0,0,1,1)
_StencilComp ("Stencil Comparison", Float) = 8
_Stencil ("Stencil ID", Float) = 0
_StencilOp ("Stencil Operation", Float) = 0
_StencilWriteMask ("Stencil Write Mask", Float) = 255
_StencilReadMask ("Stencil Read Mask", Float) = 255
_ColorMask ("Color Mask", Float) = 15
}
SubShader {
Tags { "Queue"="Transparent" "RenderType"="Transparent" }
Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
Cull Off ZWrite Off ZTest [unity_GUIZTestMode]
Blend SrcAlpha OneMinusSrcAlpha
ColorMask [_ColorMask]
Pass {
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
#include "UnityCG.cginc"
#include "UnityUI.cginc"
struct appdata { float4 vertex:POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; float4 data:TEXCOORD1; };
struct v2f { float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; float4 data:TEXCOORD1; float4 local:TEXCOORD2; };
sampler2D _MainTex;
float4 _SpriteUV, _ClipRect;
v2f vert(appdata v) { v2f o; o.local=v.vertex; o.vertex=UnityObjectToClipPos(v.vertex); o.color=v.color; o.uv=v.uv; o.data=v.data; return o; }
fixed4 frag(v2f i):SV_Target {
float opening=saturate(i.data.x);
float2 p=(i.uv-0.5)*float2(i.data.y,1)/max(opening*8,0.00001)+0.5;
float inside=step(0,p.x)*step(p.x,1)*step(0,p.y)*step(p.y,1);
float hole=tex2D(_MainTex,lerp(_SpriteUV.xy,_SpriteUV.zw,saturate(p))).a*inside;
fixed4 c=i.color;
c.a *= opening<=0 ? 1 : (opening>=1 ? 0 : 1-hole);
#ifdef UNITY_UI_CLIP_RECT
c.a *= UnityGet2DClipping(i.local.xy,_ClipRect);
#endif
return c;
}
ENDCG
}
}
}
