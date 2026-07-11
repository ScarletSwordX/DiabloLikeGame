using Gameplay.DebugTest;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.FeedbackUI
{
    [RequireComponent(typeof(Button))]
    public class GameplaySelfTestButton : MonoBehaviour
    {
        [SerializeField] GameplaySelfTestRunner _runner;

        public void AssignRunner(GameplaySelfTestRunner runner) => _runner = runner;

        void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() => _runner?.RunAll());
        }
    }
}
