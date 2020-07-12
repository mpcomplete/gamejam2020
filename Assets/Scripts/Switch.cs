using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnSwitchTriggered(bool enabled);

public class Switch : LightStrikeableBase {
  public OnSwitchTriggered OnSwitchTriggered = null;

  public override void OnCollide(LightBeam input) {
    HandleSwitchTriggered(true);
  }

  void HandleSwitchTriggered(bool enabled) {
    if (enabled)
      GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.yellow * 1.8f);
    else
      GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.black);
    //OnSwitchTriggered(enabled);
  }
}
