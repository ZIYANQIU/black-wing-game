using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoEndingController : MonoBehaviour
{
    [SerializeField] private GameState gameState;
    [SerializeField] private StoryRunner storyRunner;
    [SerializeField] private GameObject endingPanel;
    [SerializeField] private TMP_Text endingTitleText;

    private void Awake()
    {
        if (endingPanel != null)
            endingPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (storyRunner != null)
            storyRunner.OnStoryEnded += TryShowEnding;
    }

    private void OnDisable()
    {
        if (storyRunner != null)
            storyRunner.OnStoryEnded -= TryShowEnding;
    }

    private void TryShowEnding()
    {
        if (gameState == null || endingPanel == null || endingTitleText == null)
        {
            Debug.LogError("DemoEndingController: Missing references.");
            return;
        }

        string title = null;

        if (gameState.HasFlag("flag_demo_end_up"))
            title = "End 1 · 失途之人";
        else if (gameState.HasFlag("flag_demo_end_down"))
            title = "End 2 · 逃出生天？";
        else if (gameState.HasFlag("flag_demo_end_middle"))
            title = "Demo 演示结束";

        if (title == null)
            return;

        endingTitleText.text = title;
        endingPanel.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
