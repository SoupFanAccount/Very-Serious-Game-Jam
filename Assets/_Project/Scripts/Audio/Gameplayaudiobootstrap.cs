using UnityEngine;

// Put this on an object in the gameplay (Main) scene.
// AudioManager persists from the menu via DontDestroyOnLoad, so this
// just tells it to switch to the shop track once gameplay starts.
public class GameplayAudioBootstrap : MonoBehaviour
{
    private void Start()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayShopMusic();
    }
}