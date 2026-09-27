#ifndef MULTI_PASS_FUR_WOOL_HLSL
#define MULTI_PASS_FUR_WOOL_HLSL

// =============================================================================
// [WOOL PROTOTYPE]  Stylized cartoon wool shell displacement.
//
// One shared function, called from Lit-MP / Depth-MP / DepthNormals-MP, so the
// forward, depth and depth-normals passes displace the shells identically.
// (The shadow pass in this project does not displace shells at all.)
//
// To revert: delete this file, remove its 3 includes, and drop the _Wool*
// properties from Lit-MP.shader / Param-MP.hlsl (and the material).
// =============================================================================

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "./Param-MP.hlsl"

#ifndef WOOL_PI
#define WOOL_PI 3.14159265359
#endif
#ifndef WOOL_TWO_PI
#define WOOL_TWO_PI 6.28318530718
#endif

inline float3 ComputeWoolShellPositionWS(
    float3 positionWS,
    float3 positionOS,
    half3  normalWS,
    half3  tangentWS,
    half3  bitangentWS,
    half3  groomWS,
    half   furLength,
    float  layer,
    float2 uv)
{
    const float shellStep = _TotalShellStep / _TOTAL_LAYER;

    // --- Wind / weight (same as the original straight fur) ---
    half moveFactor = pow(abs(layer), _BaseMove.w);
    half3 windAngle = _Time.w * _WindFreq.xyz;
    half3 windMove = moveFactor * _WindMove.xyz * sin(windAngle + positionOS.xyz * _WindMove.w);
    half3 move = moveFactor * _BaseMove.xyz;

    // --- Fur direction (same as the original) ---
    float bent = _BentType * layer + (1.0 - _BentType);
    half3 blendedGroom = lerp(normalWS, groomWS, _GroomingIntensity * bent);
    half3 shellDir = SafeNormalize(blendedGroom + move + windMove);

    // --- Straight pile height ---
    float height = shellStep * _CURRENT_LAYER * furLength * _FurLengthIntensity;

    // --- [WOOL CURL] ---------------------------------------------------------
    // Per-clump phase: a low-frequency slice of the fur noise picks each clump's
    // curl direction, so neighbouring clumps curl differently.
    half clump = SAMPLE_TEXTURE2D_LOD(_FurMap, sampler_FurMap,
                                      uv / _BaseMap_ST.xy * _WoolClumpScale, 0).r;
    float curlAngle = clump * WOOL_TWO_PI + _WoolCurlTurns * WOOL_TWO_PI * layer;

    // Radius is 0 at the root and at the tip, peaks in the middle, so each shell
    // traces a rounded lock instead of a straight spike.
    float curlRadius = _WoolCurlAmount * height * sin(layer * WOOL_PI);

    float3 lateral = tangentWS * cos(curlAngle) + bitangentWS * sin(curlAngle);
    // ------------------------------------------------------------------------

    return positionWS
         + shellDir * (height * _WoolPuff)
         + lateral * curlRadius;
}

#endif
