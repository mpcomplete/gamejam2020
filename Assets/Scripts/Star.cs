using System.Collections.Generic;
using UnityEngine;

public class Star : PlayObject {
    public enum State { Dormant = 0, GoStar, Star, GoSource, Source, GoDormant }
    public int BeamStrikesThisFrame = 0;

    [SerializeField] Animator Animator;
    [SerializeField] string AnimatorStateName = "State";
    public State CurrentState = State.Dormant;
    public AudioSource IgniteSource;
    public AudioSource ExplosionSource;

    public void Update() {
        Animator.SetInteger(AnimatorStateName, (int)CurrentState);
    }

    public override void OnCollide(List<LightBeam> input) {
        BeamStrikesThisFrame += input.Count;
    }

    public override void OnNoncollide() {
        BeamStrikesThisFrame = 0;
    }

    public void OnIgnite() {
        IgniteSource.Play();
    }

    public void OnExplosion() {
        ExplosionSource.Play();
    }
}