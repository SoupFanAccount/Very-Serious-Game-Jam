using System;
using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class TransitionSystem : MonoBehaviour
{
    public static TransitionSystem Instance { get; private set; }

    private Camera _mainCam;

    private void Awake()
    {
        Instance = this;
        _mainCam = Camera.main;
    }

    public void DoTransition(Action<Door> action , Door door , float fadeIn = .3f , float fadeOut = .5f)
    {
        StartCoroutine(TransitionCoroutine(action, door, fadeIn, fadeOut));
    }

    private IEnumerator TransitionCoroutine(Action<Door> action, Door door, float fadeIn = .3f, float fadeOut = .5f)
    {
        ScreenFader.FadeIn(fadeIn);

        yield return new WaitForSecondsRealtime(fadeIn);

        SwitchCamera(door);
        
        action?.Invoke(door);
        ScreenFader.FadeOut(fadeOut);
    }

    private void SwitchCamera(Door door)
    {
        if (door.CameraToEnable() == null) return;
        if(_mainCam.TryGetComponent(out CinemachineBrain brain) == false) Debug.LogError("THere is No CinemachineBrain in Main Cam!");
        
        CinemachineBlendDefinition blend = new CinemachineBlendDefinition();
        blend.Style = CinemachineBlendDefinition.Styles.Cut;

        brain.DefaultBlend = blend;
        
        CinemachineCamera currentCam = brain.ActiveVirtualCamera as CinemachineCamera;

        currentCam?.gameObject.SetActive(false);
        door.CameraToEnable().gameObject.SetActive(true);
    }
}
