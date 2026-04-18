Shader "Custom/PlayerOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineSize ("Outline Size", Float) = 10
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _OutlineColor;
            float _OutlineSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float alpha = tex2D(_MainTex, i.uv).a;

                float outline = 0;

                outline += tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, 0) * _OutlineSize).a;
                outline += tex2D(_MainTex, i.uv - float2(_MainTex_TexelSize.x, 0) * _OutlineSize).a;
                outline += tex2D(_MainTex, i.uv + float2(0, _MainTex_TexelSize.y) * _OutlineSize).a;
                outline += tex2D(_MainTex, i.uv - float2(0, _MainTex_TexelSize.y) * _OutlineSize).a;

                if (alpha == 0 && outline > 0)
                    return _OutlineColor;

                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
