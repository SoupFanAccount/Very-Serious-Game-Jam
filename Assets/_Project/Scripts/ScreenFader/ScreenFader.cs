using UnityEngine;

public static class ScreenFader
{

    private static ScreenFaderRunner _faderRunner;

    private static void Init()
    {
        if (_faderRunner != null) return;
        
        GameObject faderRunnerGO = new GameObject("FaderRunner");
        _faderRunner = faderRunnerGO.AddComponent<ScreenFaderRunner>();
    }
    
    public static void FadeIn(float duration)
    {
        Init();

        _faderRunner?.FadeIn(duration);
    }

    public static void FadeOut(float duration)
    {
        Init();

        _faderRunner?.FadeOut(duration);
    }
}
