using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using TMPro;

[System.Serializable]
public class QuestionPayload
{
    public string question;
}

public class AIAssistantHandler : MonoBehaviour
{
    public static AIAssistantHandler Instance; // ✅ 添加单例引用

    public TextMeshProUGUI aiOutputText; // 用于展示 GPT 返回结果

    private void Awake()
    {
        Instance = this;
    }

    private readonly string apiUrl = "http://localhost:5001/gpt_explain"; // 本地 Python 后端接口

    public void GenerateExplanation(string inputQuestion)
    {
        StartCoroutine(CallGPTAPI(inputQuestion));
    }

    IEnumerator CallGPTAPI(string question)
    {
        QuestionPayload postData = new QuestionPayload { question = question };
        string jsonData = JsonUtility.ToJson(postData);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ GPT 返回：" + request.downloadHandler.text);
            OcrButtonHandler.Instance.resultText.text = ParseGPTResponse(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("❌ GPT 请求失败：" + request.error);
            aiOutputText.text = "无法获取 GPT 解释";
        }
    }

    string ParseGPTResponse(string json)
    {
        var node = SimpleJSON.JSON.Parse(json);
        return node["explanation"];
    }

    public void GenerateStepExplanation(string question)
    {
        StartCoroutine(CallGptStepAPI(question));
    }

    IEnumerator CallGptStepAPI(string question)
    {
        QuestionPayload postData = new QuestionPayload { question = question };
        string jsonData = JsonUtility.ToJson(postData);

        UnityWebRequest request = new UnityWebRequest("http://localhost:5001/gpt_explain_step", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ GPT 解题步骤返回：" + request.downloadHandler.text);
            OcrButtonHandler.Instance.resultText.text = ParseGPTResponse(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("❌ GPT 解题步骤请求失败：" + request.error);
        }
    }
}