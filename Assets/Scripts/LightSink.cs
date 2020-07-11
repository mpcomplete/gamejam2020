using System.Collections.Generic;
using UnityEngine;

public class LightSink : LightStrikeableBase {
  Light enabledLight;

  public int BeamStrikesThisFrame = 0;

  void Start() {
    enabledLight = gameObject.GetComponentInChildren<Light>();
    enabledLight.enabled = false;
  }

  public override List<LightBeam> ComputeOutgoingLightBeams(LightBeam input) {
    return new List<LightBeam>();
  }

  public override void OnCollide(LightBeam lb) {
    BeamStrikesThisFrame++;
    Debug.Log($"{BeamStrikesThisFrame} Hit the Sink!");
  }
}