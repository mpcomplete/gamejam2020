using Cinemachine.Utility;
using System;
using UnityEngine;

public class Game : MonoBehaviour {
  public static Vector2Int[] Vector2IntHeadings = new Vector2Int[8]
  {
    new Vector2Int(0, 1),   // north
    new Vector2Int(1, 1),   // north east
    new Vector2Int(1, 0),   // east
    new Vector2Int(1, -1),  // south east
    new Vector2Int(0, -1),  // south
    new Vector2Int(-1, -1), // south west
    new Vector2Int(-1, 0),   // west
    new Vector2Int(-1, 1)    // north west
  };

  [Header("Boards")]
  [SerializeField] Board[] Boards = null;

  [Header("Pools")] [SerializeField] LineRenderer[] LineRenderers = null;
  int LineRendererIndex = 0;

  public LightNode LightTree;

  Board Board;

  void Start() {
    Board = Instantiate(Boards[0]);
  }

  public static Vector3 GridToWorldPosition(Vector2Int gp) {
    return new Vector3(gp.x, 1, gp.y);
  }

  // Temporary "fake" reflection stuff
  public static Vector2Int ReflectedHeading(Vector2Int heading) {
    return new Vector2Int(heading.y, heading.x);
  }

  public static LightNode MarchLightTree(Board board, Vector2Int origin, int heading, int maxDepth) {
    LightNode rootNode = new LightNode { Depth = 0, Position = origin };

    rootNode.LightBeams.Add(new LightBeam { Color = Color.red, Heading = 0 });
    rootNode.LightBeams.Add(new LightBeam { Color = Color.green, Heading = 0 });
    rootNode.LightBeams.Add(new LightBeam { Color = Color.blue, Heading = 0 });

    foreach (LightBeam lightBeam in rootNode.LightBeams) {
      rootNode.LightNodes.Add(March(board, origin, lightBeam, 1, maxDepth));
    }
    return rootNode;
  }

  public static LightNode March(Board board, Vector2Int position, LightBeam beam, int depth, int maxDepth) {
    Vector2Int vHeading = Vector2IntHeadings[beam.Heading];
    Vector2Int nextCell = position + vHeading;

    // TODO: is this totally correct?
    if (depth >= maxDepth) {
      return new LightNode { Depth = depth, Position = nextCell };
    }

    if (board.OutOfBounds(nextCell)) {
      return new LightNode { Depth = depth, Position = nextCell };
    }

    GameObject target = board.GetObjectAtCell(nextCell);

    if (target && target.TryGetComponent(out Mirror mirror)) {
      LightNode targetNode = new LightNode { Depth = depth, Position = nextCell };

      LightBeam[] beams = mirror.OnCollide(beam);
      foreach (LightBeam newbeam in beams) {
        targetNode.LightBeams.Add(newbeam);
      }

      foreach (LightBeam lb in targetNode.LightBeams) {
        targetNode.LightNodes.Add(March(board, nextCell, lb, depth + 1, maxDepth));
      }
      return targetNode;
    } else {
      return March(board, nextCell, beam, depth, maxDepth);
    }
  }

  int count = 0;
  void RenderLightTree(LightNode tree) {
    for (int i = 0; i < tree.LightBeams.Count; i++) {
      LightBeam lb = tree.LightBeams[i];
      LightNode ln = tree.LightNodes[i];
      LineRenderer lr = LineRenderers[LineRendererIndex++];
      Vector3 origin = GridToWorldPosition(tree.Position);
      Vector3 destination = GridToWorldPosition(ln.Position);

      count++;

      // need to position this line renderer in world space
      lr.gameObject.SetActive(true);
      lr.positionCount = 2;
      lr.SetPosition(0, origin);
      lr.SetPosition(1, destination);
      RenderLightTree(ln);
    }
  }

  void DisableUnusedLineRenderers() {
    for (int i = LineRendererIndex; i < LineRenderers.Length; i++) {
      LineRenderers[i].gameObject.SetActive(false);
    }
  }

  void Update() {
    KeyCode[] levelCodes = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };
    for (int i = 0; i < levelCodes.Length; i++) {
      if (Input.GetKeyDown(levelCodes[i])) {
        Destroy(Board.gameObject);
        Board = Instantiate(Boards[i]);
        break;
      }
    }

    if (Input.GetKeyDown(KeyCode.Z)) {
      Mirror firstMirror = Board.GetComponentInChildren<Mirror>();
      if (firstMirror) {
        Vector3 angles = firstMirror.transform.eulerAngles;
        angles.y += 22.5f;
        firstMirror.transform.eulerAngles = angles;
      }
    }

    const int MAX_DEPTH = 6;
    LightTree = MarchLightTree(Board, Vector2Int.zero, 0, MAX_DEPTH);

    count = 0;
    LineRendererIndex = 0;
    RenderLightTree(LightTree);
    DisableUnusedLineRenderers();
    Debug.Log(count);
  }
}

// A number from 0 to 15 representing the possible orientation of an object.
// "0" is "forward". "n" is n/16th of a revolution around.
public struct Orientation {
  private const float fraction = 360f / 16f;
  private int value;

  public Orientation(int value) {
    Debug.Assert(value >= 0 && value <= 7);
    this.value = value;
  }

  public float toAngle() => value * fraction;
  public static Orientation fromAngle(float angle) => new Orientation((int)(angle / fraction));
  public static implicit operator int(Orientation o) => o.value;
}

