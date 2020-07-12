using UnityEngine;

public delegate void OnSwitchToggled(bool on);

public class Switch : LightStrikeableBase {
  public OnSwitchToggled OnToggled = null;
  public bool On = false;
  public MeshRenderer Renderer = null;
  public Material EnabledMaterial = null;
  public Material DisabledMaterial = null;

  private void OnEnable() {
    Renderer.material = On ? EnabledMaterial : DisabledMaterial;
  }

  public override void OnCollide(LightBeam input) {
    HandleSwitchToggled(true);
  }

  void HandleSwitchToggled(bool on) {
    On = on;
    Renderer.material = on ? EnabledMaterial : DisabledMaterial;
    OnToggled?.Invoke(on);
  }

}
