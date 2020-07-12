using UnityEngine;

public class Game : MonoBehaviour {
  enum GameState { ActiveBoard, CompletedBoard, Ending }

  [SerializeField] int MAX_LIGHTBEAM_BOUNCES = 6;
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

  [Header("Gameplay")]
  [SerializeField] float BeatPeriodInMS = 1000f;

  int BoardIndex = 0;
  GameState State = GameState.ActiveBoard;
  Mirror SelectedMirror = null;
  float beatTimer = 0f;
  int quarterBeats = 0;

  void Start() {
    SelectedMirror = Board.GetComponentInChildren<Mirror>();
  }

  [ContextMenu("Load Next Board")]
  public void LoadNextBoard() {
    Destroy(Board.gameObject);
    BoardIndex = (BoardIndex + 1) % Boards.Length;
    Board = Instantiate(Boards[BoardIndex]);
    SelectedMirror = Board.GetComponentInChildren<Mirror>();
    State = GameState.ActiveBoard;
    MusicAudioSource.Stop();
    beatTimer = 0;
    quarterBeats = 0;
    Debug.Log($"Starting board with heading={Board.LightSource.Heading}, beat={quarterBeats}, {beatTimer}");
  }

  void RenderLightTree(LightNode tree) {
    LineRendererIndex = 0;
    RenderLightNode(tree);
    DisableUnusedLineRenderers();
  }

  void RenderLightNode(LightNode node) {
    const string EmissionColorName = "_EmissionColor";

    Vector3 BeamOffset = new Vector3(0, .5f, 0);

    for (int i = 0; i < node.LightBeams.Count; i++) {
      LightBeam lb = node.LightBeams[i];
      LightNode ln = node.LightNodes[i];
      LineRenderer lr = LineRenderers[LineRendererIndex++];
      Vector3 origin = BeamOffset + Board.GridToWorldPosition(node.Position);
      Vector3 destination = BeamOffset + Board.GridToWorldPosition(ln.Position);

      lr.gameObject.SetActive(true);
      lr.positionCount = 2;
      lr.SetPosition(0, origin);
      lr.SetPosition(1, destination);
      // TODO: maybe cache materials per lb.Color.
      lr.material.SetColor(EmissionColorName, lb.EmissionColor());
      RenderLightNode(ln);
    }
  }

  void DisableUnusedLineRenderers() {
    for (int i = LineRendererIndex; i < LineRenderers.Length; i++) {
      LineRenderers[i].gameObject.SetActive(false);
    }
  }

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

  /*
  When light hits an emitter it should glow brightly 
  If you have won stop the heartbeat and hold for some time
  Turn off the indicator
  Turn off the emitters
  Play the transition from the old to new board
  Play the trasformation animation for the new Emitters
  Turn on the indicator to resume play
  */

  // Active Board State
  void UpdateActiveBoard(float dt) {
    // Debug stuff.
    if (Input.GetKeyDown(KeyCode.N))
      LoadNextBoard();

    // Input handling
    for (int i = 0; i < MovementDirections.Length; i++) {
      if (Input.GetKeyDown(MovementKeyCodes[i]) && SelectedMirror) {
        Vector2Int currentPosition = Board.GetObjectCell(SelectedMirror.gameObject);
        Vector2Int direction = MovementDirections[i];
        Vector2Int nextCell = currentPosition + direction;

        if (Board.ValidMoveLocation(nextCell)) {
          SelectedMirror.transform.position = Board.GridToWorldPosition(nextCell);
        }
      }
    }

    // Selection indicator logic
    if (SelectedMirror) {
      Vector2Int selectedGridCell = Board.GetObjectCell(SelectedMirror.gameObject);
      Vector3 selectedPosition = Vector3.up + Board.GridToWorldPosition(selectedGridCell);

      SelectionIndicator.gameObject.SetActive(true);
      SelectionIndicator.MoveTowards(dt, selectedPosition);
    } else {
      SelectionIndicator.gameObject.SetActive(false);
    }

    // Update and draw the light beams
    {
      RenderLightTree(Board.MarchLightTree(MAX_LIGHTBEAM_BOUNCES));
    }

    if (Board.LightSink.BeamStrikesThisFrame > 0) {
      State = GameState.CompletedBoard;
      MusicAudioSource.clip = Board.WinningMusic;
      MusicAudioSource.Play();
    }
  }

  void FixedUpdateActiveBoard(float dt) {
    float quarterPeriod = BeatPeriodInMS / 1000f / 4f;

    while (beatTimer < Time.time) {
      beatTimer += quarterPeriod;
      quarterBeats++;
      if (quarterBeats % 4 == 0)
        BeatAudioSource.Play();

      foreach (LightStrikeableBase obj in Board.GetComponentsInChildren<LightStrikeableBase>()) {
        obj.OnQuarterBeat(quarterBeats);
      }
    }
  }


  // Completed Board State
  void UpdateCompletedBoard(float dt) {
    // Input handling
    if (Input.GetKeyDown(KeyCode.Space)) {
      LoadNextBoard();
    }

    SelectionIndicator.gameObject.SetActive(false);
  }

  void FixedUpdateCompletedBoard(float dt) {

  }


  // Ending State;
  void UpdateEnding(float dt) {

  }

  void FixedUpdateEnding(float dt) {

  }


  void Update() {
    float dt = Time.deltaTime;

    switch (State) {
    case GameState.ActiveBoard:
    UpdateActiveBoard(dt);
    break;

    case GameState.CompletedBoard:
    UpdateCompletedBoard(dt);
    break;

    case GameState.Ending:
    UpdateEnding(dt);
    break;
    }
  }

  void FixedUpdate() {
    float dt = Time.fixedDeltaTime;

    switch (State) {
    case GameState.ActiveBoard:
    FixedUpdateActiveBoard(dt);
    break;

    case GameState.CompletedBoard:
    FixedUpdateCompletedBoard(dt);
    break;

    case GameState.Ending:
    FixedUpdateEnding(dt);
    break;
    }
  }
}