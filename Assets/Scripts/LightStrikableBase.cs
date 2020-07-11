using System.Collections.Generic;
using UnityEngine;

public abstract class LightStrikeableBase : MonoBehaviour {
  public virtual List<LightBeam> ComputeOutgoingLightBeams(LightBeam input) {
    return new List<LightBeam>();
  }
  public virtual void OnCollide(LightBeam input) { }
}