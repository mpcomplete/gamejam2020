using UnityEngine;

public class Board : MonoBehaviour {
  public GameObject StartLight;
  public GameObject GoalLight;

  void Start() {
    Debug.Log($"starty: {this.GetStartLightCell()}");
    Debug.Log($"3,5: {this.GetObjectAtCell(new Vector2Int(3, 5))}");
  }

  public Vector2Int GetStartLightCell() {
    return getObjectCell(StartLight);
  }
  public Vector2Int GetGoalLightCell() {
    return getObjectCell(GoalLight);
  }
  // TODO: unity raycast may be better.
  public GameObject GetObjectAtCell(Vector2Int cell) {
    for (int i = 0; i < transform.childCount; i++) {
      // TODO: use tags or something to make sure we're only getting mirrors, etc
      GameObject obj = transform.GetChild(i).gameObject;
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