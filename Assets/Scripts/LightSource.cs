using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightSource : LightStrikeableBase {
  public LightBeamColor Beam = LightBeamColor.white;
  public int Heading = 0;

  public override List<LightBeam> ComputeOutgoingLightBeams(LightBeam input) {
    List<LightBeam> result = new List<LightBeam>();
    if (input == null) {
      // Only generate beam on start, not on collision.
      result.Add(new LightBeam { Color = Beam, Heading = Heading });
    }
    return result;
  }

  public override void OnQuarterBeat(int counter) {
    if (counter%4 == 0)
      Heading = (Heading+1) % 8;
  }
}