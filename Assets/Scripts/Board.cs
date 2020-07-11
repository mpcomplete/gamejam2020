using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {
  public LightSource LightSource;
  public LightSink LightSink;
  public Vector2Int Min = Vector2Int.zero;
  public Vector2Int Max = new Vector2Int(9, 9);
  public AudioClip WinningMusic;

  public Vector2Int GetLightSourceCell() {
    return getObjectCell(LightSource.gameObject);
  }

  public Vector2Int GetLightSinkCell() {
    return getObjectCell(LightSink.gameObject);
  }

  public bool OutOfBounds(Vector2Int v) {
      return v.x < Min.x || v.y < Min.y || v.x > Max.x || v.y > Max.y;
  }

  public IEnumerable<GameObject> GetChildren() {
    for (int i = 0; i < transform.childCount; i++) {
      // TODO: use tags or something to make sure we're only getting useful objects.
      GameObject obj = transform.GetChild(i).gameObject;
      yield return obj;
    }
  }

  // TODO: unity raycast may be better.
  public GameObject GetObjectAtCell(Vector2Int cell) {
    foreach (GameObject obj in GetChildren()) {
      if (getObjectCell(obj) == cell)
        return obj;
    }
    return null;
  }

  // Returns the cell position for the given object. Assumes its parent transform is the Board's.
  Vector2Int getObjectCell(GameObject obj) {
    return new Vector2Int((int)obj.transform.position.x, (int)obj.transform.position.z);
  }
}