using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchSelectObject : MonoBehaviour {
  public Board Board = null;
  public PlayObject ObjectToSelect = null;

  public void OnSwitchToggled(bool on) {
    if (on && ObjectToSelect) {
      Board.SelectedObject = ObjectToSelect;
    }
  }
}
