using UnityEngine;

public enum LightBeamColor {
  black = 0,
  red,
  blue,
  green,
}

[System.Serializable]
public class LightBeam {
  public LightBeamColor Color = LightBeamColor.black;
  public int Heading = 0;

  static public readonly Color[] BeamColorToEmissionColor = {
    UnityEngine.Color.black,
    UnityEngine.Color.red*1.8f,
    UnityEngine.Color.cyan*1.8f,
    UnityEngine.Color.green*1.8f,
  };
  public Color EmissionColor() {
    return BeamColorToEmissionColor[(int)this.Color];
  }
}