using UnityEngine;

[System.Serializable]
public class LightBeam {
  public Color Color = Color.black;
  public int Heading = 0;

  public LightBeam(Color color, int heading) {
    Color = color;
    Heading = heading;
  }
}