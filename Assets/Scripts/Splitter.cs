using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Splitter : PlayObject {
  [SerializeField] int[] BeamHeadingOffsets = { -1, 1 };
  [SerializeField] LightBeamColor[] BeamColors = { LightBeamColor.white, LightBeamColor.white };

  public override List<LightBeam> ComputeOutgoingLightBeams(LightBeam input) {
    return Enumerable.Range(0, BeamHeadingOffsets.Length).Select(
      i => new LightBeam { Color = BeamColors[i], Heading = (8 + input.Heading + Heading + BeamHeadingOffsets[i]) % 8 }).ToList();
  }

  public override void OnQuarterBeat(int counter) {
    if (counter%4 == 2)
      Orientation += 2;
  }

  public void OnDrawGizmos() {
    Gizmos.color = UnityEngine.Color.green;
    foreach (int heading in BeamHeadingOffsets) {
      int fullHeading = (8 + heading + Heading) % 8;
      Vector2Int dir = Board.Vector2IntHeadings[fullHeading];
      Gizmos.DrawLine(transform.position, transform.position + new Vector3(dir[0], 0, dir[1]) * 7f);
    }
  }
}