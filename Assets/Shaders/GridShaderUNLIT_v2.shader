Shader "PDT Shaders/Grid WorldSpace v1"
{
    Properties
    {
        _LineColor("Line Color", Color) = (1,1,1,1)
        _CellColor("Cell Color", Color) = (0,0,0,0)
        _SelectedColor("Selected Color", Color) = (1,0,0,1)

        [IntRange] _GridSize("Grid Size (world units per cell)", Range(1,200)) = 1
        _LineSize("Line Size", Range(0,1)) = 0.05

        [IntRange] _SelectCell("Select Cell Toggle (0 = False, 1 = True)", Range(0,1)) = 0
        [IntRange] _SelectedCellX("Selected Cell X", Range(0,100)) = 0
        [IntRange] _SelectedCellY("Selected Cell Y", Range(0,100)) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float4 _LineColor;
            float4 _CellColor;
            float4 _SelectedColor;

            float _GridSize;
            float _LineSize;

            float _SelectCell;
            float _SelectedCellX;
            float _SelectedCellY;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; // world position
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 worldUV = i.worldPos.xz / _GridSize; // project onto XZ plane

                float2 grid = frac(worldUV);
                float2 id = floor(worldUV);

                float4 color = _CellColor;
                float alpha = _CellColor.a;

                // Highlight selected cell
                if (round(_SelectCell) == 1.0 && id.x == _SelectedCellX && id.y == _SelectedCellY)
                {
                    color = _SelectedColor;
                    alpha = _SelectedColor.a;
                }

                // Grid line check
                if (grid.x < _LineSize || grid.y < _LineSize)
                {
                    color = _LineColor;
                    alpha = _LineColor.a;
                }

                if (alpha == 0.0) discard;

                return fixed4(color.rgb, alpha);
            }
            ENDCG
        }
    }
}
