using System.Collections.Generic;
using UnityEngine;

public class Mirror : LightStrikeableBase {
  // 0-15, 0 is forward, n is n/16th of a revolution around.
  public int Orientation {
    get => (int)((transform.eulerAngles.y+.1) * 16f / 360f);
    set {
      Vector3 tmp = transform.eulerAngles;
      tmp.y = (value%16) * 360f / 16f;
      transform.eulerAngles = tmp;
    }
  }

  public override List<LightBeam> ComputeOutgoingLightBeams(LightBeam input) {
    var result = new List<LightBeam>();
    // Reflection map for a mirror with orientation "0".
    // outputOrientation = reflectionMap[inputOrientation] for that 0-oriented mirror.
    // To find reflections for arbitrary headings, we put the heading into the mirror's orientation space,
    // get the reflection, then convert back to global heading.
    int[] reflectionMap = { 8, 7, 6, 5, -1, 3, 2, 1, 0, 15, 14, 13, -1, 11, 10, 9 };
    int inputOrientation = input.Heading * 2; // heading is 0-7, orientation is 0-15
    int adjustedInput = (16 + inputOrientation - (int)this.Orientation) % 16;
    int outputOrientation = reflectionMap[adjustedInput];
    if (outputOrientation != -1) {
      int adjustedOutput = (outputOrientation + (int)this.Orientation) % 16;
      if (Game.DoDebug) {
        Debug.Log($"mirror {Orientation}: {input.Heading} => {adjustedInput} reflects {outputOrientation} => {adjustedOutput} => {adjustedOutput / 2}");
        Game.DoDebug = false;
      }
      result.Add(new LightBeam { Color = input.Color, Heading = adjustedOutput / 2 });
    }
    return result;
  }
}