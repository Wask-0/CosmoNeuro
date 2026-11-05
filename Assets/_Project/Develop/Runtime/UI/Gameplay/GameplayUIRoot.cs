using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay
{
    public class GameplayUIRoot : MonoBehaviour
    {
        [field: SerializeField] public Transform HUDLayer { get; private set; }

        [field: SerializeField] public Transform PopupsLayer { get; private set; }
        
        [field: SerializeField] public Transform VFXUnderPopupsLayer { get; private set; }
        
        [field: SerializeField] public Transform VFXOVerPopupsLayer { get; private set; }
    }
}