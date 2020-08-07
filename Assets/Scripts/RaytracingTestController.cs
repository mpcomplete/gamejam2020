using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using static MathUtils;

public class RaytracingTestController : MonoBehaviour {
    public RaytracingSystem RaytracingSystem;
    public OrbitRenderingSystem OrbitRenderingSystem;
    public SpaceField SpaceField;
    public LineRenderer[] LineRenderers;
    public Color BeamColor = Color.yellow;
    public float rotationSpeed;
    public float ScalePower = .1f;
    public AnimationCurve EnergyToIntensityCurve = AnimationCurve.Linear(0, 3, 1, 5);
    public AnimationCurve EnergyToWidthCurve = AnimationCurve.Linear(0, .05f, 1, .1f);

    [Range(0, 10)]
    public float OrbitPredictorTimeInTheFuture = 1f;
    public float RangeAccuracy = .9f;

    public Velocity LaunchablePrefab;

    public void Start() {
        // TODO: This is a stupid hack because this system is half-assed
        foreach (var og in FindObjectsOfType<OrbitGroup>()) {
            og.Position();
        }
    }

    // How does a skyhook work? 
    // In the real world, there are multiple components to a successful skyhook system
    // Fundamentally, you must successfully intercept the skyhook, then ride the skyhook until
    // you have a trajectory that is roughly aligned with your destination's FUTURE
    // position/orientation based on your current speed. You then release from the skyhook and 
    // mostly coast or lightly navigate towards your destination until you are relatively close at 
    // which time you take active control back to successfully intercept your target

    // This neatly divides the skyhook system into three phases:
    //      Intercepting the skyhook from your current position
    //      Riding the skyhook waiting for a sufficiently safe path towards your destination's future orientation
    //      Intercepting your destination from your current position 

    // This implies that we will need to be able to calculate the future position and orientation of objects in question
    // This neccessarily requires us to make certain assumptions about the behaviors of these objects. Long story short,
    // we need to assume that they have stable or predictable motions and thus are assumed to be restricted to orbit
    // and rotation. However, we need to be able to answer these questions for objects that may be rotating, orbiting, and
    // even orbiting another thing that is orbiting something etc. In short, we have a chain of orbits that we must calculate
    // and then finally we must consider the rotation of the object itself.

    public void FixedUpdate() {
        float dt = Time.fixedDeltaTime;

        foreach (var deathTimer in FindObjectsOfType<DeathTimer>()) {
            deathTimer.Value -= dt;

            if (deathTimer.Value <= 0) {
                Destroy(deathTimer.gameObject);
            }
        }

        // Update all orbiters
        foreach (var orbiter in FindObjectsOfType<Orbiter>()) {
            orbiter.Radians = (orbiter.Radians + (float)orbiter.Direction * TWO_PI / orbiter.Period * dt) % (TWO_PI);
            orbiter.transform.localPosition = orbiter.Radius * UnitCircle(orbiter.Radians);
        }

        // Update all rotators
        foreach (var rotator in FindObjectsOfType<Rotator>()) {
            rotator.Radians = (rotator.Radians + (float)rotator.Direction * TWO_PI / rotator.Period * dt) % (TWO_PI);
            rotator.transform.localRotation = Quaternion.AngleAxis(-rotator.Radians * Mathf.Rad2Deg, Vector3.up);
        }

        foreach (var velocity in FindObjectsOfType<Velocity>()) {
            velocity.transform.position += dt * (Vector3)velocity.Value;
        }

        float3 FutureLocalPosition(Orbiter o, float t) {
            float radians = (o.Radians + (float)o.Direction * TWO_PI / o.Period * t) % (TWO_PI);
            float3 futurePosition = o.Radius * UnitCircle(radians);

            return futurePosition;
        }

        float FutureLocalRotation(Rotator r, float t) {
            return (r.Radians + (float)r.Direction * TWO_PI / r.Period * t) % (TWO_PI);
        }

        float3 FuturePosition(Transform transform, float t) {
            float3 futurePosition = new float3(0, 0, 0);

            while (transform != null) {
                if (transform.TryGetComponent<Rotator>(out Rotator r)) {
                    float radians = FutureLocalRotation(r, t);
                    float degrees = radians * Mathf.Rad2Deg; // approximation for conversion to degrees

                    futurePosition = Quaternion.AngleAxis(-degrees, Vector3.up) * (Vector3)futurePosition;
                }
                if (transform.TryGetComponent<Orbiter>(out Orbiter o)) {
                    futurePosition += FutureLocalPosition(o, t);
                } else {
                    futurePosition += (float3)transform.localPosition;
                }
                transform = transform.parent;
            }
            return futurePosition;
        }

        float3 Trajectory(Transform transform, float t, float deltaTime) {
            return (FuturePosition(transform, t + deltaTime) - FuturePosition(transform, t - deltaTime)) / (2 * deltaTime);
        }

        foreach (var orbitPredictor in FindObjectsOfType<OrbitPredictor>()) {
            int count = orbitPredictor.LineRenderer.positionCount;
            for (int i = 0; i < count; i++) {
                float time = (float)i / (float)(count - 1) * OrbitPredictorTimeInTheFuture;
                float3 velocity = Trajectory(orbitPredictor.transform, 0, .001f);
                float3 trajectory = normalize(velocity);
                float3 toTarget = new float3(0,0,0) - (float3)orbitPredictor.transform.position;
                float3 towardsTarget = normalize(toTarget);
                float trajectoryDotTowardsTarget = dot(trajectory, towardsTarget);
                bool isAcceptableRange = trajectoryDotTowardsTarget >= RangeAccuracy;

                orbitPredictor.LineRenderer.SetPosition(i, FuturePosition(orbitPredictor.transform, time));
                Debug.DrawRay(orbitPredictor.transform.position, trajectory, isAcceptableRange ? Color.green : Color.blue);
                
                if (isAcceptableRange) {
                    Velocity launchedObject = Instantiate(LaunchablePrefab, orbitPredictor.transform.position, Quaternion.identity);

                    launchedObject.Value = velocity;
                }
            }
        }

        // Update all target reflectors
        foreach (var targetreflector in FindObjectsOfType<TargetReflector>()) {
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

        // Render orbits
        OrbitRenderingSystem.Orbiters = FindObjectsOfType<Orbiter>();
        OrbitRenderingSystem.Count = OrbitRenderingSystem.Orbiters.Length;
        OrbitRenderingSystem.Schedule();

        // Update the spacefield 
        SpaceField.NormalizedMasses = FindObjectsOfType<NormalizedMass>();
        SpaceField.Count = SpaceField.NormalizedMasses.Length;
        SpaceField.Schedule();
    }
}