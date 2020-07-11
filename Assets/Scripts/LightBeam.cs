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
  public LightBeamColor Color = LightBeamColor.red;
  public int Heading = 0;

  public Color EmissionColor() {
    switch (Color) {
    case LightBeamColor.red: return UnityEngine.Color.red*1.8f;
    case LightBeamColor.green: return UnityEngine.Color.green*1.8f;
    case LightBeamColor.blue: return UnityEngine.Color.cyan*1.8f;
    case LightBeamColor.yellow: return UnityEngine.Color.yellow*1.8f;
    case LightBeamColor.purple: return UnityEngine.Color.magenta*1.8f;
    case LightBeamColor.cyan: return new UnityEngine.Color(.2f, .5f, .5f)*2.5f;
    case LightBeamColor.white: return UnityEngine.Color.white*2.5f;
    default: return UnityEngine.Color.grey;
    }
  }
}