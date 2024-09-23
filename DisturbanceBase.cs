using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MLNLab.Tasks
{
    public interface IDisturbance
    {
        bool IsActive { get; }

        float Intensity { get; set; } // Represents the intensity of the disturbance
        bool Initialized { get; }

        void Activate();
        void Deactivate();
        void Initialize(bool force);
    }
    
    // General purpose disturbance base class, could be used to write different types of disturbances which are managed under a central disturbance controller (a bit overkill when only using 1 type of disturbance though).
    public abstract class DisturbanceBase : MonoBehaviour, IDisturbance
    {
        [SerializeField] public Transform leader = null;
        [SerializeField] private float intensity = 1f;
        private bool initialized = false;
        [SerializeField] private bool isActive = false;
        public bool IsActive => isActive;
        public float Intensity
        {
            get => intensity;
            set => intensity = Mathf.Clamp(value, 0f, 1f);
        }

        private int seed = 0;
        public int Seed => seed;

        public float groundLevel = 0.05f;
        public float groundMargin = 0.01f;
        public bool IsFootGrounded() => leader.position.y <= groundLevel + groundMargin;

        public bool Initialized => initialized;

        protected virtual void Awake()
        {
            // Automatically register this disturbance with the manager
            DisturbanceManager.Instance.RegisterDisturbance(this);
        }

        protected virtual void OnDestroy()
        {
            // Automatically unregister this disturbance when it's destroyed
            if (DisturbanceManager.Instance != null) // Check to ensure the manager is still around
            {
                DisturbanceManager.Instance.UnregisterDisturbance(this);
            }
        }

        void FixedUpdate()
        {
            if (!isActive || leader == null) return;

            FixedUpdateTransform();
        }

        private void Update()
        {
            if (leader == null) return;

            MatchRotation();
            
            if (!isActive)
            {
                //MatchPosition();
                return;
            }

            UpdateTransform();
        }

        public virtual void InitSeed()
        {
            seed = System.DateTime.Now.GetHashCode();
            Random.InitState(seed);
        }

        public virtual void Activate()
        {
            // Enable the disturbance logic here (if applicable)
            OnActivate();
            isActive = true;
        }

        public virtual void Deactivate()
        {
            // Disable the disturbance logic here (if applicable)
            isActive = false;
            OnDeactivate();
            MatchPosition();
        }

        public void Initialize(bool force = false)
        {
            if (!initialized || force)
            {
                InitSeed();
                OnInitialize();
                initialized = true;
            }
        }

        // This method is intended to be overridden by subclasses for custom initialization logic
        protected virtual void OnInitialize() { } // Can add code here and call it with `base.OnInitialize()` from inheriting class
        protected virtual void FixedUpdateTransform() { }
        protected virtual void OnActivate() { }
        protected virtual void OnDeactivate() { }

        protected virtual void UpdateTransform() { }

        public void MatchPosition()
        {
            if (leader != null)
                transform.position = leader.position;
        }

        public void MatchRotation()
        {
            if (leader != null)
                transform.rotation = leader.rotation;
        }


        #region Calibration 
        // For Foot on ground detection

        public void CalibrateGroundLevel(float recalibrationTime = 1.0f)
        {
            StartCoroutine(RecalibrateGroundMargin(recalibrationTime));
        }

        // Coroutine to recalibrate the ground threshold
        public IEnumerator RecalibrateGroundMargin(float recalibrationTime = 1.0f)
        {
            Debug.Log("Starting groundlevel calibration");

            List<float> yPositions = new List<float>();
            float startTime = Time.time;

            // Collect Y position data for the specified recalibration time
            while (Time.time - startTime < recalibrationTime)
            {

                //Debug.Log(leader.position.y);
                yPositions.Add(leader.position.y);
                yield return null; // Wait for the next frame before continuing the loop
            }

            // Calculate the new ground threshold (mean Y position)
            float sum = 0f;
            foreach (float yPos in yPositions)
            {
                sum += yPos;
            }
            groundLevel = sum / yPositions.Count;

            // Calculate standard deviation in the Y direction
            /*float sumOfSquares = 0f;
            foreach (float yPos in yPositions)
            {
                sumOfSquares += (yPos - groundLevel) * (yPos - groundLevel);
            }
            float variance = sumOfSquares / yPositions.Count;
            float standardDeviation = Mathf.Sqrt(variance);*/

            // Get the max deviation
            float maxHeightDeviation = yPositions.Max();

            // Use standard deviation to set margins (example: groundThreshold  standardDeviation)
            //groundMargin = 0.03f;// 5 * maxHeightDeviation; //standardDeviation;

            Debug.Log($"End groundlevel calibration, ground level: {groundLevel}, ground margin: {groundMargin}");
        }

        #endregion
    }
}
