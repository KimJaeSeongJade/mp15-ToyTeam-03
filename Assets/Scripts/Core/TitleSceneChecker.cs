using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleSceneChecker : MonoBehaviour
{
    public string NextScenename;

    private void Start()
    {
        switch (PrefabShaderWarmup.HasCompleted)
        {
            case true:
                NextScenename = "MainScene";
                break;

            case false:
                NextScenename = "CacheLoadingScene";
                break;
        }

        SceneFade.Instance._destinationScene = NextScenename;
    }

#if UNITY_EDITOR
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }

#endif
}
