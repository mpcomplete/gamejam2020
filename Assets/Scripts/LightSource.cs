using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightSource : LightStrikeableBase {
  public LightBeamColor Color = LightBeamColor.white;
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
      result.Add(new LightBeam { Color = Color, Heading = Heading });
    }
    return result;
  }

  public override void OnQuarterBeat(int counter) {
    if (counter%4 == 0)
      Heading = (Heading+1) % 8;
  }

  public void OnDrawGizmos() {
    Gizmos.color = UnityEngine.Color.green;
    Vector2Int dir = Board.Vector2IntHeadings[Heading];
    Gizmos.DrawRay(transform.position, new Vector3(dir[0], 0, dir[1]));
  }
}