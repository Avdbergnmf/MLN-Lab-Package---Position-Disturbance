# Position Noise Disturbance for Unity

This repository contains the C# classes used to introduce a multisine-based noise disturbance to a game object's position. The script was developed for the experiment titled: _"Visual Disturbances to Leg Position to Increase Step-Width Variability in Immersive VR Gait Rehabilitation"_. In this experiment, this noise generation was used to induce a visual feedback distortion (VFD) in tracked avatar positions. Tested and used on `Unity version 2020.3`.

For more information visit our website: [mlnlab.nl](https://www.mlnlab.nl), or our [wiki page](https://hri-wiki.tudelft.nl/).

## License and usage
This project is licensed under the MIT License.

If used, please adequately cite this work:
```
@inproceedings{WIP,
    author = {Alex van den Berg},
    title = {PositionNoiseDisturbance},
    year = {2024},
    publisher = {WIP},
    address = {Delft, Zuid-Holland, The Netherlands},
    url = {WIP},
    doi = {WIP}
}
```

## Features

- **Multisine noise generation:** Adds customizable noise disturbances in three dimensions (x, y, z).
- **Customizable settings:** Control frequency, amplitude, and phase ranges to fine-tune noise generation.
- **Real-time noise application:** Dynamically applies positional noise during runtime.
- **Calibration:** Includes calibration functionality for detecting ground levels to stop noise when on the ground.
  
## Usage

1. Add the `PositionNoiseDisturbance` script as a component to a GameObject in your Unity project.
2. Configure the noise settings through the Unity Inspector:
   - `Noise Scale`: Adjust noise intensity per axis.
   - `Frequency Range`: Set the range of frequencies for the multisine noise.
   - `Amplitude Range`: Set the range of amplitudes for the noise.
   - `Pause Noise on Ground`: Enable/disable noise when the object is grounded.
3. Assign the target object (leader) whose position will be disturbed by noise. For example a tracked VIVE tracker position (in our paper, the ones attached to the feet).
4. Assign the GameObject with the `PositionNoiseDisturbance` component the tracked object for the virtual avatar (for example, using [FinalIK](http://root-motion.com/)).

## Example

Here is an example of adding noise disturbance to a tracked object:

```csharp
using MLNLab.Tasks;

public class DisturbedObject : MonoBehaviour
{
    [SerializeField] private Transform TrackedObject = null;

    void Start()
    {
        PositionNoiseDisturbance disturbance = gameObject.AddComponent<PositionNoiseDisturbance>();

        if (TrackedObject != null)
            disturbance.leader = trackedObject.transform;
        else
            Debug.LogWarning("TrackedObject not assigned.");

        disturbance.Initialize();
        disturbance.Activate();
    }
}
```
