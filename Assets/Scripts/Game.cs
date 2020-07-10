using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {
    [Header("Prefabs")]

    [SerializeField] Board[] Boards = null;

    Board Board;

    void Start()
    {
      Board = Instantiate(Boards[0]);
    }

    void Update() {
      if (Input.GetKeyDown(KeyCode.Space))
      {
        Destroy(Board.gameObject);
        Board = Instantiate(Boards[1]);
      }
    }
}