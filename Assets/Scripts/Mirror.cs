using UnityEngine;

public class Beam {
  public int heading; // 
  public int color; // 0-2

  public Beam(int heading, int color) {
    this.heading = heading;
    this.color = color;
  }
}

public class Mirror : MonoBehaviour {
  // 0-15, 0 is forward, n is n/16th of a revolution around.
  private int orientation {
    get => (int)((transform.eulerAngles.y+.1) * 16f / 360f);
  }

  public Beam[] OnCollide(Beam input) {
    // Reflections for a mirror with orientation "0".
    // We "rotate" the input beam by our orientation, then find the heading
    // outputOrientation = reflectionMap[inputOrientation] for that mirror.
    int inputOrientation = input.heading * 2; // heading is 0-7, orientation is 0-15
    int[] reflectionMap = { 8, 5, 6, 7, -1, 1, 2, 3, 0, 13, 14, 15, -1, 9, 10, 11 };
    int adjustedInput = (inputOrientation + (int)this.orientation) % 16;
    int outputOrientation = reflectionMap[adjustedInput];
    if (outputOrientation != -1) {
      int adjustedOutput = (outputOrientation - (int)this.orientation) % 16;
      return new Beam[] { new Beam(adjustedOutput / 2, input.color) };
    }
    return null;
  }
}