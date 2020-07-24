using UnityEngine;

public static class MathUtils {
  public static Vector3 ExponentialLerpTo(Vector3 a, Vector3 b, float epsilon, float dt) {
    return Vector3.Lerp(a, b, 1 - Mathf.Pow(epsilon, dt));
  }

  public static Quaternion ExponentialSlerpTo(Quaternion a, Quaternion b, float epsilon, float dt) {
    return Quaternion.Slerp(a, b, 1 - Mathf.Pow(epsilon, dt));
  }

  public static Vector3 GridToWorldPosition(Vector2Int gp) {
    return new Vector3(gp.x, 0, gp.y);
  }

  public static Vector2Int WorldPositionToGrid(Vector3 v) {
    return new Vector2Int((int)v.x, (int)v.z);
  }
}