using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using static MathUtils;

public class RaytracingTestController : MonoBehaviour {
    public RaytracingSystem RaytracingSystem;
    public SpaceField SpaceField;
    public LineRenderer[] LineRenderers;
    public Color BeamColor = Color.yellow;
    public float rotationSpeed;
    public float ScalePower = .1f;
    public AnimationCurve EnergyToIntensityCurve = AnimationCurve.Linear(0, 3, 1, 5);
    public AnimationCurve EnergyToWidthCurve = AnimationCurve.Linear(0, .05f, 1, .1f);

    public void Start() {
        // TODO: This is a stupid hack because this system is half-assed
        foreach (var og in FindObjectsOfType<OrbitGroup>()) {
            og.Position();
        }
    }

    public void FixedUpdate() {
        float dt = Time.fixedDeltaTime;

        // Update all orbiters
        Orbiter[] orbiters = FindObjectsOfType<Orbiter>();

        foreach (var orbiter in orbiters) {
            orbiter.Radians = (orbiter.Radians + (float)orbiter.Direction * TWO_PI / orbiter.Period * dt) % (TWO_PI);
            orbiter.transform.position = orbiter.transform.parent.position + orbiter.Radius * UnitCircle(orbiter.Radians);
        }

        // Update all rotators
        Rotator[] rotators = FindObjectsOfType<Rotator>();

        foreach (var rotator in rotators) {
            rotator.Radians = (rotator.Radians + (float)rotator.Direction * TWO_PI / rotator.Period * dt) % (TWO_PI);
            rotator.transform.LookAt(rotator.transform.parent.position + UnitCircle(rotator.Radians));
        }


        // Update all target reflectors
        TargetReflector[] targetReflectors = FindObjectsOfType<TargetReflector>();

        foreach (var targetreflector in targetReflectors) {
            float3 toSource = targetreflector.Source.transform.position - targetreflector.transform.position;
            float3 toTarget = targetreflector.Target.transform.position - targetreflector.transform.position;
            float3 halfway = normalize((normalize(toSource) + normalize(toTarget)) / 2);

            targetreflector.transform.forward = halfway;
        }


        // Raytracing for light sources
        RaytracingSystem.LightSources = FindObjectsOfType<LightSource>();
        RaytracingSystem.LightSourceCount = RaytracingSystem.LightSources.Length;
        RaytracingSystem.Schedule();

        // Raytrace rendering
        for (int i = 0; i < RaytracingSystem.Traces.Count; i++) {
            var trace = RaytracingSystem.Traces[i];
            var lr = LineRenderers[i];
            var logEnergy = pow(trace.Energy, ScalePower);
            var width = EnergyToWidthCurve.Evaluate(logEnergy);
            var intensity = EnergyToIntensityCurve.Evaluate(logEnergy);
            var emissionColor = intensity * BeamColor;

            lr.positionCount = 2;
            lr.SetPosition(0, trace.From);
            lr.SetPosition(1, trace.To);
            lr.startWidth = width;
            lr.endWidth = width;
            lr.material.SetColor("Color", BeamColor);
            lr.material.SetColor("_EmissionColor", emissionColor);
            lr.gameObject.SetActive(true);
        }
        for (int i = RaytracingSystem.Traces.Count; i < LineRenderers.Length; i++) {
            LineRenderers[i].gameObject.SetActive(false);
        }

        // Update the spacefield 
        NormalizedMass[] normalizedMasses = FindObjectsOfType<NormalizedMass>();

        SpaceField.Count = normalizedMasses.Length;
        SpaceField.NormalizedMasses = normalizedMasses;
        SpaceField.Schedule();
    }
}