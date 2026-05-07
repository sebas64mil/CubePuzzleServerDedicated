using UnityEngine;
using TMPro;

public class IPMenu : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_InputField ipInput;

    private void Start()
    {
        panel.SetActive(false);

        ipInput.text = ServerConfig.IP;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.P) && Input.GetKeyDown(KeyCode.O))
        {
            panel.SetActive(!panel.activeSelf);
        }
    }

    public void SaveIP()
    {
        ServerConfig.IP = ipInput.text;

        Debug.Log("Nueva IP guardada: " + ServerConfig.IP);
    }
}