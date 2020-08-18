using UnityEngine;

public class SkyHookRocket : MonoBehaviour {
    public enum Plan { EnterOrbit, RideTheHook, Free }

    public Plan CurrentPlan = Plan.EnterOrbit;
    public Transform Origin = null;
    public Transform Destination = null;
    public Transform Target = null;
    public float orbitRadius = 5f;
    public float TransitionTime = 1f;
    public float TimeRemaining = 1f;
}