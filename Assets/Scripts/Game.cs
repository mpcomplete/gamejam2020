using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Game : MonoBehaviour {
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

  enum GameState { ActiveBoard, CompletedBoard, Ending }

  [SerializeField] int MAX_LIGHTBEAM_BOUNCES = 6;
  [Header("Audio")]
  [SerializeField] AudioSource MusicAudioSource = null;
  [SerializeField] AudioSource BeatAudioSource = null;
  [SerializeField] AudioClip WinningMusic = null;

  [Header("Boards")]
  [SerializeField] Board Board = null;
  [SerializeField] Board[] Boards = null;

  [Header("Scene Objects")]
  [SerializeField] SelectionIndicator SelectionIndicator = null;
  [SerializeField] LineRenderer[] LineRenderers = null;

  [Header("Gameplay")]
  [SerializeField] float BeatPeriodInMS = 1000f;
  [SerializeField] float PauseOnVictoryDuration = 2f;
  [SerializeField] float TransformSinksDuration = 1f;
  [SerializeField] float RotationalEpsilon = 1e-5f;


  int BoardIndex = 0;
  int LineRendererIndex = 0;
  GameState State = GameState.ActiveBoard;
  Mirror SelectedMirror = null;
  float beatTimer = 0f;
  int quarterBeats = 0;

  void Start() {
    // TODO: This is a stupid hack but I am bad at thought
    foreach (var source in Board.GetSources())
      source.Animator.Play("Extend Arms", -1, 0);
    SelectedMirror = Board.GetComponentInChildren<Mirror>();
    StartLevel(Board);
  }

  void StartLevel(Board board) {
    Board = board;
    SelectedMirror = Board.GetComponentInChildren<Mirror>();
    State = GameState.ActiveBoard;
    MusicAudioSource.Stop();
    beatTimer = Time.time;
    quarterBeats = 0;
    //SelectionIndicator.gameObject.SetActive(true);
    DebugDumpLevel("Starting");
  }

  void DebugDumpLevel(string prefix) {
    var tmp = String.Join(",", Board.GetComponentsInChildren<Mirror>().Select(m => m.Orientation.ToString()));
    Debug.Log($"{prefix} level {BoardIndex} Source:{Board.LightSource.Heading}, mirrors:{tmp}");
  }

  void RenderLightTree(LightNode tree) {
    RenderLightNode(tree);
  }

  void RenderLightNode(LightNode node) {
    const string EmissionColorName = "_EmissionColor";

    Vector3 BeamOffset = new Vector3(0, .7f, 0);

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

  // Active Board State
  void UpdateActiveBoard(float dt) {
    // Movement handling
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
      var noncollided = new HashSet<LightStrikeableBase>(Board.GetPlayObjects());
      var collided = new Dictionary<LightStrikeableBase, List<LightBeam>>();
      var lightTree = Board.MarchLightTree(collided, MAX_LIGHTBEAM_BOUNCES);

      foreach (var kv in collided) {
        noncollided.Remove(kv.Key);
        kv.Key.OnCollide(kv.Value);
      }
      foreach (var obj in noncollided)
        obj.OnNoncollide();

      RenderLightTree(lightTree);
    }

    if (Input.GetKeyDown(KeyCode.Equals) || Board.LightSink.BeamStrikesThisFrame > 0) {
      StartCoroutine(LevelCompletionSequence());
    }
    if (Input.GetKeyDown(KeyCode.Minus)) {
      Destroy(Board.gameObject);
      BoardIndex = (BoardIndex + 1) % Boards.Length;
      Board newBoard = Instantiate(Boards[BoardIndex]);
      StartLevel(newBoard);
    }
  }

  void FixedUpdateActiveBoard(float dt) {
    float quarterPeriod = BeatPeriodInMS / 1000f / 4f;

    while (beatTimer < Time.time) {
      beatTimer += quarterPeriod;
      quarterBeats++;
      if (quarterBeats % 4 == 0)
        BeatAudioSource.Play();

      foreach (LightStrikeableBase obj in Board.GetPlayObjects()) {
        obj.OnQuarterBeat(quarterBeats);
      }
    }
    foreach (LightStrikeableBase obj in Board.GetPlayObjects()) {
      int orientation = obj.Orientation;
      Quaternion targetRotation = Quaternion.Euler(obj.transform.eulerAngles.x, orientation * 360f / 16f, obj.transform.eulerAngles.z);
      float t = 1.0f - Mathf.Pow(RotationalEpsilon, dt);

      obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, targetRotation, t);
    }
  }

  IEnumerator LevelCompletionSequence() {
    float victoryTimer = 0f;

    DebugDumpLevel("Won");
    State = GameState.CompletedBoard;
    MusicAudioSource.clip = WinningMusic;
    MusicAudioSource.Play();
    SelectionIndicator.gameObject.SetActive(false);

    // hold with lasers drawn and light emitting from sinks
    while (victoryTimer < PauseOnVictoryDuration) {
      var noncollided = new HashSet<LightStrikeableBase>(Board.GetPlayObjects());
      var collided = new Dictionary<LightStrikeableBase, List<LightBeam>>();
      var lightTree = Board.MarchLightTree(collided, MAX_LIGHTBEAM_BOUNCES);

      foreach (var kv in collided) {
        noncollided.Remove(kv.Key);
        kv.Key.OnCollide(kv.Value);
      }
      foreach (var obj in noncollided)
        obj.OnNoncollide();

      RenderLightTree(lightTree);
      yield return null;
      victoryTimer += Time.deltaTime;
    }

    // transition out the old board
    LightSink[] oldSinks = Board.GetSinks();
    foreach (LightSink sink in oldSinks) {
      sink.transform.SetParent(null, true);
    }
    yield return new WaitForSeconds(Board.PlayOutro());
    Destroy(Board.gameObject);
    foreach (LightSink sink in oldSinks) {
      Destroy(sink.gameObject);
    }

    // transition in the new board
    BoardIndex = (BoardIndex + 1) % Boards.Length;
    Board newBoard = Instantiate(Boards[BoardIndex]);
    LightSource[] newSources = newBoard.GetSources();
    foreach (LightSource source in newSources) {
      source.transform.SetParent(null, true);
      source.Animator.Play("Power Up", -1, 0);
    }
    yield return new WaitForSeconds(newBoard.PlayIntro());
    foreach (LightSource source in newSources) {
      source.transform.SetParent(newBoard.transform, true);
      source.Animator.Play("Extend Arms", -1, 0);
    }

    // transform Sinks to Emitters
    yield return new WaitForSeconds(TransformSinksDuration);

    // enter play mode
    StartLevel(newBoard);
  }

  // Ending State;
  void UpdateEnding(float dt) {

  }

  void FixedUpdateEnding(float dt) {

  }

  void Update() {
    float dt = Time.deltaTime;

    LineRendererIndex = 0;
    switch (State) {
    case GameState.ActiveBoard:
      UpdateActiveBoard(dt);
      break;

    case GameState.CompletedBoard:
      break;

    case GameState.Ending:
      UpdateEnding(dt);
      break;
    }
    DisableUnusedLineRenderers();
  }

  void FixedUpdate() {
    float dt = Time.fixedDeltaTime;

    switch (State) {
    case GameState.ActiveBoard:
      FixedUpdateActiveBoard(dt);
      break;

    case GameState.CompletedBoard:
      break;

    case GameState.Ending:
      FixedUpdateEnding(dt);
      break;
    }
  }
}