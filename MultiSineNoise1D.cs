using UnityEngine;

// Simple class to generate a multisine signal in 1D. Customize numWaves, frequence, amplitude, and phase ranges to adapt the signal to the requirements. 
//   In general, more waves, with broader ranged result in less predictable signals.
//   Can normalize signal based on estimated variance. Won't fully contain signal within 0-1 range, so also has a clamping function to ensure controllability.
public class MultisineNoise1D
{
    public int numWaves = 1;
    public Vector2 freqRange = new Vector2(0.1f, 10f);
    public Vector2 ampRange = new Vector2(0.5f, 1f);
    public Vector2 phaseRange = new Vector2(0f, 2f * Mathf.PI);
    public int seed = 2023;

    private float[] frequencies;
    private float[] amplitudes;
    private float[] phases;

    private float sdCalc = 1f;
    public float gain = 1f;

    // Constructor
    public MultisineNoise1D(int numWaves = 1, int seed = 2023, float gain = 1f)
    {
        this.seed = seed;
        this.numWaves = numWaves;
        this.gain = gain;

        // Initialize arrays to a fixed size (example: 200, adjust based on your needs)
        frequencies = new float[200];
        amplitudes = new float[200];
        phases = new float[200];

        InitWaves(false);
    }

    public void InitWaves(bool randomFreq)
    {
        int maxNumWaves = frequencies.Length;
        Random.InitState(seed);

        int numWavesToUse = Mathf.Min(numWaves, maxNumWaves);

        if (randomFreq)
        {
            for (int i = 0; i < numWavesToUse; i++)
            {
                frequencies[i] = Random.Range(freqRange.x, freqRange.y);
            }
        }
        else
        {
            float step = (freqRange.y - freqRange.x) / (numWavesToUse - 1);
            for (int i = 0; i < numWavesToUse; i++)
            {
                frequencies[i] = freqRange.x + step * i;
            }
        }

        for (int i = 0; i < numWavesToUse; i++)
        {
            amplitudes[i] = Random.Range(ampRange.x, ampRange.y);
            phases[i] = Random.Range(phaseRange.x, phaseRange.y);
        }

        // Calculation of the estimated standard deviation
        float totalVariance = 0f;
        for (int i = 0; i < numWavesToUse; i++)
        {
            float variance = amplitudes[i] * amplitudes[i] / 2;
            totalVariance += variance;
        }
        sdCalc = Mathf.Sqrt(totalVariance);
    }

    public float GenerateSignal(float time, bool normalizeOutput = true)
    {
        float[] timeArr = new float[1];
        timeArr[0] = time;
        return GenerateSignal(timeArr, normalizeOutput)[0];
    }

    public float[] GenerateSignal(float[] time, bool normalizeOutput = true)
    {
        float[] signal = new float[time.Length];

        for (int t = 0; t < time.Length; t++)
        {
            for (int i = 0; i < numWaves; i++)
            {
                signal[t] += amplitudes[i] * Mathf.Sin(2 * Mathf.PI * frequencies[i] * time[t] + phases[i]);
            }

            if (normalizeOutput && sdCalc != 0)
            {
                signal[t] = (signal[t]) / (2 * sdCalc);

                signal[t] = Mathf.Clamp(signal[t], -1, 1); // Make sure it doesn't exceed 1.
            }

            signal[t] *= gain;
        }

        return signal;
    }
}
