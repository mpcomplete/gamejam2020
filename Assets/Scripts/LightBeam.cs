using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LightBeamColor {
  red    = 1<<0,
  green  = 1<<1,
  blue   = 1<<2,
  yellow = red|green,
  purple = red|blue,
  cyan   = green|blue,
  white  = red|blue|green,
}

[System.Serializable]
public class LightBeam {
  public LightBeamColor Color = LightBeamColor.white;
  public int Heading = 0;

  static public LightBeamColor[] Colors = {
    LightBeamColor.red,
    LightBeamColor.green,
    LightBeamColor.blue,
    LightBeamColor.yellow,
    LightBeamColor.purple,
    LightBeamColor.cyan,
    LightBeamColor.white,
  };
  public Color EmissionColor() {
    switch (Color) {
    case LightBeamColor.red: return UnityEngine.Color.red;
    case LightBeamColor.green: return UnityEngine.Color.green;
    case LightBeamColor.blue: return UnityEngine.Color.blue;
    case LightBeamColor.yellow: return UnityEngine.Color.yellow;
    case LightBeamColor.purple: return UnityEngine.Color.magenta;
    case LightBeamColor.cyan: return new UnityEngine.Color(.2f, .5f, .5f);
    case LightBeamColor.white: return UnityEngine.Color.white;
    default: return UnityEngine.Color.grey;
    }
  }
}