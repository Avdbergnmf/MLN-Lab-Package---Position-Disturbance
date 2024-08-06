using UnityEngine;

namespace MLNLab.Tasks
{
#if UNITY_EDITOR
    [Component("Position Disturbance", "Adds a multisine base noise offset to the transform with parameters that can be tuned. Used for adding disturbances to tracked avatar positions.")]
    [HelpURL("https://hri-wiki.tudelft.nl/")]
    [AddComponentMenu("MLN Lab/Position Noise Disturbance")]
#endif
    [System.Serializable]
    public class PositionNoiseDisturbance : DisturbanceBase
    {
#if UNITY_EDITOR
        [Header("Settings")]
#endif
        [SerializeField] bool pauseNoiseOnGround = true;
        [Tooltip("Scale the noise in different (global) directions. Set an axis to 0 to disable the noise in that direction.")]
        [SerializeField] private Vector3 noiseScale = Vector3.one;
        [SerializeField] public Vector2 freqRange = new Vector2(0.1f, 10f);
        private Vector2 ampRange = new Vector2(0.5f, 1f);
        private Vector2 phaseRange = new Vector2(0f, 2f * Mathf.PI);

        [SerializeField] private float maxDistance = 1.0f;  // Maximum distance from target's 
#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField] private Vector3 offset = Vector3.zero;              // Current noise 

        private float time = 0f;
        private float timeOffset = 0f;

        public Vector3 Offset { get { return offset; } set { } }

        // noise objects
        private MultisineNoise1D xNoise;
        private MultisineNoise1D yNoise;
        private MultisineNoise1D zNoise;

        private void Start()
        {
            Initialize();
        }

        protected override void OnInitialize()
        {
            if (pauseNoiseOnGround)
                CalibrateGroundLevel();

            InitNoise();
            //GetRandomTimeOffset();
            GetNewOffset();
        }

        private void InitNoise()
        {
            int num_waves = 100;
            xNoise = new MultisineNoise1D(num_waves, Mathf.CeilToInt(Random.value * 1000));
            yNoise = new MultisineNoise1D(num_waves, Mathf.CeilToInt(Random.value * 1000));
            zNoise = new MultisineNoise1D(num_waves, Mathf.CeilToInt(Random.value * 1000));

            xNoise.ampRange = ampRange;
            xNoise.freqRange = freqRange;
            xNoise.phaseRange = phaseRange;
            xNoise.InitWaves(false);
            yNoise.ampRange = ampRange;
            yNoise.freqRange = freqRange;
            yNoise.phaseRange = phaseRange;
            yNoise.InitWaves(false);
            zNoise.ampRange = ampRange;
            zNoise.freqRange = freqRange;
            zNoise.phaseRange = phaseRange;
            zNoise.InitWaves(false);

            Debug.Log("Noise waves initialized");
        }

        public void GetRandomTimeOffset()
        {
            float maxTimeOffset = 1000f;
            timeOffset = Random.Range(0f, maxTimeOffset);
        }

        protected override void FixedUpdateTransform() // Only runs if isActive==true, paused when false
        {
            ApplyOffset();

            if (pauseNoiseOnGround)
                if (IsFootGrounded())
                    return;

            time += Time.fixedDeltaTime;
        }

        private void GetNewOffset()
        {
            // Randomize frequency range using Perlin noise
            float offsetTime = time + timeOffset;

            // Update the offset target based on the noise function
            float x = xNoise.GenerateSignal(offsetTime, true);
            float y = yNoise.GenerateSignal(offsetTime, true);
            float z = zNoise.GenerateSignal(offsetTime, true);
            Vector3 noise = new Vector3(x, y, z);

            offset = Vector3.Scale(noise, noiseScale) * maxDistance * Intensity;
        } 

        public void ApplyOffset()
        {
            GetNewOffset();

            // Set position to target's position plus noise offset
            transform.position = leader.transform.position + offset;
        }

        public void ZeroOffset()
        {
            offset = Vector3.zero;
            ApplyOffset();
        }
    }
}