using System.Collections.Generic;
using UnityEngine;

// TODO: rename? Interactable?
public abstract class LightStrikeableBase : MonoBehaviour {
  // Called during LightBeam collision to gather all the resulting LightBeams (e.g. for a reflection).
  public virtual List<LightBeam> ComputeOutgoingLightBeams(LightBeam input) {
    return new List<LightBeam>();
  }
  // Called when a LightBeam collides with this object.
  public virtual void OnCollide(LightBeam input) { }
  // Called 4 times per beat. `counter` increases by 1 each call.
  public virtual void OnQuarterBeat(int counter) { }
}