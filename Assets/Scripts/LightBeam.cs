using UnityEngine;

[System.Serializable]
public class LightBeam {
  public Color Color = Color.white;
  public int Heading = 0;

  public LightBeam(BeamConfiguration.BeamSpec spec, int heading) {
    Color = spec.Color;
    Heading = (heading + spec.HeadingOffset) % 8;
  }

  public LightBeam(Color color, int heading) {
    Color = color;
    Heading = heading;
  }
}