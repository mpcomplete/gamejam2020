using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Splitter : LightStrikeableBase {
  public override List<LightBeam> ComputeOutgoingLightBeams(LightBeam input) {
    int[] headings = { -1, 1 };
    return headings.Select(heading => new LightBeam { Color = input.Color, Heading = (8 + input.Heading + heading + Orientation/2) % 8 }).ToList();
  }

  public override void OnQuarterBeat(int counter) {
    if (counter%4 == 2)
      Orientation += 2;
  }
}