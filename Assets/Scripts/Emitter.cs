using System;
using Unity.Mathematics;
using UnityEngine;

public class Emitter : MonoBehaviour {
    public GameObject Emittee;
    public float EmissionSpeed = 1f;
    public float EmissionPeriod = 1f;
    public float TimeTillEmission = 1f;
}