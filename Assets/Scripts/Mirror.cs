﻿using System.Collections.Generic;
using UnityEngine;

public class Mirror : PlayObject {
  [SerializeField] string AnimatorStateParameterName = "On";
  [SerializeField] Animator Animator = null;

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

      result.Add(new LightBeam(input.Color, adjustedOutput / 2));
    }
    return result;
  }

  public override void OnCollide(List<LightBeam> input) {
    Animator.SetBool(AnimatorStateParameterName, true);
  }
  public override void OnNoncollide() {
    Animator.SetBool(AnimatorStateParameterName, false);
  }

  public override void OnQuarterBeat(int counter) {
    if (counter%4 == 0 || counter%4 == 1)
      Orientation += 1;
  }
}