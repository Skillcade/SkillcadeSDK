using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

namespace SkillcadeSDK.Replays.GUI
{
    public class ReplayWorldControlPanel : MonoBehaviour
    {
        [SerializeField] private ReplayWorldControlItem _itemPrefab;
        [SerializeField] private Transform _itemsParent;

        [Inject] private readonly ReplayReadService _replayReadService;
        
        private List<ReplayWorldControlItem> _items;

        private void OnEnable()
        {
            if (_replayReadService == null || !_replayReadService.IsReplayReady)
                return;
            
            if (_items != null)
                return;

            _items = new List<ReplayWorldControlItem>();
            var orderedWorlds = _replayReadService.ClientWorlds.OrderBy(x => x.Key);
            foreach (var clientWorld in orderedWorlds)
            {
                int worldId = clientWorld.Key;
                var item = Instantiate(_itemPrefab, _itemsParent);
                item.WorldId = worldId;
                item.ActiveState.SetActive(worldId == _replayReadService.CurrentActiveWorldId);
                item.WorldNameText.text = clientWorld.Key == ReplayReadService.ServerWorldId
                    ? "Server View"
                    : $"Player_{worldId} View";
                item.SelectButton.onClick.AddListener(() => SetActiveWorld(worldId));
                _items.Add(item);
            }
        }

        private void SetActiveWorld(int worldId)
        {
            _replayReadService.SetActiveWorld(worldId);
            foreach (var item in _items)
            {
                item.ActiveState.SetActive(item.WorldId == worldId);
            }
        }
    }
}