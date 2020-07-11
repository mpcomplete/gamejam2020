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
  [SerializeField] Board Board = null;
  [SerializeField] Board[] Boards = null;

  [Header("Scene Objects")]
  [SerializeField] SelectionIndicator SelectionIndicator = null;
  [SerializeField] LineRenderer[] LineRenderers = null;
  int LineRendererIndex = 0;

  [Header("UI")]
  [SerializeField] BoardCompleteOverlay BoardCompleteOverlay = null;

  [Header("Gameplay")]
  [SerializeField] float BeatPeriodInMS = 1000f;

  int BoardIndex = 0;
  GameState State = GameState.CompletedBoard;

  void Start() {
    SelectedMirror = Board.GetComponentInChildren<Mirror>();
    BoardCompleteOverlay.ClickableRegion.onClick.AddListener(LoadNextBoard);
    State = GameState.ActiveBoard;
  }

  void OnDestroy() {
    BoardCompleteOverlay.ClickableRegion.onClick.RemoveListener(LoadNextBoard);
  }

  [ContextMenu("Load Next Board")]
  public void LoadNextBoard() {
    Destroy(Board.gameObject);
    BoardIndex = (BoardIndex + 1) % Boards.Length;
    Board = Instantiate(Boards[BoardIndex]);
    SelectedMirror = Board.GetComponentInChildren<Mirror>();
    State = GameState.ActiveBoard;
    MusicAudioSource.Stop();
  }

  public static Vector3 GridToWorldPosition(Vector2Int gp) {
    return new Vector3(gp.x, 0, gp.y);
  }

  public static LightNode MarchLightTree(Board board, int maxDepth) {
    LightNode rootNode = new LightNode { Position = board.GetLightSourceCell() };

    rootNode.LightBeams = board.LightSource.ComputeOutgoingLightBeams(null);

    foreach (LightBeam lightBeam in rootNode.LightBeams) {
      rootNode.LightNodes.Add(March(board, rootNode.Position, lightBeam, maxDepth));
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
      return new LightNode { Position = position + vHeading * 100 };
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
    Vector3 BeamOffset = new Vector3(0, .5f, 0);

    for (int i = 0; i < tree.LightBeams.Count; i++) {
      LightBeam lb = tree.LightBeams[i];
      LightNode ln = tree.LightNodes[i];
      LineRenderer lr = LineRenderers[LineRendererIndex++];
      Vector3 origin = BeamOffset + GridToWorldPosition(tree.Position);
      Vector3 destination = BeamOffset + GridToWorldPosition(ln.Position);

      lr.gameObject.SetActive(true);
      lr.positionCount = 2;
      lr.SetPosition(0, origin);
      lr.SetPosition(1, destination);
      // TODO: maybe cache materials per lb.Color.
      lr.material.SetColor("_EmissionColor", lb.EmissionColor());
      RenderLightTree(ln);
    }
  }

  void DisableUnusedLineRenderers() {
    for (int i = LineRendererIndex; i < LineRenderers.Length; i++) {
      LineRenderers[i].gameObject.SetActive(false);
    }
  }

  public static KeyCode[] ObjectSelectionKeyCodes = new KeyCode[9] {
    KeyCode.Alpha1,
    KeyCode.Alpha2,
    KeyCode.Alpha3,
    KeyCode.Alpha4,
    KeyCode.Alpha5,
    KeyCode.Alpha6,
    KeyCode.Alpha7,
    KeyCode.Alpha8,
    KeyCode.Alpha9
  };

  public static KeyCode[] MovementKeyCodes = new KeyCode[4] {
    KeyCode.W,
    KeyCode.D,
    KeyCode.S,
    KeyCode.A
  };

  public static Vector2Int[] MovementDirections = new Vector2Int[4] {
    Vector2Int.up,
    Vector2Int.right,
    Vector2Int.down,
    Vector2Int.left
  };

  Mirror SelectedMirror = null;

  void Update() {
    float dt = Time.deltaTime;

    switch (State) {
    case GameState.ActiveBoard: {
        Mirror[] mirrors = Board.GetComponentsInChildren<Mirror>();
        if (Input.GetKeyDown(KeyCode.Delete))
          LoadNextBoard();

        for (int i = 0; i < mirrors.Length; i++) {
          if (Input.GetKeyDown(ObjectSelectionKeyCodes[i])) {
            SelectedMirror = mirrors[i];
          }
        }

        for (int i = 0; i < MovementDirections.Length; i++) {
          if (Input.GetKeyDown(MovementKeyCodes[i]) && SelectedMirror) {
            Vector2Int currentPosition = Board.GetObjectCell(SelectedMirror.gameObject);
            Vector2Int direction = MovementDirections[i];
            Vector2Int nextCell = currentPosition + direction;

            if (Board.ValidMoveLocation(nextCell)) {
              SelectedMirror.transform.position = GridToWorldPosition(nextCell);
            }
          }
        }

        if (SelectedMirror) {
          Vector2Int selectedGridCell = Board.GetObjectCell(SelectedMirror.gameObject);
          Vector3 selectedPosition = Vector3.up + GridToWorldPosition(selectedGridCell);
          Vector3 currentPosition = SelectionIndicator.transform.position;

          SelectionIndicator.gameObject.SetActive(true);
          // TODO: Slightly bad behavior... probably should happen in FixedUpdate
          SelectionIndicator.MoveTowards(dt, selectedPosition);
        } else {
          SelectionIndicator.gameObject.SetActive(false);
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

    case GameState.CompletedBoard: {
      SelectionIndicator.gameObject.SetActive(false);
      }
      break;
    }

    const int MAX_DEPTH = 6;
    LightNode LightTree = MarchLightTree(Board, MAX_DEPTH);

    LineRendererIndex = 0;
    RenderLightTree(LightTree);
    DisableUnusedLineRenderers();
  }

  float beatTimer = 0f;
  int quarterBeats = 0;
  void FixedUpdate() {
    switch (State) {
    case GameState.ActiveBoard:
      float quarterPeriod = BeatPeriodInMS / 1000f / 4f;
      while (beatTimer < Time.time) {
        OnQuarterBeat();
        beatTimer += quarterPeriod;
      }
      break;
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