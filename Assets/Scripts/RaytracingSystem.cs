using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using static MathUtils;

public class RaytracingSystem : MonoBehaviour {
    public struct Trace {
        public float3 From;
        public float3 To;
        public float Energy;
    }

    public LightSource[] LightSources;
    public int LightSourceCount;
    public List<Trace> Traces = new List<Trace>(4096);

    public float MAX_RAY_DISTANCE = 50f;
    [Range(0, 128)]
    public int TOTAL_HEADINGS = 16;
    [Range(0, 4)]
    public int MAX_BOUNCES = 2;

    public void Schedule() {
        Traces.Clear();
        for (int i = 0; i < LightSourceCount; i++) {
            Execute(i, LightSources[i]);
        }
    }

    public void Execute(int index, LightSource source) {
        const float TOTAL_RADIANS = 2 * math.PI;

        float TOTAL_HEADINGS_F = (float)TOTAL_HEADINGS;
        float PER_RAY_ENERGY = source.Energy / TOTAL_HEADINGS_F;

        for (int i = 0; i < TOTAL_HEADINGS; i++) {
            float radians = ((float)i / TOTAL_HEADINGS_F) * TOTAL_RADIANS;
            float3 localHeading = UnitCircle(radians);
            float3 worldHeading = normalize(source.transform.TransformVector(localHeading));
            Ray ray = new Ray(source.transform.position, worldHeading);

            if (Physics.Raycast(ray, out RaycastHit hit, MAX_RAY_DISTANCE) && hit.transform.GetComponent<Reflector>()) {
                Traces.Add(new Trace { From = ray.origin, To = hit.point, Energy = source.Energy });
                Bounce(MAX_BOUNCES, source.Energy, ray, hit);
            } else {
                Traces.Add(new Trace { From = ray.origin, To = ray.direction * MAX_RAY_DISTANCE, Energy = source.Energy });
            }
        }
    }

    float3 Reflect(float3 N, float3 L) {
        return 2 * dot(N, L) * N - L;
    }

    float3 Tangent(float3 v) {
        return new float3(v.z, 0, -v.x);
    }

    float3 Binormal(float3 normal, float3 tangent) {
        return cross(normal, tangent);
    }

    float3 TangentToWorldSpace(float3 tangent, float3 binormal, float3 normal, float3 v) {
        return tangent * v.x + binormal * v.y + normal * v.z;
    }

    float3[] ReflectionVectors(float3 tangent, float3 binormal, float3 normal, float3 incidentVector, int count) {
        float3[] vs = new float3[count];
        float totalReflectionRadians = PI;
        float radiansBetweenEachRay = totalReflectionRadians / (float)(count - 1);

        for (int i = 0; i < count; i++) {
            float radians = (float)i * radiansBetweenEachRay;

            vs[i] = TangentToWorldSpace(tangent, binormal, normal, new float3(cos(radians), 0, sin(radians)));
        }
        return vs;
    }

    // TODO: Use these to avoid allocations
    float3[] reflectionVectors = new float3[128];
    float[] reflectionVectorEnergies = new float[128];
    float3[] normalizedReflectionVectorEnergies = new float3[128];

    void Bounce(int depth, float incidentEnergy, Ray incidentRay, RaycastHit incidentHit) {
        if (depth <= 0)
            return;

        float3 reflectionVector = Reflect(incidentHit.normal, -incidentRay.direction);
        Ray ray = new Ray(incidentHit.point, reflectionVector);
        float energy = incidentEnergy;

        if (Physics.Raycast(ray, out RaycastHit hit, MAX_RAY_DISTANCE)) {
            Traces.Add(new Trace { From = ray.origin, To = hit.point, Energy = energy });
            Bounce(depth - 1, energy, ray, hit);
        } else {
            Traces.Add(new Trace { From = ray.origin, To = ray.direction * MAX_RAY_DISTANCE, Energy = energy });
        }


        /*
        float3 normal = incidentHit.normal;
        float3 tangent = Tangent(normal);
        float3 binormal = Binormal(normal, tangent);
        float3 reflectionVector = Reflect(incidentHit.normal, -incidentRay.direction);
        float3[] reflectionVectors = ReflectionVectors(tangent, binormal, normal, -incidentRay.direction, TOTAL_HEADINGS / 2 + 1);
        AnimationCurve reflectionCurve = incidentHit.transform.GetComponent<RaystrikeableBody>().RayMaterial.ReflectionCurve;
        float[] reflectionVectorEnergies = reflectionVectors.Select(v => reflectionCurve.Evaluate(dot(reflectionVector, v))).ToArray();

        float totalReflectionEnergy = reflectionVectorEnergies.Sum();

        if (totalReflectionEnergy == 0)
            return;

        float[] normalizedReflectionVectorEnergies = reflectionVectorEnergies.Select(v => v / totalReflectionEnergy).ToArray();

        for (int i = 0; i < reflectionVectors.Length; i++) {
            float3 v = reflectionVectors[i];
            float energy = normalizedReflectionVectorEnergies[i] * incidentEnergy;
            Ray ray = new Ray(incidentHit.point, v);

            if (Physics.Raycast(ray, out RaycastHit hit, MAX_RAY_DISTANCE)) {
                Traces.Add(new Trace { From = ray.origin, To = hit.point, Energy = energy });
                Bounce(depth - 1, energy, ray, hit);
            } else {
                Traces.Add(new Trace { From = ray.origin, To = ray.direction * MAX_RAY_DISTANCE, Energy = energy });
            }
        }
        */
    }
}