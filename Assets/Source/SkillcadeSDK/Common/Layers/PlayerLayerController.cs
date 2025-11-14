using UnityEngine;
using VContainer;

namespace SkillcadeSDK.Common
{
    public class PlayerLayerController : MonoBehaviour
    {
        [Inject] private readonly LayerProvider _layerProvider;
        
        private int _defaultLayer;
        private int? _layer;

        private void Awake()
        {
            _defaultLayer = gameObject.layer;
        }

        private void OnEnable()
        {
            this.InjectToMe();
            
            _layerProvider.CollisionsStateChanged += OnCollisionStateChanged;
            if (_layerProvider.CollisionsEnabled)
                GetLayer();
        }

        private void OnDisable()
        {
            _layerProvider.CollisionsStateChanged -= OnCollisionStateChanged;
            ReturnLayer();
        }

        private void OnCollisionStateChanged()
        {
            ReturnLayer();
            if (_layerProvider.CollisionsEnabled)
                GetLayer();
        }
        
        private void GetLayer()
        {
            if (_layerProvider.TryGetLayer(out int layer))
            {
                gameObject.SetLayerWithChildren(layer);
                _layer = layer;
            }
        }
        
        private void ReturnLayer()
        {
            if (_layer.HasValue)
            {
                _layerProvider.ReturnLayer(_layer.Value);
                _layer = null;
                gameObject.SetLayerWithChildren(_defaultLayer);
            }
        }
    }
}