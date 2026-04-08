using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; 
        }
    }


    // ----------------- Cursor ------------------
    public void SetCursor(bool state)
    {
        Cursor.visible = state;
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
    }

    // ----------------- Scene ------------------
    public void ChangeScene(string sceneName)
    {
        IsPaused = false;
        Time.timeScale = 1f;
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("ChangeScene: nombre de escena vacío");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    // ----------------- Quit ------------------
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ----------------- Pause ------------------
    public void GamePause(bool pause)
    {
        IsPaused = pause;
        Time.timeScale = pause ? 0f : 1f;
    }

    // ----------------- Restart ------------------
    public void Restart()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}