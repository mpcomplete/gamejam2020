using System.Collections.Generic;
using UnityEngine;

public class LightSink : PlayObject {
  public Animator Animator;
  public int BeamStrikesThisFrame = 0;

  public override List<LightBeam> ComputeOutgoingLightBeams(LightBeam input) {
    return new List<LightBeam>();
  }

  public override void OnCollide(List<LightBeam> lb) {
    BeamStrikesThisFrame = lb.Count;
    Animator.Play("Lit", -1, 0f);
    Animator.StopPlayback();
  }

  public override void OnNoncollide() {
    BeamStrikesThisFrame = 0;
    Animator.Play("Idle", -1, 0f);
  }
}