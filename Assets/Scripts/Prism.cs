using System.Collections.Generic;
using UnityEngine;

public class Prism : LightStrikeableBase {
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