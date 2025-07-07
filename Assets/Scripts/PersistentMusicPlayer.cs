using UnityEngine;
using UnityEngine.SceneManagement; // Required to access scene management functionalities

public class PersistentMusicPlayer : MonoBehaviour
{
    // This static property creates a Singleton pattern.
    // It ensures that only one instance of this script (and thus, one music player) exists throughout the game.
    public static PersistentMusicPlayer Instance { get; private set; }

    // Use [SerializeField] to make this private array visible and editable in the Unity Inspector.
    // This is where you'll list the names of scenes where the music should NOT play (i.e., pause/stop).
    [SerializeField]
    private string[] scenesToStopMusic;

    private AudioSource audioSource; // Reference to the AudioSource component on this GameObject

    void Awake()
    {
        // --- Singleton Implementation ---
        // Check if an instance of this script already exists.
        if (Instance == null)
        {
            // If no instance exists, this is the first one.
            Instance = this; // Assign this instance as the singleton.
            DontDestroyOnLoad(gameObject); // Crucial: This prevents the GameObject from being destroyed when loading new scenes.

            // Get a reference to the AudioSource component attached to this same GameObject.
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("PersistentMusicPlayer: No AudioSource found on this GameObject. Please add one!");
                return; // Stop execution if no AudioSource is found.
            }

            // Subscribe to the 'sceneLoaded' event.
            // This means the 'OnSceneLoaded' method will be called every time a new scene is loaded.
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Initial check for the first loaded scene
            CheckSceneForMusicPlayback(SceneManager.GetActiveScene().name);

        }
        else
        {
            // If an instance already exists, this means a duplicate 'PersistentMusicPlayer'
            // GameObject was created in a new scene. Destroy the duplicate to maintain the singleton.
            Debug.LogWarning("PersistentMusicPlayer: Duplicate instance found, destroying self.");
            Destroy(gameObject);
        }
    }

    // This method is automatically called by Unity's SceneManager whenever a new scene finishes loading.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Call our custom method to decide whether to play or stop music based on the loaded scene's name.
        CheckSceneForMusicPlayback(scene.name);
    }

    // This method contains the core logic for music playback control.
    void CheckSceneForMusicPlayback(string currentSceneName)
    {
        bool stopMusicInCurrentScene = false;

        // Iterate through the array of scene names where music should be stopped.
        foreach (string sceneNameInBlacklist in scenesToStopMusic)
        {
            // Compare the current scene's name with each name in the blacklist.
            // Use OrdinalIgnoreCase for robust comparison, ignoring case differences.
            if (currentSceneName.Equals(sceneNameInBlacklist, System.StringComparison.OrdinalIgnoreCase))
            {
                stopMusicInCurrentScene = true; // Mark that music should be stopped in this scene.
                break; // Exit the loop early as we've found a match.
            }
        }

        if (stopMusicInCurrentScene)
        {
            // If the current scene is in the blacklist and music is currently playing, pause it.
            // Using Pause() allows you to resume from where it left off later. Use Stop() to reset.
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
                Debug.Log($"Music Paused for scene: {currentSceneName}");
            }
        }
        else
        {
            // If the current scene is NOT in the blacklist and music is currently NOT playing, play it.
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
                Debug.Log($"Music Playing for scene: {currentSceneName}");
            }
        }
    }

    // This method is called when the GameObject or the application is destroyed.
    // It's crucial to unsubscribe from events to prevent memory leaks, especially for persistent objects.
    void OnDestroy()
    {
        // Only unsubscribe if this instance was the actual singleton.
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded; // Remove the listener for the sceneLoaded event.
        }
    }

    // --- Optional Public Methods for External Control ---
    // These methods allow other scripts to manually control the background music if needed.

    public void StopBackgroundMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("Background music stopped manually.");
        }
    }

    public void PlayBackgroundMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
            Debug.Log("Background music playing manually.");
        }
    }

    public void PauseBackgroundMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log("Background music paused manually.");
        }
    }

    public void UnpauseBackgroundMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.UnPause();
            Debug.Log("Background music unpaused manually.");
        }
    }

    public bool IsMusicPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }
}