using System.Collections.Generic;
using UnityEngine;

public class LightSink : MonoBehaviour, ILightStrikeable {
  Light enabledLight;

  public int BeamStrikesThisFrame = 0;

  void Start() {
    enabledLight = gameObject.GetComponentInChildren<Light>();
    enabledLight.enabled = false;
  }

  public List<LightBeam> ComputeOutgoingLightBeams(LightBeam input) {
    return new List<LightBeam>();
  }

  public void OnCollide(LightBeam lb) {
    BeamStrikesThisFrame++;
  }
}