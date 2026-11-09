// Shader created with Shader Forge v1.38 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:True,fnsp:True,fnfb:True,fsmp:False;n:type:ShaderForge.SFN_Final,id:4013,x:33670,y:32879,varname:node_4013,prsc:2|emission-429-RGB,alpha-8246-OUT,voffset-5363-OUT;n:type:ShaderForge.SFN_Tex2d,id:429,x:33252,y:32923,ptovrint:False,ptlb:Texture,ptin:_Texture,varname:node_429,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:097062fa4040bc6499b44b553f041419,ntxv:0,isnm:False|UVIN-9277-UVOUT;n:type:ShaderForge.SFN_Tex2d,id:8804,x:33005,y:33126,ptovrint:False,ptlb:Displace,ptin:_Displace,varname:node_8804,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:32266f1703545d24c95559c1225625ad,ntxv:0,isnm:False|UVIN-7692-UVOUT;n:type:ShaderForge.SFN_Multiply,id:5363,x:33446,y:33212,varname:node_5363,prsc:2|A-8804-RGB,B-3096-OUT,C-4135-OUT;n:type:ShaderForge.SFN_NormalVector,id:3096,x:33061,y:33311,prsc:2,pt:False;n:type:ShaderForge.SFN_ValueProperty,id:4135,x:33213,y:33416,ptovrint:False,ptlb:Power,ptin:_Power,varname:node_4135,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_TexCoord,id:9277,x:33033,y:32923,varname:node_9277,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_TexCoord,id:7692,x:32765,y:33133,varname:node_7692,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Slider,id:8246,x:33272,y:33119,ptovrint:False,ptlb:Opacity,ptin:_Opacity,varname:node_8246,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;proporder:429-8804-4135-8246;pass:END;sub:END;*/

Shader "Shader Forge/Water Opacity" {
    Properties {
        _Texture ("Texture", 2D) = "white" {}
        _Displace ("Displace", 2D) = "white" {}
        _Power ("Power", Float ) = 0
        _Opacity ("Opacity", Range(0, 1)) = 1
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal 
            #pragma target 2.0
            uniform sampler2D _Texture; uniform float4 _Texture_ST;
            uniform sampler2D _Displace; uniform float4 _Displace_ST;
            uniform float _Power;
            uniform float _Opacity;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 _Displace_var = tex2Dlod(_Displace,float4(TRANSFORM_TEX(o.uv0, _Displace),0.0,0));
                v.vertex.xyz += (_Displace_var.rgb*v.normal*_Power);
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 normalDirection = i.normalDir;
////// Lighting:
////// Emissive:
                float4 _Texture_var = tex2D(_Texture,TRANSFORM_TEX(i.uv0, _Texture));
                float3 emissive = _Texture_var.rgb;
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,_Opacity);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
