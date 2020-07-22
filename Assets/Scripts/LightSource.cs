using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightSource : PlayObject {
  [SerializeField] BeamConfiguration BeamConfiguration = null;
  public Star Star;

  public override List<LightBeam> ComputeOutgoingLightBeams(LightBeam input) {
    List<LightBeam> result = new List<LightBeam>();

    // TODO: I don't really love this... maybe change?
    // Only generate beam on start, not on collision.
    if (input != null) {
      return result;
    }

    foreach (var spec in BeamConfiguration.BeamSpecs) {
      result.Add(new LightBeam(spec, Heading));
    }
    return result;
  }

  public override void OnQuarterBeat(int counter) { 
    if (counter%4 == 0) {
      Heading = (Heading+1) % 8;
    }
  }

  public void OnDrawGizmos() {
    Gizmos.color = UnityEngine.Color.green;
    Vector2Int dir = Board.Vector2IntHeadings[Heading];
    Gizmos.DrawLine(transform.position, transform.position + new Vector3(dir[0], 0, dir[1]) * 7f);
  }
}