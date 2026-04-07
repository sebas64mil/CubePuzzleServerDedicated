using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefabLocal;
    [SerializeField] private GameObject playerPrefabRemote;

    [Header("Containers (posiciones para id 0 / id 1)")]
    [SerializeField] private Transform container0;
    [SerializeField] private Transform container1;

    public GameObject LocalPlayer { get; private set; }
    public GameObject RemotePlayer { get; private set; }

    public void Spawn(int localId)
    {
        ClearExisting();

        if (playerPrefabLocal == null || playerPrefabRemote == null)
        {
            Debug.LogError("SpawnManager: prefabs no asignados (local/remote).");
            return;
        }

        if (container0 == null || container1 == null)
        {
            Debug.LogError("SpawnManager: containers no asignados (container0/container1).");
            return;
        }

        // Decide qué prefab y contenedor corresponde al jugador local y al remoto
        Transform localContainer = localId == 0 ? container0 : container1;
        Transform remoteContainer = localId == 0 ? container1 : container0;

        GameObject localPrefab = localId == 0 ? playerPrefabLocal : playerPrefabRemote;
        GameObject remotePrefab = localId == 0 ? playerPrefabRemote : playerPrefabLocal;

        LocalPlayer = Instantiate(localPrefab, localContainer.position, localContainer.rotation, localContainer);
        LocalPlayer.name = "Player_Local";

        RemotePlayer = Instantiate(remotePrefab, remoteContainer.position, remoteContainer.rotation, remoteContainer);
        RemotePlayer.name = "Player_Remote";
    }

    private void ClearExisting()
    {
        if (LocalPlayer != null) Destroy(LocalPlayer);
        if (RemotePlayer != null) Destroy(RemotePlayer);
        LocalPlayer = null;
        RemotePlayer = null;
    }
}