using UnityEngine;
public static class GameInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeEngineSettings()
    {
        // 1. Disable VSync to give targetFrameRate priority
        QualitySettings.vSyncCount = 0;
        // 2. Lock game globally to 60 FPS
        Application.targetFrameRate = 60;
        Debug.Log("[GameInitializer] Engine configured: VSync=Off, TargetFPS=60");
    }
}