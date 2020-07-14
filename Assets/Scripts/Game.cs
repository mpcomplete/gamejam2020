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
  [SerializeField] AudioSource SFXAudioSource = null;
  [SerializeField] AudioClip VictoryAudioClip = null;
  [SerializeField] float MusicVolume = .2f;

  [Header("Boards")]
  [SerializeField] Board Board = null;
  [SerializeField] Board[] Boards = null;

  [Header("Scene Objects")]
  [SerializeField] SelectionIndicator SelectionIndicator = null;
  [SerializeField] LineRenderer[] LineRenderers = null;

  [Header("Gameplay")]
  [SerializeField] float BeamIntensity = 1f;
  [SerializeField] float MaximumBeamWidth = .1f;
  [SerializeField] Vector3 BeamOffset = new Vector3(0, .7f, 0);
  [SerializeField] AnimationCurve BeamWidthForShotFractionCurve = AnimationCurve.Linear(0, 0, 1, 1);
  [SerializeField] float PauseOnVictoryDuration = 2f;
  [SerializeField] float TransformSinksDuration = 1f;
  [SerializeField] float RotationalEpsilon = 1e-5f;

  int BoardIndex = 0;
  int LineRendererIndex = 0;
  GameState State = GameState.ActiveBoard;

  void Start() {
    // TODO: This is a stupid hack but I am bad at thought
    foreach (var source in Board.GetSources())
      source.Animator.Play("Extend Arms", -1, 0);
    StartLevel(Board);
  }

  void StartLevel(Board board) {
    Board = board;
    State = GameState.ActiveBoard;
    MusicAudioSource.Stop();
    MusicAudioSource.clip = Board.Music;
    MusicAudioSource.volume = MusicVolume;
    MusicAudioSource.Play();
    DebugDumpLevel("Starting");
  }

  void DebugDumpLevel(string prefix) {
    var tmp1 = String.Join(",", Board.GetComponentsInChildren<LightSource>().Select(o => o.Orientation.ToString()));
    var tmp2 = String.Join(",", Board.GetComponentsInChildren<Mirror>().Select(o => o.Orientation.ToString()));
    var tmp3 = String.Join(",", Board.GetComponentsInChildren<Splitter>().Select(o => o.Orientation.ToString()));
    Debug.Log($"{prefix} level {BoardIndex} Source:{tmp1}, mirrors:{tmp2}, splitters:{tmp3}");
  }

  void RenderLightNode(LightNode node, float width) {
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
      lr.startWidth = width;
      lr.endWidth = width;
      lr.material = GetBeamMaterial(lr, lb);
      RenderLightNode(ln, width);
    }
  }

  Material GetBeamMaterial(LineRenderer renderer, LightBeam beam) {
    Color color = beam.EmissionColor() * BeamIntensity;
    renderer.material.SetColor("_EmissionColor", color);
    return renderer.material;
  }

  void DisableUnusedLineRenderers() {
    for (int i = LineRendererIndex; i < LineRenderers.Length; i++) {
      LineRenderers[i].gameObject.SetActive(false);
    }
  }

  void UpdateLightTrees(float shotFraction) {
    var noncollided = new HashSet<PlayObject>(Board.GetPlayObjects());
    var collided = new Dictionary<PlayObject, List<LightBeam>>();
    var beamWidth = MaximumBeamWidth * BeamWidthForShotFractionCurve.Evaluate(shotFraction);

    foreach (LightSource source in Board.GetSources()) {
      RenderLightNode(Board.MarchLightTree(source, collided, MAX_LIGHTBEAM_BOUNCES), beamWidth);
    }
    foreach (var kv in collided) {
      noncollided.Remove(kv.Key);
      kv.Key.OnCollide(kv.Value);
    }
    foreach (var obj in noncollided)
      obj.OnNoncollide();
  }

  [SerializeField] bool DebugMode = false;
  int debugIndex = -1;
  PlayObject debugObject { get => debugIndex < 0 ? null : Board.GetPlayObjects()[debugIndex]; }

  // Active Board State
  void UpdateActiveBoard(float dt) {
    // Movement handling
    if (Board.SelectedObject) {
      for (int i = 0; i < MovementDirections.Length; i++) {
        if (Input.GetKeyDown(MovementKeyCodes[i])) {
          Vector2Int currentPosition = Board.GetObjectCell(Board.SelectedObject.gameObject);
          Vector2Int direction = MovementDirections[i];
          Vector2Int nextCell = currentPosition + direction;

          if (Board.ValidMoveLocation(nextCell)) {
            Board.SelectedObject.transform.position = Board.GridToWorldPosition(nextCell);
          }
        }
      }
    }

    // Selection indicator logic
    if (Board.SelectedObject) {
      Vector2Int selectedGridCell = Board.GetObjectCell(Board.SelectedObject.gameObject);
      Vector3 selectedPosition = Vector3.up + Board.GridToWorldPosition(selectedGridCell);

      SelectionIndicator.gameObject.SetActive(true);
      SelectionIndicator.MoveTowards(dt, selectedPosition);
    } else {
      SelectionIndicator.gameObject.SetActive(false);
    }

    // Tick all light sinks
    foreach (LightSink sink in Board.GetSinks()) {
      sink.BeamStrikesThisFrame = 0;
    }

    // Update the light trees
    // float shotFraction = 1 - (Board.Metronome.TimeTillNextBeat / Board.Metronome.BeatPeriod);
    float shotFraction = 1;

    // Debug.Log(shotFraction);
    UpdateLightTrees(shotFraction);

    // Winning conditions
    if (Board.IsVictory()) {
      StartCoroutine(LevelCompletionSequence());
    }

    // Debug stuff
    if (DebugMode) {
      if (Input.GetKeyDown(KeyCode.Equals)) {
        StartCoroutine(LevelCompletionSequence());
      }
      if (Input.GetKeyDown(KeyCode.Minus)) {
        Destroy(Board.gameObject);
        BoardIndex = (BoardIndex + 1) % Boards.Length;
        Board newBoard = Instantiate(Boards[BoardIndex]);
        StartLevel(newBoard);
      }
      if (Input.GetKeyDown(KeyCode.Tab)) {
        if (Input.GetKey(KeyCode.LeftShift)) {
          debugIndex = -1;
        } else {
          do {
            debugIndex = (debugIndex + 1) % Board.GetPlayObjects().Length;
          } while (!(debugObject.GetComponent<Mirror>() || debugObject.GetComponent<LightSource>() || debugObject.GetComponent<Splitter>()));
          Debug.Log($"Selected {debugObject} at {debugObject.transform.position}");
          DebugDumpLevel("Current state");
        }
      }
      if (Input.GetKeyDown(KeyCode.Comma) && debugIndex >= 0) {
        debugObject.Orientation--;
      }
      if (Input.GetKeyDown(KeyCode.Period) && debugIndex >= 0) {
        debugObject.Orientation++;
      }
    }
  }

  void FixedUpdateActiveBoard(float dt) {
    if (Board.Metronome.Tick(dt)) {
      foreach (var playObject in Board.GetPlayObjects()) {
        playObject.OnQuarterBeat(Board.Metronome.Beats);
      }
    }

    // Smoothly animate the transforms of all play objects based on orientation
    foreach (PlayObject obj in Board.GetPlayObjects()) {
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
    SFXAudioSource.clip = VictoryAudioClip;
    SFXAudioSource.Play();
    SelectionIndicator.gameObject.SetActive(false);

    // hold with lasers drawn and light emitting from sinks
    while (victoryTimer < PauseOnVictoryDuration) {
      UpdateLightTrees(1f - victoryTimer / PauseOnVictoryDuration);
      MusicAudioSource.volume -= Time.deltaTime/PauseOnVictoryDuration*MusicVolume;
      yield return null;
      victoryTimer += Time.deltaTime;
    }
    MusicAudioSource.Stop();

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