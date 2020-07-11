using System;
using UnityEngine;

public class Game : MonoBehaviour {
  [Header("Prefabs")]

  [SerializeField] Board[] Boards = null;
  [SerializeField] LightBeam SourceBeam = null;

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

    // Let's just cast against the current board with our sample lightbeam
    SourceBeam.Cast(Board, Vector2Int.zero, Vector2Int.up);
    SourceBeam.Render();
  }
}