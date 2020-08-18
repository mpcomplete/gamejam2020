using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MathUtils;

public class Game : MonoBehaviour {
  public static KeyCode[] MovementKeyCodes = new KeyCode[4] {
    KeyCode.UpArrow,
    KeyCode.RightArrow,
    KeyCode.DownArrow,
    KeyCode.LeftArrow
  };

  public static KeyCode[] AlternativeKeyCodes = new KeyCode[4] {
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
  [SerializeField] AudioClip WorldVictoryAudioClip = null;
  [SerializeField] float MusicVolume = .2f;

  [Header("UI")]
  [SerializeField] Animator IntroCardAnimator = null;

  [Header("Boards")]
  [SerializeField] Board Board = null;
  [SerializeField] Board[] Boards = null;
  [SerializeField] int BoardIndex = 0;

  [Header("Scene Objects")]
  [SerializeField] SelectionIndicator SelectionIndicator = null;
  [SerializeField] LineRenderer[] LineRenderers = null;
  [SerializeField] SpaceField SpaceField = null;
  [SerializeField] ConstellationLine[] ConstellationLines = null;

  [Header("Systems")]
  [SerializeField] OrbitRenderingSystem OrbitRenderingSystem = null;
  [SerializeField] RaytracingSystem RaytracingSystem = null;

  [Header("Gameplay")]
  [SerializeField] float BeamIntensity = 1f;
  [SerializeField] float MaximumBeamWidth = .1f;
  [SerializeField] AnimationCurve BeamWidthForShotFractionCurve = AnimationCurve.Linear(0, 0, 1, 1);
  [SerializeField] float VictoryDuration = 2f;
  [SerializeField] float StartLevelDuration = 1f;
  [SerializeField] float RotationalEpsilon = 1e-5f;
  [SerializeField] float TranslationEpsilon = 1e-5f;
  [SerializeField] Vector3 SelectionIndicatorOffset = .5f * Vector3.up;
  
  [Header("Camera")]
  [SerializeField] Camera Camera = null;
  [SerializeField] float CameraHeight = 60f;
  [SerializeField] Transform CameraZoomedOutTransform = null;

  int LineRendererIndex = 0;
  GameState State = GameState.Intro;

  void Start() {
    foreach (var board in Boards) {
      board.gameObject.SetActive(false);
    }
    IntroCardAnimator.gameObject.SetActive(true);
    SpaceField.gameObject.SetActive(false);
    foreach (var line in ConstellationLines) {
      line.gameObject.SetActive(false);
    }
  }

  IEnumerator BeginPlay(Board board) {
    const float TITLE_CARD_FADE_DURATION = 1f;

    IntroCardAnimator.SetBool("Hidden", true);
    yield return new WaitForSeconds(TITLE_CARD_FADE_DURATION);
    yield return StartLevel(board);
    SpaceField.gameObject.SetActive(true);
  }

  IEnumerator LevelCompletionSequence() {
    float victoryTimer = 0f;

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
      foreach (var obj in noncollided) {
        obj.OnNoncollide();
      }

      MusicAudioSource.volume = Mathf.Lerp(MusicVolume, 0, victoryTimer);
      yield return null;
      victoryTimer += Time.deltaTime;
    }

    MusicAudioSource.Stop();

    BoardIndex++;
    if (BoardIndex < Boards.Length) {
      yield return StartLevel(Boards[BoardIndex]);
    } else {
      SFXAudioSource.clip = WorldVictoryAudioClip;
      SFXAudioSource.Play();
      State = GameState.Ending;
    }
  }

  IEnumerator StartLevel(Board board) {
    // TODO: Could fade away with animator state?
    // Disable Selection Indicator
    SelectionIndicator.gameObject.SetActive(false);

    // TODO: Could play outro animation?
    // Swap the active boards
    Board?.gameObject.SetActive(false);
    Board = board;
    
    // Ignite the source if necessary
    foreach (var source in Board.LightSources) {
      source.Star.Ignite();
    }

    // TODO: Could be a cross fade?
    // Turn off old music
    MusicAudioSource.Stop();

    Vector2Int center = Board.Min + (Board.Max - Board.Min) / 2;
    Vector3 targetCenter = GridToWorldPosition(center);
    Vector3 targetCamera = GridToWorldPosition(center) + new Vector3(0, CameraHeight, -CameraHeight);
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

  void RenderLightNode(LightNode node, float width) {
    for (int i = 0; i < node.LightBeams.Count; i++) {
      LightBeam lb = node.LightBeams[i];
      LightNode ln = node.LightNodes[i];
      LineRenderer lr = LineRenderers[LineRendererIndex++];
      Vector3 origin = GridToWorldPosition(node.Position);
      Vector3 destination = GridToWorldPosition(ln.Position);

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
    renderer.material.SetColor("_EmissionColor", beam.Color * BeamIntensity);
    return renderer.material;
  }

  void DisableUnusedLineRenderers() {
    for (int i = LineRendererIndex; i < LineRenderers.Length; i++) {
      LineRenderers[i].gameObject.SetActive(false);
    }
  }

  // Active Board State
  void UpdateActiveBoard(float dt) {
    // Movement handling
    if (Board.SelectedObject) {
      for (int i = 0; i < MovementDirections.Length; i++) {
        if (Input.GetKeyDown(MovementKeyCodes[i]) || Input.GetKeyDown(AlternativeKeyCodes[i])) {
          if (Board.SelectedObject.TryGetComponent(out TargetMover tm)) {
            Vector2Int direction = MovementDirections[i];
            Vector2Int nextCell = tm.TargetCell + direction;
            
            tm.TargetCell = nextCell;
          } else {
            Vector2Int currentPosition = WorldPositionToGrid(Board.SelectedObject.transform.position);
            Vector2Int direction = MovementDirections[i];
            Vector2Int nextCell = currentPosition + direction;

            Board.SelectedObject.transform.position = GridToWorldPosition(nextCell);
          }
        }
      }
    }

    // Reset all light sinks
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
      foreach (var obj in noncollided) {
        obj.OnNoncollide();
      }
    }

    // Winning conditions
    if (Board.IsVictory()) {
      StartCoroutine(LevelCompletionSequence());
    }
  }

  void FixedUpdateActiveBoard(float dt) {
    if (Board.Metronome.Tick(dt)) {
      foreach (var playObject in Board.GetPlayObjects()) {
        playObject.OnQuarterBeat(Board.Metronome.QuarterBeats);
      }

      /*
      GameObject[] orbiters = new GameObject[16];
      int count = Board.ObjectsWith<Orbiter, SmoothMover>(orbiters);

      for (int i = 0; i < count; i++) {
        Orbiter orbiter = orbiters[i].GetComponent<Orbiter>();
        SmoothMover smoothMover = orbiters[i].GetComponent<SmoothMover>();

        if (Board.Metronome.QuarterBeats % orbiter.QuarterBeatsPerOrientation == 0) {
          int newOrientation = (orbiter.Orientation + orbiter.Sign) % Orbiter.UNIQUE_ORIENTATIONS;
          float orbitFraction = (float)newOrientation / Orbiter.UNIQUE_ORIENTATIONS;
          Vector3 localPosition = Orbiter.CalculatePosition(orbiter, orbitFraction);

          orbiter.Orientation = newOrientation;
          smoothMover.TargetPosition = localPosition;
        }
      }
      */
    }


    // Smoothly animate the transforms of all play objects based on orientation
    foreach (PlayObject obj in Board.GetPlayObjects()) {
      int orientation = obj.Orientation;
      Vector3 eulerAngles = obj.transform.eulerAngles;
      Quaternion targetRotation = Quaternion.Euler(eulerAngles.x, orientation * 360f / 16f, eulerAngles.z);

      obj.transform.rotation = ExponentialSlerpTo(obj.transform.rotation, targetRotation, RotationalEpsilon, dt);
    }

    // Smoothly animate all the target mover's
    foreach (TargetMover tm in Board.GetComponentsInChildren<TargetMover>()) {
      tm.transform.position = ExponentialLerpTo(tm.transform.position, GridToWorldPosition(tm.TargetCell), TranslationEpsilon, dt);
    }

    // Smoothly-animate all the smooth mover's
    foreach (SmoothMover sm in Board.GetComponentsInChildren<SmoothMover>()) {
      sm.transform.localPosition = ExponentialLerpTo(sm.transform.localPosition, sm.TargetPosition, sm.LerpEpsilon, dt);
    }

    // Emit rays from all objects
    {
      RaytracingSystem.LightSources = Board.LightSources;       
      RaytracingSystem.LightSourceCount = Board.LightSources.Length;
      RaytracingSystem.Schedule();
    }

    // Selection indicator logic
    if (Board.SelectedObject) {
      if (Board.SelectedObject.TryGetComponent(out TargetMover tm)) {
        Vector2Int selectedGridCell = tm.TargetCell;
        Vector3 selectedPosition = SelectionIndicatorOffset + GridToWorldPosition(selectedGridCell);

        SelectionIndicator.gameObject.SetActive(true);
        SelectionIndicator.transform.position = ExponentialLerpTo(SelectionIndicator.transform.position, selectedPosition, TranslationEpsilon, dt);
      } else {
        Vector2Int selectedGridCell = WorldPositionToGrid(Board.SelectedObject.transform.position);
        Vector3 selectedPosition = SelectionIndicatorOffset + GridToWorldPosition(selectedGridCell);

        SelectionIndicator.gameObject.SetActive(true);
        SelectionIndicator.transform.position = ExponentialLerpTo(SelectionIndicator.transform.position, selectedPosition, TranslationEpsilon, dt);
      }
    } else {
      SelectionIndicator.gameObject.SetActive(false);
    }
  }

  // Intro State
  void UpdateIntro(float dt) {
    if (Input.anyKeyDown) {
      StartCoroutine(BeginPlay(Boards[BoardIndex]));
    }
  }

  // Ending State
  void UpdateEnding(float dt) {
    // TODO: This could probably be handled more cleanly in a outro sequence
    Board.gameObject.SetActive(false);
    SpaceField.gameObject.SetActive(false);
    foreach (var line in ConstellationLines) {
      line.gameObject.SetActive(true);
    }
    Camera.transform.position = ExponentialLerpTo(Camera.transform.position, CameraZoomedOutTransform.position, TranslationEpsilon, dt);
  }

  void FixedUpdateEnding(float dt) {

  }

  void Update() {
    float dt = Time.deltaTime;

    LineRendererIndex = 0;
    switch (State) {
    case GameState.Intro:
      UpdateIntro(dt);
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

    // Render the orbits 
    if (Board) {
      Orbiter[] orbiters = Board.GetComponentsInChildren<Orbiter>();

      OrbitRenderingSystem.Orbiters = orbiters.ToList();
      OrbitRenderingSystem.Count = orbiters.Length;
      // OrbitRenderingSystem.Schedule();
    }

    // Update the spacefield
    if (Board) {
      SpaceField.NormalizedMasses = FindObjectsOfType<NormalizedMass>();
      SpaceField.Count = SpaceField.NormalizedMasses.Length;
      SpaceField.Schedule();
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