[System.Serializable]
public class Metronome
{
    public int Beats = 0;
    public float TimeTillNextBeat = 0;
    public float BeatPeriod = .5f;

    public int QuarterBeats = 0;
    public float TimeTillNextQuarterBeat = 0;

    public bool Tick(float dt) {
        const int MAX_CYCLES = 1000; // to prevent annoying infinite loops

        float quarterBeatPeriod = BeatPeriod / 4f;
        int cycles = 0;
        bool ticked = false;

        if (BeatPeriod <= 0) {
            return false;
        }

        while (dt > TimeTillNextQuarterBeat && cycles < MAX_CYCLES) {
            ticked = true;
            QuarterBeats++;
            cycles++;
            dt -= TimeTillNextQuarterBeat;
            TimeTillNextQuarterBeat = quarterBeatPeriod;
            if (QuarterBeats % 4 == 0) {
                Beats++;
                TimeTillNextBeat = BeatPeriod;
            }
        }
        TimeTillNextBeat -= dt;
        TimeTillNextQuarterBeat -= dt;
        return ticked;
    }
}