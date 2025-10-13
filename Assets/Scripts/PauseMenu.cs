using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public void GoHome()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
