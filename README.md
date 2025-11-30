# Technical Commentary: Procedural Height Map Generation

## System Overview
The **Height Map Generator** is a custom tool integrated directly into the Unity Editor, designed to facilitate the creation of procedural height maps for terrain generation and texturing. By extending Unity's `EditorWindow` class, the system provides a dedicated graphical user interface (GUI) that allows developers to manipulate generation parameters in real-time and visualize the results instantly before saving them as assets. This integration streamlines the workflow, enabling rapid iteration without the need to enter Play Mode.

## Tools and Technologies
The core of the system is built using **C#** within the **Unity Engine** ecosystem. Key APIs and libraries include:

*   **UnityEditor API:** Used to create the custom window (`EditorWindow`), define menu items (`[MenuItem]`), and render UI controls (`EditorGUILayout`, `GUILayout`). This allows for a native "look and feel" and seamless integration with the editor environment.
*   **UnityEngine API:** Provides essential math functions and data structures, most notably `Mathf.PerlinNoise` for noise generation and `Texture2D` for visualizing and storing the generated data.
*   **System.IO:** Utilized for file management operations, specifically to handle the saving of generated textures as PNG files to the project's asset database.

## Algorithmic Implementation
The generation logic relies on **Perlin Noise**, a gradient noise function widely used in computer graphics to simulate natural phenomena. However, raw Perlin noise is often too smooth and lacks the detail required for realistic terrain. To address this, the system implements **Fractal Brownian Motion (fBm)**, a technique that layers multiple passes of noise (octaves) to build complexity.

The algorithm iterates through each pixel of the target resolution (defined by `width` and `height`). For every pixel, it calculates a noise value by summing contributions from multiple octaves. The specific characteristics of this summation are controlled by:
*   **Octaves:** The number of noise layers. More octaves add finer detail.
*   **Persistence:** Determines how much the amplitude (height influence) decreases with each subsequent octave. A value of 0.5 implies each layer has half the impact of the previous one.
*   **Lacunarity:** Determines how much the frequency (detail density) increases with each octave. A value of 2.0 means each layer is twice as detailed as the previous one.
*   **Scale:** A global zoom factor that determines the overall size of the noise features.

The resulting noise values are normalized to a 0-1 range using `Mathf.InverseLerp` based on the global min and max noise heights found during generation. This ensures the height map utilizes the full dynamic range of the texture.

## Procedural and Generative Aspects
The system is fundamentally **procedural** because the content is generated algorithmically rather than being manually painted or captured. It is **generative** in that it can produce an infinite variety of unique outputs from a compact set of rules and parameters.

The **Seed** parameter is the cornerstone of this generative capability. By initializing a pseudo-random number generator (`System.Random`) with a specific seed, the system creates unique offsets for each octave. This means that changing the seed results in a completely different terrain landscape, while keeping the same seed guarantees reproducibility—a critical feature for development pipelines where consistent results are often needed alongside random variation.

Finally, the normalized noise map is converted into a visual representation by mapping the 0-1 values to a grayscale gradient (`Color.Lerp(Color.black, Color.white, value)`). This visual feedback loop allows the user to intuitively understand the "terrain" they are creating, where white represents peaks and black represents valleys, effectively bridging the gap between abstract mathematical noise and tangible game assets.

## System In Action
https://youtu.be/UxqiNQwUIVs
