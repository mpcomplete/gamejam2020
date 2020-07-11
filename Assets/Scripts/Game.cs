using Cinemachine.Utility;
using System;
using UnityEngine;

public class Game : MonoBehaviour {
  [Header("Prefabs")]

  [SerializeField] Board[] Boards = null;

  Board Board;

  void Start() {
    Board = Instantiate(Boards[0]);
  }

  void Update() {
    KeyCode[] levelCodes = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };
    for (int i = 0; i < levelCodes.Length; i++) {
      if (Input.GetKeyDown(levelCodes[i])) {
        Destroy(Board.gameObject);
        Board = Instantiate(Boards[i]);
        break;
      }
    }
  }
}

// A number from 0 to 15 representing the possible orientation of an object.
// "0" is "forward". "n" is n/16th of a revolution around.
public struct Orientation {
  private const float fraction = 360f / 16f;
  private int value;

  public Orientation(int value) {
    Debug.Assert(value >= 0 && value <= 7);
    this.value = value;
  }

  public float toAngle() => value * fraction;
  public static Orientation fromAngle(float angle) => new Orientation((int)(angle / fraction));
  public static implicit operator int(Orientation o) => o.value;
}

