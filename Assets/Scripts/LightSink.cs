using UnityEngine;

public class LightSink : MonoBehaviour {
  Light enabledLight;

  void Start() {
    enabledLight = gameObject.GetComponentInChildren<Light>();
    enabledLight.enabled = false;
  }
}
