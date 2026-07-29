using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class StartButton : MonoBehaviour
    {
        private Button _button;

        void Start()
        {
            _button = GetComponent<Button>();
            
            _button.onClick.AddListener(MenuController.Instance.PlayGame);
        }
    }
}