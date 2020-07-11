using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightSource : LightStrikeableBase {
  public LightBeamColor[] Beams = { LightBeamColor.red, LightBeamColor.green, LightBeamColor.green };
  public int Heading = 0;

  public override List<LightBeam> ComputeOutgoingLightBeams(LightBeam input) {
    if (input == null) {
      // Only generate beams on start, not on collision.
      return Beams.Select(beam => new LightBeam { Color = beam, Heading = Heading }).ToList();
    }
    return new List<LightBeam>();
  }

  public override void OnQuarterBeat(int counter) {
    if (counter%4 == 0)
      Heading = (Heading+1) % 8;
  }
}