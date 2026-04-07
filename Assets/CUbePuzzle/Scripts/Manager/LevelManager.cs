using UnityEngine;

public class LevelManager : MonoBehaviour
{
    // ----------------- Change Scene ------------------
    public void ChangeScene(string sceneName)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager not initialized");
            return;
        }

        GameManager.Instance.ChangeScene(sceneName);
    }

    // ----------------- Quit Game ------------------
    public void QuitGame()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager not initialized");
            return;
        }

        GameManager.Instance.QuitGame();
    }

    // ----------------- Pause ------------------
    public void PauseGame()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.GamePause(true);
    }

    // ----------------- Resume ------------------
    public void ResumeGame()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.GamePause(false);
    }

    // ----------------- Toggle Pause ------------------
    public void TogglePause()
    {
        if (GameManager.Instance == null) return;

        bool newState = !GameManager.Instance.IsPaused;
        GameManager.Instance.GamePause(newState);
    }

    // ----------------- Restart ------------------
    public void RestartLevel()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.Restart();
    }

    // ----------------- Cursor ------------------
    public void ShowCursor()
    {
        GameManager.Instance?.SetCursor(true);
    }

    public void HideCursor()
    {
        GameManager.Instance?.SetCursor(false);
    }
}