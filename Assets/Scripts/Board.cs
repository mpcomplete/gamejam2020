using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {
  public LightSource LightSource;
  public LightSink LightSink;
  public Vector2Int Min = Vector2Int.zero;
  public Vector2Int Max = new Vector2Int(10, 10);
  public AudioClip WinningMusic;

  public Vector2Int GetLightSourceCell() {
    return GetObjectCell(LightSource.gameObject);
  }

  public Vector2Int GetLightSinkCell() {
    return GetObjectCell(LightSink.gameObject);
  }

  public bool ValidMoveLocation(Vector2Int v) {
    return !OutOfBounds(v) && (GetObjectAtCell(v) == null);
  }

  public bool OutOfBounds(Vector2Int v) {
    return v.x < Min.x || v.y < Min.y || v.x > Max.x || v.y > Max.y;
  }

  // TODO: unity raycast may be better.
  public LightStrikeableBase GetObjectAtCell(Vector2Int cell) {
    foreach (LightStrikeableBase obj in GetComponentsInChildren<LightStrikeableBase>()) {
      if (GetObjectCell(obj.gameObject) == cell)
        return obj;
    }
    return null;
  }

  // Returns the cell position for the given object. Assumes its parent transform is the Board's.
  public Vector2Int GetObjectCell(GameObject obj) {
    return new Vector2Int((int)obj.transform.position.x, (int)obj.transform.position.z);
  }
}