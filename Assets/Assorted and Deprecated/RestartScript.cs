using UnityEngine;
using UnityEngine.SceneManagement;

namespace CVRLabSJSU
{
    public class RestartScript : MonoBehaviour
    {
        // Use this for initialization
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown("r"))
            {
                SceneManager.LoadScene(0);
            }
        }
    }
}