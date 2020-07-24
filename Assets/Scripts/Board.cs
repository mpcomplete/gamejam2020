using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MathUtils;

public class Board : MonoBehaviour {
  public static Vector2Int[] Vector2IntHeadings = new Vector2Int[8] {
    new Vector2Int(0, 1),   // north
    new Vector2Int(1, 1),   // north east
    new Vector2Int(1, 0),   // east
    new Vector2Int(1, -1),  // south east
    new Vector2Int(0, -1),  // south
    new Vector2Int(-1, -1), // south west
    new Vector2Int(-1, 0),  // west
    new Vector2Int(-1, 1)   // north west
  };

  public LightSink[] LightSinks;
  public LightSource[] LightSources;
  public PlayObject SelectedObject;
  public Vector2Int Min = Vector2Int.zero;
  public Vector2Int Max = new Vector2Int(10, 10);
  public AudioClip Music;
  public Metronome Metronome;

  public LightNode MarchLightTree(LightSource source, Dictionary<PlayObject, List<LightBeam>> collisions, int maxDepth) {
    LightNode root = new LightNode {
      Object = source,
      Position = WorldPositionToGrid(source.transform.position),
      LightBeams = source.ComputeOutgoingLightBeams(null)
    };
    foreach (LightBeam lightBeam in root.LightBeams) {
      root.LightNodes.Add(March(root.Position, lightBeam, collisions, maxDepth));
    }
    return root;
  }

  public LightNode March(Vector2Int position, LightBeam beam, Dictionary<PlayObject, List<LightBeam>> collisions, int depth) {
    Vector2Int vHeading = Vector2IntHeadings[beam.Heading];
    Vector2Int nextCell = position + vHeading;

    if (depth < 0) {
      return new LightNode { Position = nextCell };
    }

    if (!InBounds(nextCell)) {
      return new LightNode { Position = position + vHeading * 100 };
    }

    PlayObject target = GetObjectAtCell(nextCell);
    if (target) {
      LightNode targetNode = new LightNode { Object = target, Position = nextCell };

      if (!collisions.ContainsKey(target))
        collisions[target] = new List<LightBeam>();
      collisions[target].Add(beam);

      targetNode.LightBeams = target.ComputeOutgoingLightBeams(beam);

      foreach (LightBeam lb in targetNode.LightBeams) {
        targetNode.LightNodes.Add(March(nextCell, lb, collisions, depth - 1));
      }
      return targetNode;
    } else {
      return March(nextCell, beam, collisions, depth);
    }
  }

  public int ObjectsWith<A, B>(GameObject[] objects) where A : MonoBehaviour where B : MonoBehaviour {
    int index = 0;
    foreach (A a in GetComponentsInChildren<A>()) {
      if (a.GetComponent<B>()) {
        objects[index] = a.gameObject;
        index++;
      }
    }
    return index;
  }

  public bool ValidMoveLocation(Vector2Int v) {
    return InBounds(v) && (GetObjectAtCell(v) == null);
  }

  public bool InBounds(Vector2Int v) {
    return v.x >= Min.x && v.x <= Max.x && v.y >= Min.y && v.y <= Max.y;
  }

  public bool IsVictory() {
    return LightSinks.All(s => s.BeamStrikesThisFrame > 0);
  }

  public PlayObject[] GetPlayObjects() {
    return GetComponentsInChildren<PlayObject>();
  }

  public PlayObject GetObjectAtCell(Vector2Int cell) {
    foreach (PlayObject obj in GetPlayObjects()) {
      if (obj.TryGetComponent(out TargetMover tm)) {
        if (tm.TargetCell == cell) {
          return obj;
        }
      } else {
        if (WorldPositionToGrid(obj.transform.position) == cell) {
          return obj;
        }
      }
    }
    return null;
  }

  public void OnDrawGizmos() {
    Vector3 ll = new Vector3(Min.x, 0, Min.y);
    Vector3 ul = new Vector3(Min.x, 0, Max.y);
    Vector3 ur = new Vector3(Max.x, 0, Max.y);
    Vector3 lr = new Vector3(Max.x, 0, Min.y);

    Gizmos.color = Min.x < Max.x && Min.y < Max.y ? Color.green : Color.red;
    Gizmos.DrawLine(ll, ul);
    Gizmos.DrawLine(ul, ur);
    Gizmos.DrawLine(ur, lr);
    Gizmos.DrawLine(lr, ll);
  }
}