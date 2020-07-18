using System.Collections.Generic;
using UnityEngine;

public class LightSink : PlayObject {
  public int BeamStrikesThisFrame = 0;
  public Star Star;

  public override List<LightBeam> ComputeOutgoingLightBeams(LightBeam input) {
    return new List<LightBeam>();
  }

  public override void OnCollide(List<LightBeam> lb) {
    BeamStrikesThisFrame = lb.Count;
    Star.SetHit(true);
  }

  public override void OnNoncollide() {
    BeamStrikesThisFrame = 0;
    Star.SetHit(false);
  }
}