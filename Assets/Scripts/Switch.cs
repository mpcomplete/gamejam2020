using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public delegate void OnSwitchToggled(bool on);
[System.Serializable] public class BoolEvent : UnityEvent<bool> { }

public class Switch : PlayObject {
  public BoolEvent OnToggled = null;
  public bool On = false;

  [Header("Rendering")]
  public MeshRenderer Renderer = null;
  public Material EnabledMaterial = null;
  public Material DisabledMaterial = null;

  private void OnEnable() {
    Renderer.material = On ? EnabledMaterial : DisabledMaterial;
  }

  public override void OnCollide(List<LightBeam> input) {
    if (!On)
      HandleSwitchToggled(true);
  }
  public override void OnNoncollide() {
    if (On)
      HandleSwitchToggled(false);
  }

  void HandleSwitchToggled(bool on) {
    On = on;
    Renderer.material = on ? EnabledMaterial : DisabledMaterial;
    OnToggled?.Invoke(on);
  }
}
