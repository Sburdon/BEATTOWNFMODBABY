Shader "Custom/OutlineTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness", Range(0.001, 0.05)) = 0.005
    }
    
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _OutlineColor;
            float _OutlineThickness;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 texSize = float2(1.0, 1.0) / _ScreenParams.xy; // Adjust outline thickness
                float alpha = tex2D(_MainTex, i.uv).a;

                // Sample surrounding pixels
                float alphaLeft   = tex2D(_MainTex, i.uv + float2(-_OutlineThickness, 0)).a;
                float alphaRight  = tex2D(_MainTex, i.uv + float2(_OutlineThickness, 0)).a;
                float alphaUp     = tex2D(_MainTex, i.uv + float2(0, _OutlineThickness)).a;
                float alphaDown   = tex2D(_MainTex, i.uv + float2(0, -_OutlineThickness)).a;

                bool isOutline = (alpha == 0) && (alphaLeft > 0 || alphaRight > 0 || alphaUp > 0 || alphaDown > 0);

                if (isOutline)
                {
                    return _OutlineColor;
                }

                return tex2D(_MainTex, i.uv); // Render the normal texture
            }
            ENDCG
        }
    }
}
