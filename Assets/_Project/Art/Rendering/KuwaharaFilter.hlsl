void Kuwahara_float(
    UnityTexture2D Texture,
    UnitySamplerState Sampler,
    float2 UV,
    float Radius,
    out float4 Out
)
{
    int r = (int)Radius;
    float count = (r + 1.0) * (r + 1.0);

    float4 mean[4] = {float4(0,0,0,0), float4(0,0,0,0), float4(0,0,0,0), float4(0,0,0,0)};
    float4 variance[4] = {float4(0,0,0,0), float4(0,0,0,0), float4(0,0,0,0), float4(0,0,0,0)};

    float2 texelSize = float2(
        ddx(UV).x,
        ddy(UV).y
    );

    for (int j = -r; j <= 0; j++)
    {
        for (int i = -r; i <= 0; i++)
        {
            float4 c = SAMPLE_TEXTURE2D(Texture.tex, Sampler.samplerstate, UV + float2(i, j) * texelSize);
            mean[0] += c;
            variance[0] += c * c;
        }
    }
    for (int j = -r; j <= 0; j++)
    {
        for (int i = 0; i <= r; i++)
        {
            float4 c = SAMPLE_TEXTURE2D(Texture.tex, Sampler.samplerstate, UV + float2(i, j) * texelSize);
            mean[1] += c;
            variance[1] += c * c;
        }
    }
    for (int j = 0; j <= r; j++)
    {
        for (int i = -r; i <= 0; i++)
        {
            float4 c = SAMPLE_TEXTURE2D(Texture.tex, Sampler.samplerstate, UV + float2(i, j) * texelSize);
            mean[2] += c;
            variance[2] += c * c;
        }
    }
    for (int j = 0; j <= r; j++)
    {
        for (int i = 0; i <= r; i++)
        {
            float4 c = SAMPLE_TEXTURE2D(Texture.tex, Sampler.samplerstate, UV + float2(i, j) * texelSize);
            mean[3] += c;
            variance[3] += c * c;
        }
    }

    float minVariance = 1e9;
    Out = mean[0] / count;

    for (int k = 0; k < 4; k++)
    {
        mean[k] /= count;
        variance[k] = abs(variance[k] / count - mean[k] * mean[k]);
        float v = variance[k].r + variance[k].g + variance[k].b;
        if (v < minVariance)
        {
            minVariance = v;
            Out = mean[k];
        }
    }
}