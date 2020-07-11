using System.Collections.Generic;
using UnityEngine;

public class LightSource : LightStrikeableBase {
    public override List<LightBeam> ComputeOutgoingLightBeams(LightBeam input)
    {
        return new List<LightBeam>();
    }

    public override void OnCollide(LightBeam input)
    {
    }
}