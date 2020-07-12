using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnSwitchToggled(bool on);

public class Switch : LightStrikeableBase {
  public OnSwitchToggled OnToggled = null;
  public bool On = false;
  public MeshRenderer Renderer = null;
  public Material EnabledMaterial = null;
  public Material DisabledMaterial = null;
  public GameObject OnTriggerVanishTarget = null;
  [Header("Audio")]
  public AudioSource AudioSource = null;
  public AudioClip VanishTargetClip = null;

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

    if (On && OnTriggerVanishTarget != null) {
      StartCoroutine(VanishTarget());
    }
  }

  IEnumerator VanishTarget() {
    const float duration = .5f;
    float timer = 0f;
    AudioSource.clip = VanishTargetClip;
    AudioSource.Play();
    while (timer < duration) {
      OnTriggerVanishTarget.transform.position += Vector3.down*Time.deltaTime/duration;
      yield return null;
      timer += Time.deltaTime;
    }
    Destroy(OnTriggerVanishTarget);
    OnTriggerVanishTarget = null;
  }
}
