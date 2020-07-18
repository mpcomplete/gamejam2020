using UnityEngine;

public class Star : MonoBehaviour {
    public enum State { Dormant = 0, GoStar, Star, GoSource, Source, GoDormant }

    [SerializeField] Animator Animator = null;
    [SerializeField] AudioSource IgniteSource = null;
    [SerializeField] AudioSource ExplosionSource = null;
    [SerializeField] string AnimatorStateName = "State";
    [SerializeField] string AnimatorHitName = "Beam Strike";
    public State CurrentState = State.Dormant;
    public float NormalizedMass = .5f;

    public void Update() {
        Animator.SetInteger(AnimatorStateName, (int)CurrentState);
    }

    public void Ignite() {
        CurrentState = State.Star;
    }

    public void SetHit(bool isHit) {
        Animator.SetBool(AnimatorHitName, isHit);
    }

    public void OnIgnite() {
        IgniteSource.Play();
    }

    public void OnExplosion() {
        ExplosionSource.Play();
    }
}