using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightSource : LightStrikeableBase {
  readonly LightBeamColor[] colors = { LightBeamColor.red, LightBeamColor.green, LightBeamColor.blue };
  int[] headings = { 1, 2, 3 };

  public override List<LightBeam> ComputeOutgoingLightBeams(LightBeam input) {
    if (input == null) {
      // Only generate beams on start, not on collision.
      return Enumerable.Range(0, 3).Select(i => new LightBeam { Color = colors[i], Heading = headings[i] }).ToList();
    }
    return new List<LightBeam>();
  }

  public override void OnQuarterBeat(int counter) {
    if (counter%4 == 0)
      headings[0] = (headings[0]+1) % 8;
    if (counter%4 == 0 || counter%4 == 1)
      headings[1] = (headings[1]+1) % 8;
    headings[2] = (headings[2]+1) % 8;
  }
}