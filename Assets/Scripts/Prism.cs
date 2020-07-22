using System.Collections.Generic;
using UnityEngine;

public class Prism : PlayObject {
  public override List<LightBeam> ComputeOutgoingLightBeams(LightBeam input) {
    var result = new List<LightBeam>();

    result.Add(new LightBeam(input.Color, (Heading - 2) % 8));
    result.Add(new LightBeam(input.Color, (Heading - 1) % 8));
    result.Add(new LightBeam(input.Color, (Heading - 0) % 8));
    result.Add(new LightBeam(input.Color, (Heading + 1) % 8));
    result.Add(new LightBeam(input.Color, (Heading + 2) % 8));
    return result;
  }

  public override void OnQuarterBeat(int counter) {
    if (counter%4 == 2)
      Orientation += 1;
  }
}