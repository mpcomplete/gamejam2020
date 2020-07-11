using Cinemachine.Utility;
using System;
using UnityEngine;

public class Game : MonoBehaviour {
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

  enum GameState { ActiveBoard, CompletedBoard }

  [Header("Audio")]
  [SerializeField] AudioSource MusicAudioSource = null;
  [SerializeField] AudioSource BeatAudioSource = null;

  [Header("Boards")]
  [SerializeField] Board[] Boards = null;

  [Header("Pools")]
  [SerializeField] LineRenderer[] LineRenderers = null;
  int LineRendererIndex = 0;

  [Header("UI")]
  [SerializeField] BoardCompleteOverlay BoardCompleteOverlay = null;

  [Header("Gameplay")]
  [SerializeField] float BeatPeriodInMS = 1000f;

  Board Board;
  int BoardIndex = 0;
  GameState State = GameState.CompletedBoard;

  void Start() {
    BoardCompleteOverlay.ClickableRegion.onClick.AddListener(LoadNextBoard);
    Board = Instantiate(Boards[0]);
    State = GameState.ActiveBoard;
  }

  void OnDestroy() {
    BoardCompleteOverlay.ClickableRegion.onClick.RemoveListener(LoadNextBoard);
  }

  [ContextMenu("Load Next Board")]
  public void LoadNextBoard() {
    int nextBoardIndex = BoardIndex + 1 >= Boards.Length ? 0 : BoardIndex + 1;

    Destroy(Board.gameObject);
    BoardIndex = nextBoardIndex;
    Board = Instantiate(Boards[BoardIndex]);
    State = GameState.ActiveBoard;
    MusicAudioSource.Stop();
  }

  public static Vector3 GridToWorldPosition(Vector2Int gp) {
    return new Vector3(gp.x, 1, gp.y);
  }

  public static LightNode MarchLightTree(Board board, Vector2Int origin, int heading, int maxDepth) {
    LightNode rootNode = new LightNode { Position = origin };

    rootNode.LightBeams.Add(new LightBeam { Color = Color.red, Heading = heading });
    rootNode.LightBeams.Add(new LightBeam { Color = Color.green, Heading = heading });
    rootNode.LightBeams.Add(new LightBeam { Color = Color.blue, Heading = heading });

    foreach (LightBeam lightBeam in rootNode.LightBeams) {
      rootNode.LightNodes.Add(March(board, origin, lightBeam, maxDepth));
    }
    return rootNode;
  }

  public static LightNode March(Board board, Vector2Int position, LightBeam beam, int depth) {
    Vector2Int vHeading = Vector2IntHeadings[beam.Heading];
    Vector2Int nextCell = position + vHeading;

    if (depth < 0) {
      return new LightNode { Position = nextCell };
    }

    if (board.OutOfBounds(nextCell)) {
      return new LightNode { Position = nextCell };
    }

    GameObject target = board.GetObjectAtCell(nextCell);

    if (target && target.TryGetComponent(out LightStrikeableBase strikeable)) {
      LightNode targetNode = new LightNode { Position = nextCell };

      strikeable.OnCollide(beam);
      targetNode.LightBeams = strikeable.ComputeOutgoingLightBeams(beam);

      foreach (LightBeam lb in targetNode.LightBeams) {
        targetNode.LightNodes.Add(March(board, nextCell, lb, depth - 1));
      }
      return targetNode;
    } else {
      return March(board, nextCell, beam, depth);
    }
  }

  void RenderLightTree(LightNode tree) {
    for (int i = 0; i < tree.LightBeams.Count; i++) {
      LightBeam lb = tree.LightBeams[i];
      LightNode ln = tree.LightNodes[i];
      LineRenderer lr = LineRenderers[LineRendererIndex++];
      Vector3 origin = GridToWorldPosition(tree.Position);
      Vector3 destination = GridToWorldPosition(ln.Position);

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

  public static bool DoDebug = false;

  void Update() {
    switch (State) {
    case GameState.ActiveBoard: {
        KeyCode[] levelCodes = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };
        for (int i = 0; i < levelCodes.Length; i++) {
          if (Input.GetKeyDown(levelCodes[i])) {
            Destroy(Board.gameObject);
            Board = Instantiate(Boards[i]);
            break;
          }
        }

        KeyCode[] mirrorCodes = { KeyCode.Q, KeyCode.W, KeyCode.E };
        for (int i = 0; i < mirrorCodes.Length; i++) {
          if (Input.GetKeyDown(mirrorCodes[i])) {
            Mirror[] mirrors = Board.GetComponentsInChildren<Mirror>();
            if (mirrors != null && i < mirrors.Length) {
              int dir = Input.GetKey(KeyCode.LeftShift) ? -1 : 1;
              mirrors[i].Orientation = mirrors[i].Orientation + dir;
              DoDebug = true;
            }
          }
        }

        BoardCompleteOverlay.gameObject.SetActive(false);

        if (Board.LightSink.BeamStrikesThisFrame > 0) {
          State = GameState.CompletedBoard;
          BoardCompleteOverlay.gameObject.SetActive(true);
          MusicAudioSource.clip = Board.WinningMusic;
          MusicAudioSource.Play();
        }
      }
      break;

    case GameState.CompletedBoard:
      break;
    }

    const int MAX_DEPTH = 6;
    LightNode LightTree = MarchLightTree(Board, Board.GetLightSourceCell(), 0, MAX_DEPTH);

    LineRendererIndex = 0;
    RenderLightTree(LightTree);
    DisableUnusedLineRenderers();
  }

  float beatTimer = 0f;
  int quarterBeats = 0;
  void FixedUpdate() {
    float quarterPeriod = BeatPeriodInMS / 1000f / 4f;
    while (beatTimer < Time.time) {
      OnQuarterBeat();
      beatTimer += quarterPeriod;
    }
  }

  // Called 4 times per "beat", defined by BeatPeriodInMS.
  public void OnQuarterBeat() {
    quarterBeats++;
    if (quarterBeats % 4 == 0)
      BeatAudioSource.Play();

    foreach (GameObject child in Board.GetChildren()) {
      if (child.TryGetComponent(out LightStrikeableBase obj))
        obj.OnQuarterBeat(quarterBeats);
    }
  }
}