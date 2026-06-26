void MainLightShadow_float(float3 WorldPos, out float ShadowAtten)
{
    #ifdef SHADERGRAPH_PREVIEW
        ShadowAtten = 1;
    #else
        #if SHADOWS_SCREEN
            float4 clipPos = TransformWorldToHClip(WorldPos);
            float4 shadowCoord = ComputeScreenPos(clipPos);
        #else
            float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
        #endif
        Light mainLight = GetMainLight(shadowCoord);
        ShadowAtten = mainLight.shadowAttenuation;
    #endif
}