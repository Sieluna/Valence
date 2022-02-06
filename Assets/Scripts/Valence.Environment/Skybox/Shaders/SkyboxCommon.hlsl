#ifndef VALENCE_SKY_COMMON_INCLUDED
#define VALENCE_SKY_COMMON_INCLUDED

float2 RayIntersectSphere(float3 RayOrigin, float3 RayDirection, float4 Sphere)
{

}

// Henyey-Greenstein phase function factor [-1, 1]
// represents the average cosine of the scattered directions
// 0 is isotropic scattering
// > 1 is forward scattering, < 1 is backwards
#define G 0.76

float GetRayleighPhase(float cosTheta)
{
    // float pi316 = 3.0 / (16.0 * pi);
    const float pi316 = 0.0596831;
    return pi316 * (1.0 + pow(cosTheta, 2.0));
}

float GetHenyeyGreensteinPhase(float cosTheta, float g)
{
    // float pi14 = 1.0 / (4.0 * pi);
    const float pi14 = 0.07957747;
    return pi14 * ((1.0 - g * g) / pow(1.0 + g * g - 2.0 * g * cosTheta, 1.5));
}

float GetSchlickPhase(float cosTheta, float g)
{
    // float pi38 = 3.0 / (8.0 * pi);
    const float pi38 = 0.1193662;
    return pi38 * ((1.0 - g * g) * (1.0 + cosTheta * cosTheta)) /
                  ((2.0 + g * g) * pow(1.0 + g * g - 2.0 * g * cosTheta, 1.5));
}

#endif // VALENCE_SKY_COMMON_INCLUDED