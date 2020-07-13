using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable] public class BoolEvent : UnityEvent<bool> { }

public class Switch : PlayObject {
  public BoolEvent OnToggled = null;
  public bool On = false;

  [SerializeField] string AnimatorStateParameterName = "On";
  [SerializeField] Animator Animator = null;

  private void OnEnable() {
    Animator.SetBool(AnimatorStateParameterName, On);
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
    Animator.SetBool(AnimatorStateParameterName, On);
    OnToggled?.Invoke(on);
  }
}
