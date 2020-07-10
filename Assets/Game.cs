using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {
  [Header("Prefabs")]
  [SerializeField]
  GameObject[] CrystalPrefab;

  GameObject startLight;
  GameObject goalLight;

  // Start is called before the first frame update
  void Start() {
    startLight = Instantiate(CrystalPrefab[0]);
    startLight.transform.position = new Vector3(0, 0, -10);

    goalLight = Instantiate(CrystalPrefab[1]);
    goalLight.transform.position = new Vector3(5, 0, 5);
  }

  // Update is called once per frame
  void Update() {

  }
}
