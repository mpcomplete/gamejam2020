using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

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

  public static Vector3 GridToWorldPosition(Vector2Int gp) {
    return new Vector3(gp.x, 0, gp.y);
  }

  [Header("Animation")]
  [SerializeField] PlayableDirector PlayableDirector = null;
  [SerializeField] PlayableAsset IntroPlayable = null;
  [SerializeField] PlayableAsset OutroPlayable = null;

  public PlayObject SelectedObject;
  public Vector2Int Min = Vector2Int.zero;
  public Vector2Int Max = new Vector2Int(10, 10);
  public AudioClip Music;

  public LightNode MarchLightTree(LightSource source, Dictionary<PlayObject, List<LightBeam>> collisions, int maxDepth) {
    LightNode root = new LightNode {
      Object = source,
      Position = GetObjectCell(source.gameObject),
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

    if (OutOfBounds(nextCell)) {
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

  public float PlayIntro() {
    PlayableDirector.playableAsset = IntroPlayable;
    PlayableDirector.Play();
    return (float)IntroPlayable.duration;
  }

  public float PlayOutro() {
    PlayableDirector.playableAsset = OutroPlayable;
    PlayableDirector.Play();
    return (float)OutroPlayable.duration;
  }

  public bool ValidMoveLocation(Vector2Int v) {
    return !OutOfBounds(v) && HasFloorAtCell(v) && (GetObjectAtCell(v) == null);
  }

  public bool OutOfBounds(Vector2Int v) {
    return v.x < Min.x || v.y < Min.y || v.x > Max.x || v.y > Max.y;
  }

  public bool IsVictory() => GetSinks().All(s => s.BeamStrikesThisFrame > 0);

  public PlayObject[] GetPlayObjects() => GetComponentsInChildren<PlayObject>();
  public LightSink[] GetSinks() => GetComponentsInChildren<LightSink>();
  public LightSource[] GetSources() => GetComponentsInChildren<LightSource>();

  // TODO: unity raycast may be better.
  public PlayObject GetObjectAtCell(Vector2Int cell) {
    foreach (PlayObject obj in GetPlayObjects()) {
      if (GetObjectCell(obj.gameObject) == cell)
        return obj;
    }
    return null;
  }

  public Vector2Int GetObjectCell(GameObject obj) {
    return new Vector2Int((int)obj.transform.position.x, (int)obj.transform.position.z);
  }

  public bool HasFloorAtCell(Vector2Int cell) {
    return Physics.Raycast(GridToWorldPosition(cell), Vector3.down, .5f);
  }
}