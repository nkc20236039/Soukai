using UnityEngine;
using UnityEngine.SceneManagement;

public class Replay : MonoBehaviour
{
    public void OnButtonClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
