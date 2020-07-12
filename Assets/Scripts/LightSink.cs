﻿using System.Collections.Generic;
using UnityEngine;

public class LightSink : PlayObject {
  public Animator Animator;
  public int BeamStrikesThisFrame = 0;

  public override List<LightBeam> ComputeOutgoingLightBeams(LightBeam input) {
    return new List<LightBeam>();
  }

  public override void OnCollide(List<LightBeam> lb) {
    BeamStrikesThisFrame++;
  }
}