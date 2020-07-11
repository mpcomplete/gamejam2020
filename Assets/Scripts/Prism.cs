using System.Collections.Generic;
using UnityEngine;

public class Prism : LightStrikeableBase {
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
    var result = new List<LightBeam>();
    int[] headingAdjustMap = { 0, 1, 2, 3 };
    int headingAdjust = headingAdjustMap[(int)input.Color];
    result.Add(new LightBeam { Color = input.Color, Heading = (input.Heading + headingAdjust) % 8 });
    return result;
  }

  public override void OnQuarterBeat(int counter) {
    if (counter%4 == 2)
      Orientation += 1;
  }
}