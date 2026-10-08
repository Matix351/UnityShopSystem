using UnityEngine;
using UnityEngine.UI;

public class PlayerDebugToggleBindings : MonoBehaviour
{
    [SerializeField] private PlayerDebug _playerDebug;
    [SerializeField] private Toggle _hasEnoughMoneyToggle;
    [SerializeField] private Toggle _hasEnoughInventorySpaceToggle;

    private void OnEnable()
    {
        _hasEnoughMoneyToggle.onValueChanged.AddListener(_playerDebug.setEnoughMoney);
        _hasEnoughInventorySpaceToggle.onValueChanged.AddListener(_playerDebug.setEnougInventorySlots);
        _playerDebug.StateChanged += RefreshToggles;
        RefreshToggles();
    }

    private void OnDisable()
    {
        if (_playerDebug == null)
            return;

        if (_hasEnoughMoneyToggle != null)
            _hasEnoughMoneyToggle.onValueChanged.RemoveListener(_playerDebug.setEnoughMoney);
        if (_hasEnoughInventorySpaceToggle != null)
            _hasEnoughInventorySpaceToggle.onValueChanged.RemoveListener(_playerDebug.setEnougInventorySlots);
        _playerDebug.StateChanged -= RefreshToggles;
    }

    private void RefreshToggles()
    {
        _hasEnoughMoneyToggle.SetIsOnWithoutNotify(_playerDebug.HasEnoughMoney);
        _hasEnoughInventorySpaceToggle.SetIsOnWithoutNotify(_playerDebug.HasEnoughInventorySpace);
    }
}
