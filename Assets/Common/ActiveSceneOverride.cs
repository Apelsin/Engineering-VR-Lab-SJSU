using UnityEngine;
using UnityEngine.SceneManagement;

namespace CVRLabSJSU
{
    public class ActiveSceneOverride : MonoBehaviour
    {
        [SerializeField]
        public GameObject[] GameObjectsToActivate = new GameObject[] { };

        private void Awake()
        {
            foreach (var game_object in GameObjectsToActivate)
                game_object.SetActive(false);
        }

        private void Start()
        {
            var active_scene = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(gameObject.scene);
            foreach (var game_object in GameObjectsToActivate)
                game_object.SetActive(true);
            SceneManager.SetActiveScene(active_scene);
        }
    }
}