using System.Collections.Generic;
using UnityEngine;

public class Splitter : PlayObject {
  public override List<LightBeam> ComputeOutgoingLightBeams(LightBeam input) {
    List<LightBeam> outputs = new List<LightBeam>(2);

    outputs.Add(new LightBeam(input.Color, (8 + Heading - 1) % 8));
    outputs.Add(new LightBeam(input.Color, (8 + Heading + 1) % 8));
    return outputs;
  }

  public override void OnQuarterBeat(int counter) {
    if (counter%4 == 2)
      Orientation += 2;
  }

  public void OnDrawGizmos() {
    Gizmos.color = UnityEngine.Color.green;

    {
      int heading = (8 + Heading - 1) % 8;
      Vector2Int dir = Board.Vector2IntHeadings[heading];

      Gizmos.DrawLine(transform.position, transform.position + new Vector3(dir[0], 0, dir[1]) * 7f);
    }
    {
      int heading = (8 + Heading + 1) % 8;
      Vector2Int dir = Board.Vector2IntHeadings[heading];

      Gizmos.DrawLine(transform.position, transform.position + new Vector3(dir[0], 0, dir[1]) * 7f);
    }
  }
}