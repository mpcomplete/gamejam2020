using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Splitter : LightStrikeableBase {
  // 0-15, 0 is forward, n is n/16th of a revolution around.
  public int Orientation {
    get => (int)((transform.eulerAngles.y+.1) * 16f / 360f);
    set {
      Vector3 tmp = transform.eulerAngles;
      tmp.y = (value%16) * 360f / 16f;
      transform.eulerAngles = tmp;
    }
  }

  public override List<LightBeam> ComputeOutgoingLightBeams(LightBeam input) {
    int[] headings = { -1, 1 };
    return headings.Select(heading => new LightBeam { Color = input.Color, Heading = (8 + input.Heading + heading + Orientation/2) % 8 }).ToList();
  }

  public override void OnQuarterBeat(int counter) {
    if (counter%4 == 2)
      Orientation += 2;
  }
}