using UnityEngine;

namespace _Project.Develop.Runtime.UI.MainScene
{
    public class MainSceneUIRoot : MonoBehaviour
    {
        [field: SerializeField] public Transform HUDLayer { get; private set; }
        [field: SerializeField] public Transform PopupsLayer { get; private set; }
        [field: SerializeField] public Transform VFXUnderPopupsLayer { get; private set; }
        [field: SerializeField] public Transform VFXOVerPopupsLayer { get; private set; }
    }
}