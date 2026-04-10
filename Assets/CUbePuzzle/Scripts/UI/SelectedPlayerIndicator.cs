using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SelectedPlayerIndicator : MonoBehaviour
{
    [Tooltip("Image UI que mostrará el sprite según SelectedPlayer.Id.")]
    [SerializeField] private Image targetImage;

    [Tooltip("Sprites indexados por playerId. Index 0 -> jugador 0, index 1 -> jugador 1.")]
    [SerializeField] private Sprite[] sprites = new Sprite[2];

    [Tooltip("Sprite por defecto si no hay sprite para el id.")]
    [SerializeField] private Sprite fallbackSprite;

    private int _lastAppliedId = -1;

    private void Reset()
    {
        if (targetImage == null) targetImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        ApplySpriteForSelectedPlayer();
    }

    private void Start()
    {
        ApplySpriteForSelectedPlayer();
    }

    private void Update()
    {
        if (SelectedPlayer.Id != _lastAppliedId)
            ApplySpriteForSelectedPlayer();
    }
    public void ApplySpriteForSelectedPlayer()
    {
        if (targetImage == null)
            return;

        int id = SelectedPlayer.Id;
        Sprite toSet = null;

        if (sprites != null && id >= 0 && id < sprites.Length)
            toSet = sprites[id];

        if (toSet == null)
            toSet = fallbackSprite;

        targetImage.sprite = toSet;
        targetImage.gameObject.SetActive(toSet != null);

        _lastAppliedId = id;
    }

    public void Refresh() => ApplySpriteForSelectedPlayer();
}
