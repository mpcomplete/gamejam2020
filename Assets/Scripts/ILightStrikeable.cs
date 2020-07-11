using System.Collections.Generic;
using UnityEngine;

public interface ILightStrikeable
{
    List<LightBeam> ComputeOutgoingLightBeams(LightBeam input);
    void OnCollide(LightBeam input);
}