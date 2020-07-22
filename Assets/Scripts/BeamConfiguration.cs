using UnityEngine;

[CreateAssetMenu(menuName="Source/BeamConfiguration")]
public class BeamConfiguration : ScriptableObject
{
    [System.Serializable]
    public struct BeamSpec {
        public int HeadingOffset;
        public Color Color;
    }

    public BeamSpec[] BeamSpecs;
}