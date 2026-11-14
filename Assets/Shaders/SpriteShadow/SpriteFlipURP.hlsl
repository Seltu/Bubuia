#ifndef SELTU_SPRITE_FLIP_URP_INCLUDED
#define SELTU_SPRITE_FLIP_URP_INCLUDED

#if !defined(SHADERGRAPH_PREVIEW)
#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
#endif

void ApplySpriteFlip_float(float3 PositionOS, out float3 FlippedPositionOS)
{
#if defined(SHADERGRAPH_PREVIEW)
    FlippedPositionOS = PositionOS;
#elif defined(SHADERPASS) && defined(SHADERPASS_SHADOWCASTER) && (SHADERPASS == SHADERPASS_SHADOWCASTER)
    FlippedPositionOS = PositionOS;
#else
    SetUpSpriteInstanceProperties();
    FlippedPositionOS = UnityFlipSprite(PositionOS, unity_SpriteProps.xy);
#endif
}

#endif