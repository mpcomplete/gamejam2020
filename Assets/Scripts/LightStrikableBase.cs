using System.Collections.Generic;
using UnityEngine;

// TODO: rename? Interactable?
public abstract class LightStrikeableBase : MonoBehaviour {
  // 0-15, 0 is forward, n is n/16th of a revolution around.
  [SerializeField] int orientation = 0;
  public int Orientation {
    get => orientation;
    set => orientation = (value+16)%16;
  }
  public int Heading {
    get => orientation/2;
    set => Orientation = value*2;
  }

  // Called during LightBeam collision to gather all the resulting LightBeams (e.g. for a reflection).
  public virtual List<LightBeam> ComputeOutgoingLightBeams(LightBeam input) {
    return new List<LightBeam>();
  }
  // Called every frame that one or more LightBeams collide with this object.
  public virtual void OnCollide(List<LightBeam> input) { }
  // Called every frame that nothing collides with this object.
  public virtual void OnNoncollide() { }
  // Called 4 times per beat. `counter` increases by 1 each call.
  public virtual void OnQuarterBeat(int counter) { }

  // Editor-only.
  void OnValidate() {
    transform.eulerAngles = new Vector3(transform.eulerAngles.x, orientation * 360f / 16f, transform.eulerAngles.z);
  }
}