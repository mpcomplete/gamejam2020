using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LightNode {
  public PlayObject Object;
  public Vector2Int Position;
  public List<LightBeam> LightBeams = new List<LightBeam>();
  public List<LightNode> LightNodes = new List<LightNode>();
}