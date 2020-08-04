using UnityEngine;

[CreateAssetMenu(menuName="Ray Material")]
public class RayMaterial : ScriptableObject
{
    public AnimationCurve ReflectionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve TransmissionCurve = AnimationCurve.Constant(0, 1, 0);
}