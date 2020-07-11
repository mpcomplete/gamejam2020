using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightSource : LightStrikeableBase {
  readonly Color[] colors = { Color.red, Color.green, Color.blue };
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
  }
}