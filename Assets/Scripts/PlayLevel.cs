using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeatWalker
{
    public class NextLevel : MonoBehaviour
    {
        [SerializeField]
        int nextScene;


        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                SceneManager.LoadScene(nextScene);
            }
        }

        public void ButtonTransition() 
        {
            SceneManager.LoadScene(nextScene);
        }

        public void Quit()
        {
            Debug.Log("Quitting Game ;-;");
            Application.Quit();
        }
    }
}
