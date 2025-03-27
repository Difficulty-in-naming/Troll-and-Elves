Shader "UI/InventoryGridHighlight" 
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _HighlightColor ("Highlight Color", Color) = (1, 1, 1, 1)
        _BorderSize ("Border Size", Float) = 0.01
        _BorderColor ("Border Color", Color) = (0, 0, 0, 1)
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        
        Pass
        {
            Name "Default"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            
            float4 _HighlightColor;
            float4 _BorderColor;
            float _BorderSize;
            
            // 支持最多64个网格位置
            int2 _CellPositions[64];
            int _CellCount;
            
            // 网格尺寸（假设是 5x5 的网格）
            int _GridWidth = 5;
            int _GridHeight = 5;
            
            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                
                OUT.color = v.color * _Color;
                return OUT;
            }
            
            bool IsCellHighlighted(float2 uv)
            {
                // 计算当前 UV 对应的网格坐标 (0-based)
                float gridX = uv.x / 64;
                float gridY = uv.y / 64;
                
                // 检查是否在高亮列表中
                for (int i = 0; i < _CellCount && i < 64; i++)
                {
                    if (gridX == _CellPositions[i].x && gridY == _CellPositions[i].y)
                    {
                        return true;
                    }
                }
                
                return false;
            }
            
            fixed4 frag(v2f IN) : SV_Target
            {
                // 默认颜色为透明
                fixed4 outputColor = fixed4(0, 0, 0, 0);
                
                // 检查当前位置是否需要高亮
                if (IsCellHighlighted(IN.texcoord))
                {
                    // 计算单元格宽高
                    float cellWidth = 1.0 / _GridWidth;
                    float cellHeight = 1.0 / _GridHeight;
                    
                    // 计算当前像素在单元格内的相对位置 (0-1)
                    float cellX = fmod(IN.texcoord.x, cellWidth) / cellWidth;
                    float cellY = fmod(IN.texcoord.y, cellHeight) / cellHeight;
                    
                    // 边框大小（相对于单元格尺寸）
                    float borderSizeX = _BorderSize;
                    float borderSizeY = _BorderSize;
                    
                    // 判断是否在边框上
                    if (cellX < borderSizeX || cellX > (1.0 - borderSizeX) || 
                        cellY < borderSizeY || cellY > (1.0 - borderSizeY))
                    {
                        outputColor = _BorderColor; // 边框颜色
                    }
                    else
                    {
                        outputColor = _HighlightColor; // 填充颜色
                    }
                }
                
                // 应用UI裁剪矩形
                #ifdef UNITY_UI_CLIP_RECT
                outputColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
                
                // 应用Alpha裁剪
                #ifdef UNITY_UI_ALPHACLIP
                clip (outputColor.a - 0.001);
                #endif
                
                return outputColor;
            }
            ENDCG
        }
    }
}

