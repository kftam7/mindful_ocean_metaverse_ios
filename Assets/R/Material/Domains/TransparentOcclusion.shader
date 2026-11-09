Shader "Custom/TransparentOcclusion"
{
    SubShader
    {
        // No actual 'pass' is necessary
        Pass
        {
            ZTest LEqual
            ZWrite On
            ColorMask 0 // This ensures we don't render any actual pixels
        }
    }
}
