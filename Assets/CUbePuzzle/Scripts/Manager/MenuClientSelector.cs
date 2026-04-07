using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuClientSelector : MonoBehaviour
{

    public void SelectClient(int id)
    {
        SelectedPlayer.Id = id;
    }

    public void SelectClient0() => SelectClient(0);
    public void SelectClient1() => SelectClient(1);
}