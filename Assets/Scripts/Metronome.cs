[System.Serializable]
public class Metronome
{
    public int Beats = 0;
    public float TimeTillNextBeat = 0;
    public float BeatPeriod = .5f;

    public bool Tick(float dt) {
        const int MAX_CYCLES = 1000; // to prevent annoying infinite loops

        int cycles = 0;
        bool ticked = false;

        if (BeatPeriod <= 0) {
            return false;
        }

        while (dt > TimeTillNextBeat && cycles < MAX_CYCLES) {
            ticked = true;
            Beats++;
            cycles++;
            dt -= TimeTillNextBeat;
            TimeTillNextBeat = BeatPeriod;
        }
        TimeTillNextBeat -= dt;
        return ticked;
    }
}