using System.Collections;
using UnityEngine;

public class SwitchDestroyTarget : MonoBehaviour {
  public AudioSource AudioSource = null;
  public AudioClip AudioClip = null;

  public void OnSwitchToggled(bool on) {
    if (on) {
      StartCoroutine(DestroySequence());
    }
  }

  IEnumerator DestroySequence() {
    const float duration = .5f;
    float timer = 0f;
    AudioSource.transform.SetParent(null);
    AudioSource.clip = AudioClip;
    AudioSource.Play();
    while (timer < duration) {
      transform.position += Vector3.down*Time.deltaTime/duration;
      yield return null;
      timer += Time.deltaTime;
    }
    Destroy(gameObject);

    yield return new WaitForSeconds(AudioClip.length);
    Destroy(AudioSource.gameObject);
  }
}
