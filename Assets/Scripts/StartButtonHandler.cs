using UnityEngine;

public class StartButtonHandler : MonoBehaviour
{
    public GameObject welcomeText;
    public GameObject startButton;
    public GameObject mainPanel;

    public void OnStartButtonClicked()
    {
        Debug.Log("点击了开始按钮！");

        // 隐藏欢迎界面
        welcomeText.SetActive(false);
        startButton.SetActive(false);

        // 显示主功能区
        mainPanel.SetActive(true);
    }
}