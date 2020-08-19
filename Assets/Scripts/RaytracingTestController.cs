using System;
using System.Collections.Generic;
using System.Linq;
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
    public float G = 100f;

    [Range(0, 10)]
    public float OrbitPredictorTimeInTheFuture = 1f;
    public float RangeAccuracy = .9f;
    public float SkyHookTransitionEpsilon = 1e-4f;
    public float MassFactor = 100f;
    public float3 InitialHeading = float3(0,0,0);
    public float InitialSpeed;

    public List<Emitter> Emitters;
    public List<Velocity> Velocities;
    public List<DeathTimer> DeathTimers;
    public List<Orbiter> Orbiters;
    public List<FreeBody> FreeBodies;
    public List<Traveler> Travelers;
    public Rotator[] Rotators;
    public Geosynchronous[] Geosynchronouses;
    public OrbitPredictor[] OrbitPredictors;
    public TargetReflector[] TargetReflectors;
    public LightSource[] LightSources;
    public NormalizedMass[] NormalizedMasses;
    public SkyHookRocket[] SkyHookRockets;

    public void Start() {
        // TODO: This is a stupid hack because this system is half-assed
        foreach (var og in FindObjectsOfType<OrbitGroup>()) {
            og.Position();
        }

        // TODO: kind of a hacky way to set courses for all these predictive chasers
        foreach (var chaser in FindObjectsOfType<PredictiveChaser>()) {
            float tf = 3f;
            float3 p0 = chaser.transform.position;
            float3 v0 = InitialHeading * InitialSpeed;
            float3 pf = FuturePosition(chaser.Target, tf);
            float3 vf = FutureVelocity(chaser.Target, tf);

            chaser.TimeElapsed = 0;
            chaser.Trajectory = PathSystem.TrajectoryFrom(tf, p0, v0, pf, vf);
        }

        Emitters = FindObjectsOfType<Emitter>().ToList();
        FreeBodies = FindObjectsOfType<FreeBody>().ToList();
        Travelers = FindObjectsOfType<Traveler>().ToList();
        Orbiters = FindObjectsOfType<Orbiter>().ToList();
        Velocities = FindObjectsOfType<Velocity>().ToList();
        DeathTimers = FindObjectsOfType<DeathTimer>().ToList();

        Rotators = FindObjectsOfType<Rotator>();
        Geosynchronouses = FindObjectsOfType<Geosynchronous>();
        OrbitPredictors = FindObjectsOfType<OrbitPredictor>();
        TargetReflectors = FindObjectsOfType<TargetReflector>();
        LightSources = FindObjectsOfType<LightSource>();
        NormalizedMasses = FindObjectsOfType<NormalizedMass>();
        SkyHookRockets = FindObjectsOfType<SkyHookRocket>();
    }

    float3 FutureLocalPosition(Orbiter o, float t) {
        // TODO: Mass stubbed out here... should come from the body?
        const float MASS = 1;
        float period = Orbiter.Period(o, MASS, G);
        float radians = (o.Radians + (float)o.Direction * TWO_PI / period * t) % (TWO_PI);
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
    
    float3 FutureVelocity(Transform transform, float t) { 
        const float dt = .01f;

        return (FuturePosition(transform, t + dt) - FuturePosition(transform, t - dt)) / (2 * dt);
    }

    public struct Course {
        public float3 Origin;
        public float3 Destination;
        public float Duration;
    }

    void RenderPath(float3[] waypoints) {
        for (int i = 1; i < waypoints.Length; i++) {
            Debug.DrawLine(waypoints[i-1], waypoints[i], Color.white);
        }
    }

    public void Register(GameObject go) {
        if (go.TryGetComponent<Velocity>(out Velocity v)) {
            Velocities.Add(v);
        }
        if (go.TryGetComponent<DeathTimer>(out DeathTimer d)) {
            DeathTimers.Add(d);
        }
        if (go.TryGetComponent<FreeBody>(out FreeBody f)) {
            FreeBodies.Add(f);
        }
    }

    public void UnRegister(GameObject go) {
        if (go.TryGetComponent<Velocity>(out Velocity v)) {
            Velocities.Remove(v);
        }
        if (go.TryGetComponent<DeathTimer>(out DeathTimer d)) {
            DeathTimers.Remove(d);
        }
        if (go.TryGetComponent<FreeBody>(out FreeBody f)) {
            FreeBodies.Remove(f);
        }
    }

    public void FixedUpdate() {
        float dt = Time.fixedDeltaTime;

        foreach (var emitter in Emitters) {
            emitter.TimeTillEmission -= dt;

            if (emitter.TimeTillEmission <= 0) {
                GameObject emittee = Instantiate(emitter.Emittee, emitter.transform.position, emitter.transform.rotation);

                Register(emittee);
                if (emittee.TryGetComponent<Velocity>(out Velocity v)) {
                    v.Value = emitter.transform.forward * emitter.EmissionSpeed;
                }
                // TODO: note... this is not totally correct as we could slightly overshoot...
                // TODO: consider fixing this AND the initial position based on this slight overshoot to get perfect behavior
                emitter.TimeTillEmission = emitter.EmissionPeriod; 
            }
        }

        // Update deathtimers: modifies component arrays
        for (int i = 0; i < DeathTimers.Count; i++) {
            DeathTimer deathTimer = DeathTimers[i];

            deathTimer.Value -= dt;
            if (deathTimer.Value <= 0) {
                UnRegister(deathTimer.gameObject);
                Destroy(deathTimer.gameObject);
                i--;
            }
        }

        // Update all free bodies
        foreach (var freebody in FreeBodies) {
            Velocity v = freebody.GetComponent<Velocity>();

            foreach (var normalizedMass in NormalizedMasses) {
                float3 delta = normalizedMass.transform.position - freebody.transform.position;
                float3 direction = normalize(delta);
                float d = length(delta);

                v.Value += direction * dt * normalizedMass.Value * MassFactor * G / (d * d);
            }
        }

        // There are several versions of this system that will offer better and better behavior
        // The stupid chaser with fixed speed
        //      Constantly tries to move towards the CURRENT location of their target
        foreach (var chaser in FindObjectsOfType<Chaser>()) {
            float3 delta = chaser.Target.position - chaser.transform.position;
            float3 direction = normalize(chaser.Target.position - chaser.transform.position);
            float remainingDistance = min(length(delta), chaser.MaxSpeed * dt);

            chaser.transform.position = chaser.transform.position + remainingDistance * (Vector3)direction;
            Debug.DrawLine(chaser.transform.position, chaser.Target.position, Color.red);
        }

        // The predictive chaser
        //      Predicts the location of the target various times in the future
        //      Chooses the first time where the distance to the object is within its range for that given time based on its max speed
        //      Moves towards that future location at the required speed
        var predictiveChasers = FindObjectsOfType<PredictiveChaser>();
        foreach (var predictiveChaser in predictiveChasers) {
            predictiveChaser.TimeElapsed += dt;
            predictiveChaser.transform.position = PathSystem.PositionFromTrajectoryAtTime(predictiveChaser.Trajectory, predictiveChaser.TimeElapsed);
        }
        foreach (var predictiveChaser in predictiveChasers) {
            if (predictiveChaser.TimeElapsed >= predictiveChaser.Trajectory.duration) {
                predictiveChaser.transform.SetParent(predictiveChaser.Target, true);
                predictiveChaser.transform.localPosition = Vector3.zero;
                Destroy(predictiveChaser);
            }
        }

        foreach (var traveler in Travelers) {
            // every traveler needs to calculate the position of their target 
        }

        // Set the required radius of all orbiters that are geosynchronous
        foreach (var geosynchronous in Geosynchronouses) {
            if (geosynchronous.TryGetComponent<Orbiter>(out Orbiter orbiter)) {
                const float MASS = 1;

                orbiter.Radius = Orbiter.RequiredRadiusForPeriod(MASS, G, geosynchronous.Rotator.Period);
            }
        }

        // Update all orbiters
        foreach (var orbiter in Orbiters) {
            // TODO: This mass value is stubbed out here but probably should be taken from the body itself?
            const float MASS = 1;
            float period = Orbiter.Period(orbiter, MASS, G);

            orbiter.Radians = (orbiter.Radians + (float)orbiter.Direction * TWO_PI / period * dt) % (TWO_PI);
            orbiter.transform.localPosition = orbiter.Radius * UnitCircle(orbiter.Radians);
        }

        // Update all rotators
        foreach (var rotator in Rotators) {
            rotator.Radians = (rotator.Radians + (float)rotator.Direction * TWO_PI / rotator.Period * dt) % (TWO_PI);
            rotator.transform.localRotation = Quaternion.AngleAxis(-rotator.Radians * Mathf.Rad2Deg, Vector3.up);
        }

        // Update all target reflectors
        foreach (var targetreflector in TargetReflectors) {
            float3 toSource = targetreflector.Source.transform.position - targetreflector.transform.position;
            float3 toTarget = targetreflector.Target.transform.position - targetreflector.transform.position;
            float3 halfway = normalize((normalize(toSource) + normalize(toTarget)) / 2);

            targetreflector.transform.forward = halfway;
        }

        // Update Skyhook Rockets
        foreach (var skyhookRocket in SkyHookRockets) {
            switch (skyhookRocket.CurrentPlan) {
                case SkyHookRocket.Plan.EnterOrbit:
                skyhookRocket.TimeRemaining -= dt;

                if (skyhookRocket.TimeRemaining <= 0) {
                    Orbiter o = (Orbiter)skyhookRocket.gameObject.AddComponent(typeof(Orbiter));

                    o.Radians = 0;
                    o.Radius = skyhookRocket.orbitRadius;
                    o.Direction = Orbiter.RotationalDirection.CounterClockwise;
                    Orbiters.Add(o);
                    skyhookRocket.TimeRemaining = 0;
                    skyhookRocket.transform.SetParent(skyhookRocket.Origin, true);
                    skyhookRocket.CurrentPlan = SkyHookRocket.Plan.RideTheHook;
                } else {
                    float3 currentPosition = skyhookRocket.transform.position;
                    float3 futurePosition = FuturePosition(skyhookRocket.Origin, skyhookRocket.TimeRemaining);
                    float fraction = skyhookRocket.TimeRemaining / skyhookRocket.TransitionTime;

                    Debug.DrawLine(currentPosition, futurePosition, Color.red);
                    skyhookRocket.TimeRemaining -= dt;
                    skyhookRocket.transform.position = lerp(futurePosition, currentPosition, fraction);
                }

                break;

                case SkyHookRocket.Plan.RideTheHook:
                // Check if we should release 
                if (Input.GetKeyDown(KeyCode.Space)) {
                    float3 trajectory = FutureVelocity(skyhookRocket.transform, 0);
                    Orbiter orbiter = skyhookRocket.GetComponent<Orbiter>();
                    Velocity velocity = (Velocity)skyhookRocket.gameObject.AddComponent(typeof(Velocity));

                    Orbiters.Remove(orbiter);
                    Velocities.Add(velocity);
                    Destroy(orbiter);
                    velocity.Value = trajectory;
                    skyhookRocket.transform.SetParent(null, true);
                    skyhookRocket.transform.forward = normalize(trajectory);
                    skyhookRocket.CurrentPlan = SkyHookRocket.Plan.Free;
                }
                break;
            }
        }

        foreach (var velocity in Velocities) {
            velocity.transform.position += dt * (Vector3)velocity.Value;
        }

        foreach (var orbitPredictor in OrbitPredictors) {
            int count = orbitPredictor.LineRenderer.positionCount;
            for (int i = 0; i < count; i++) {
                float time = (float)i / (float)(count - 1) * OrbitPredictorTimeInTheFuture;
                float3 velocity = FutureVelocity(orbitPredictor.transform, 0);
                float3 trajectory = normalize(velocity);
                float3 toTarget = new float3(0,0,0) - (float3)orbitPredictor.transform.position;
                float3 towardsTarget = normalize(toTarget);
                float trajectoryDotTowardsTarget = dot(trajectory, towardsTarget);

                orbitPredictor.LineRenderer.SetPosition(i, FuturePosition(orbitPredictor.transform, time));
            }
        }

        // Raytracing for light sources
        RaytracingSystem.LightSources = LightSources;
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
        OrbitRenderingSystem.Orbiters = Orbiters;
        OrbitRenderingSystem.Count = OrbitRenderingSystem.Orbiters.Count;
        OrbitRenderingSystem.Schedule();

        // Update the spacefield 
        SpaceField.NormalizedMasses = NormalizedMasses;
        SpaceField.Count = SpaceField.NormalizedMasses.Length;
        SpaceField.Schedule();
    }
}