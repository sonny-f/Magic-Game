using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        
    }

    public void startButton()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void quitButton()
    {
        Application.Quit();
    }
}
