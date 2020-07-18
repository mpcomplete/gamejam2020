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

  enum GameState { Intro, ActiveBoard, CompletedBoard, Ending }

  [SerializeField] int MAX_LIGHTBEAM_BOUNCES = 6;
  [Header("Audio")]
  [SerializeField] AudioSource MusicAudioSource = null;
  [SerializeField] AudioSource SFXAudioSource = null;
  [SerializeField] AudioClip VictoryAudioClip = null;
  [SerializeField] float MusicVolume = .2f;

  [Header("Boards")]
  [SerializeField] Board Board = null;
  [SerializeField] Board[] Boards = null;
  [SerializeField] int BoardIndex = 0;

  [Header("Scene Objects")]
  [SerializeField] SelectionIndicator SelectionIndicator = null;
  [SerializeField] LineRenderer[] LineRenderers = null;
  [SerializeField] SpaceField SpaceField = null;

  [Header("Gameplay")]
  [SerializeField] float BeamIntensity = 1f;
  [SerializeField] float MaximumBeamWidth = .1f;
  [SerializeField] AnimationCurve BeamWidthForShotFractionCurve = AnimationCurve.Linear(0, 0, 1, 1);
  [SerializeField] float VictoryDuration = 2f;
  [SerializeField] float StartLevelDuration = 1f;
  [SerializeField] float RotationalEpsilon = 1e-5f;
  [SerializeField] float TranslationEpsilon = 1e-5f;
  
  [Header("Camera")]
  [SerializeField] Camera Camera = null;
  [SerializeField] float CameraHeight = 60f;

  int LineRendererIndex = 0;
  GameState State = GameState.Intro;

  IEnumerator Start() {
    foreach (var board in Boards) {
      board.gameObject.SetActive(false);
    }
    yield return StartLevel(Boards[BoardIndex]);
  }

  Vector3 ExponentialLerpTo(Vector3 a, Vector3 b, float epsilon, float dt) {
    return Vector3.Lerp(a, b, 1 - Mathf.Pow(epsilon, dt));
  }

  Quaternion ExponentialSlerpTo(Quaternion a, Quaternion b, float epsilon, float dt) {
    return Quaternion.Slerp(a, b, 1 - Mathf.Pow(epsilon, dt));
  }

  IEnumerator StartLevel(Board board) {
    // TODO: Could fade away with animator state?
    // Disable Selection Indicator
    SelectionIndicator.gameObject.SetActive(false);

    // TODO: Could play outro animation?
    // Swap the active boards
    Board?.gameObject.SetActive(false);
    Board = board;

    // TODO: Could be a cross fade?
    // Turn off old music
    MusicAudioSource.Stop();

    Vector2Int center = Board.Min + (Board.Max - Board.Min) / 2;
    Vector3 targetCenter = Board.GridToWorldPosition(center);
    Vector3 targetCamera = Board.GridToWorldPosition(center) + new Vector3(0, CameraHeight, -CameraHeight);
    Vector3 currentCenter = SpaceField.transform.position;
    Vector3 currentCamera = Camera.transform.position;

    float startTimer = StartLevelDuration;
    bool timeElapsed = false;
    while (!timeElapsed) {
      yield return null;
      float dt = Mathf.Min(startTimer, Time.deltaTime);

      currentCenter = ExponentialLerpTo(currentCenter, targetCenter, TranslationEpsilon, dt);
      currentCamera = ExponentialLerpTo(currentCamera, targetCamera, TranslationEpsilon, dt);
      SpaceField.transform.position = currentCenter;
      Camera.transform.position = currentCamera;
      startTimer -= dt;
      timeElapsed = Mathf.Approximately(startTimer, 0);
    }
    SpaceField.transform.position = targetCenter;
    Camera.transform.position = targetCamera;

    // TODO: Could play intro animation?
    // Activate the new Board
    Board.gameObject.SetActive(true);

    // TODO: Could fade in with animator state?
    // Enable Selection Indicator
    SelectionIndicator.gameObject.SetActive(false);

    // TODO: Could be a cross fade?
    // Turn on new music
    MusicAudioSource.clip = Board.Music;
    MusicAudioSource.volume = MusicVolume;
    MusicAudioSource.Play();

    // Set the gamestate to active
    State = GameState.ActiveBoard;
    yield return null;
  }

  void DebugDumpLevel(string prefix) {
    var tmp1 = String.Join(",", Board.GetComponentsInChildren<LightSource>().Select(o => o.Orientation.ToString()));
    var tmp2 = String.Join(",", Board.GetComponentsInChildren<Mirror>().Select(o => o.Orientation.ToString()));
    var tmp3 = String.Join(",", Board.GetComponentsInChildren<Splitter>().Select(o => o.Orientation.ToString()));
    Debug.Log($"{prefix} level {BoardIndex} Source:{tmp1}, mirrors:{tmp2}, splitters:{tmp3}");
  }

  void RenderLightNode(LightNode node, float width) {
    for (int i = 0; i < node.LightBeams.Count; i++) {
      LightBeam lb = node.LightBeams[i];
      LightNode ln = node.LightNodes[i];
      LineRenderer lr = LineRenderers[LineRendererIndex++];
      Vector3 origin = Board.GridToWorldPosition(node.Position);
      Vector3 destination = Board.GridToWorldPosition(ln.Position);

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
    foreach (LightSink sink in Board.LightSinks) {
      sink.BeamStrikesThisFrame = 0;
    }

    // Update light trees and send collision notifications
    {
      float shotFraction = 1 - (Board.Metronome.TimeTillNextBeat / Board.Metronome.BeatPeriod);
      float beamWidth = MaximumBeamWidth * BeamWidthForShotFractionCurve.Evaluate(shotFraction);
      var noncollided = new HashSet<PlayObject>(Board.GetPlayObjects());
      var collided = new Dictionary<PlayObject, List<LightBeam>>();

      foreach (LightSource source in Board.LightSources) {
        RenderLightNode(Board.MarchLightTree(source, collided, MAX_LIGHTBEAM_BOUNCES), beamWidth);
      }
      foreach (var kv in collided) {
        noncollided.Remove(kv.Key);
        kv.Key.OnCollide(kv.Value);
      }
      foreach (var obj in noncollided)
        obj.OnNoncollide();
    }

    // Update the spacefield
    {
      Star sourceStar = Board.LightSources[0].Star;
      Star sinkStar = Board.LightSinks[0].Star;
      Vector3[] positions = new Vector3[2] { sourceStar.transform.position, sinkStar.transform.position };
      float[] weights = new float[2] { sourceStar.NormalizedMass, sinkStar.NormalizedMass };

      SpaceField.Render(positions, weights);
    }

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
        playObject.OnQuarterBeat(Board.Metronome.QuarterBeats);
      }
    }

    // Smoothly animate the transforms of all play objects based on orientation
    foreach (PlayObject obj in Board.GetPlayObjects()) {
      int orientation = obj.Orientation;
      Vector3 eulerAngles = obj.transform.eulerAngles;
      Quaternion targetRotation = Quaternion.Euler(eulerAngles.x, orientation * 360f / 16f, eulerAngles.z);

      obj.transform.rotation = ExponentialSlerpTo(obj.transform.rotation, targetRotation, RotationalEpsilon, dt);
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
    foreach (var sink in Board.LightSinks) {
      sink.Star.Ignite();
    }
    while (victoryTimer < VictoryDuration) {
      float shotFraction = 1 - (victoryTimer / VictoryDuration);
      float beamWidth = MaximumBeamWidth * BeamWidthForShotFractionCurve.Evaluate(shotFraction);
      var noncollided = new HashSet<PlayObject>(Board.GetPlayObjects());
      var collided = new Dictionary<PlayObject, List<LightBeam>>();

      foreach (LightSource source in Board.LightSources) {
        RenderLightNode(Board.MarchLightTree(source, collided, MAX_LIGHTBEAM_BOUNCES), beamWidth);
      }
      foreach (var kv in collided) {
        noncollided.Remove(kv.Key);
        kv.Key.OnCollide(kv.Value);
      }
      foreach (var obj in noncollided)
        obj.OnNoncollide();

      MusicAudioSource.volume = Mathf.Lerp(MusicVolume, 0, victoryTimer);
      yield return null;
      victoryTimer += Time.deltaTime;
    }

    MusicAudioSource.Stop();

    BoardIndex++;
    if (BoardIndex < Boards.Length) {
      yield return StartLevel(Boards[BoardIndex]);
    } else {
      State = GameState.Ending;
    }
  }

  // Ending State;
  void UpdateEnding(float dt) {
    Debug.Log("It's ovah");
  }

  void FixedUpdateEnding(float dt) {

  }

  void Update() {
    float dt = Time.deltaTime;

    LineRendererIndex = 0;
    switch (State) {
    case GameState.Intro:
      break;
      
    case GameState.ActiveBoard:
      UpdateActiveBoard(dt);
      break;

    case GameState.CompletedBoard:
      break;

    case GameState.Ending:
      UpdateEnding(dt);
      break;
    }

    // TODO: Maybe it's more sane to just... do this by iterating all the stars in the level?
    // Update the spacefield
    if (Board) {
      Vector3[] positions = new Vector3[4];
      float[] normalizedMasses = new float[4];
      int i = 0;
      foreach (var source in Board.LightSources) {
        positions[i] = source.Star.transform.position;
        normalizedMasses[i] = source.Star.NormalizedMass;
        i++;
      }
      foreach (var sink in Board.LightSinks) {
        positions[i] = sink.Star.transform.position;
        normalizedMasses[i] = sink.Star.NormalizedMass;
        i++;
      }
      SpaceField.Render(positions, normalizedMasses);
    }
    DisableUnusedLineRenderers();
  }

  void FixedUpdate() {
    float dt = Time.fixedDeltaTime;

    switch (State) {
    case GameState.Intro:
      break;

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