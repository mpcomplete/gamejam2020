using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightSource : PlayObject {
  public Star Star;
  public bool DebugMode = false;

  public override List<LightBeam> ComputeOutgoingLightBeams(LightBeam input) {
    List<LightBeam> result = new List<LightBeam>();
    if (input == null) {
      // Only generate beam on start, not on collision.
      if (DebugMode) {
        return Enumerable.Range(0, 8).Select(i => new LightBeam {
          Color = LightBeam.Colors[i % LightBeam.Colors.Length],
          Heading = i
        }).ToList();
      }
      result.Add(new LightBeam { Color = LightBeamColor.white, Heading = Heading });
      // result.Add(new LightBeam { Color = LightBeamColor.white, Heading = (Heading+2) % 8 });
      result.Add(new LightBeam { Color = LightBeamColor.white, Heading = (Heading+4) % 8 });
      // result.Add(new LightBeam { Color = LightBeamColor.white, Heading = (Heading+6) % 8 });
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