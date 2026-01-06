using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SkillcadeSDK.Replays.GUI
{
    public class ReplayWorldControlItem : MonoBehaviour
    {
        [HideInInspector] public int WorldId;
        
        [SerializeField] public TMP_Text WorldNameText;
        [SerializeField] public GameObject ActiveState;
        [SerializeField] public Button SelectButton;
    }
}