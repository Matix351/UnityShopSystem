using UnityEngine;
using UnityEngine.UI;

public class PlayerDebugToggleBindings : MonoBehaviour
{
    [SerializeField] private PlayerDebug _playerDebug;
    [SerializeField] private Toggle _hasEnoughMoneyToggle;
    [SerializeField] private Toggle _hasEnoughInventorySpaceToggle;

    private void OnEnable()
    {
        _hasEnoughMoneyToggle.onValueChanged.AddListener(_playerDebug.SetEnoughMoney);
        _hasEnoughInventorySpaceToggle.onValueChanged.AddListener(_playerDebug.SetEnougInventorySlots);
        _playerDebug.StateChanged += refreshToggles;
        refreshToggles();
    }

    private void OnDisable()
    {
        if (_playerDebug == null)
            return;

        if (_hasEnoughMoneyToggle != null)
            _hasEnoughMoneyToggle.onValueChanged.RemoveListener(_playerDebug.SetEnoughMoney);
        if (_hasEnoughInventorySpaceToggle != null)
            _hasEnoughInventorySpaceToggle.onValueChanged.RemoveListener(_playerDebug.SetEnougInventorySlots);
        _playerDebug.StateChanged -= refreshToggles;
    }

    private void refreshToggles()
    {
        _hasEnoughMoneyToggle.SetIsOnWithoutNotify(_playerDebug.HasEnoughMoney);
        _hasEnoughInventorySpaceToggle.SetIsOnWithoutNotify(_playerDebug.HasEnoughInventorySpace);
    }
}
