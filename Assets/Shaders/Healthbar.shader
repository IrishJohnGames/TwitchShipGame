Shader "Sprites/HealthBar"
  {
      Properties
      {
         [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
          _Color ("Main Color", Color) = (0.2,1,0.2,1)
          _Color2 ("Damaged color", Color) = (1,1,0,1)
          _HealthPercent("HealthPercent", Float) = 0
          [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
      
      
      }
  
      SubShader
      {
          Tags
          { 
              "Queue"="Transparent" 
              "IgnoreProjector"="True" 
              "RenderType"="Transparent" 
              "PreviewType"="Plane"
              "CanUseSpriteAtlas"="True"
          }
  
          Cull Off
          Lighting Off
          ZWrite Off
          Blend One OneMinusSrcAlpha
  
          Pass
          {
          CGPROGRAM
              #pragma vertex vert
              #pragma fragment frag
              #pragma multi_compile _ PIXELSNAP_ON
              #include "UnityCG.cginc"
              
              struct appdata_t
              {
                  float4 vertex   : POSITION;
                  float4 color    : COLOR;
                  float2 texcoord : TEXCOORD0;
              };
  
              struct v2f
              {
                  float4 vertex   : SV_POSITION;
                  fixed4 color    : COLOR;
                  half2 texcoord  : TEXCOORD0;
              };
              //main color
              fixed4 _Color;
              //damaged color
              fixed4 _Color2;
              //health percent              
              half _HealthPercent;
              //texture
              sampler2D _MainTex;
           
              v2f vert(appdata_t IN)
              {
                  //setup
                  v2f OUT;
                  OUT.vertex = UnityObjectToClipPos(IN.vertex);
                  OUT.texcoord = IN.texcoord;
                  #ifdef PIXELSNAP_ON
                  OUT.vertex = UnityPixelSnap (OUT.vertex);
                  #endif
  
                  return OUT;
              }
  
    
  
              fixed4 frag(v2f IN) : SV_Target
              {
                  //read texture color
                  fixed4 c = tex2D(_MainTex, IN.texcoord);
                  //check if pixel is above health threshold                  
                  if ( IN.texcoord.x >  _HealthPercent )
                  {
                    //pixel is above health threshold, apply damaged color
                    c *= _Color2;
                  }
                  else
                  {
                     //pixel is below health threshold, apply healthy color
                     c *= _Color;
                  }
                  
                  return c;
              }
          ENDCG
          }
      }
  }