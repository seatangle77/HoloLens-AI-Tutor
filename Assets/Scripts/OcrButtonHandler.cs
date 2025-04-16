using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;

using SimpleJSON;  // ✅ 添加这一行

public class OcrButtonHandler : MonoBehaviour
{
    public static OcrButtonHandler Instance; // 新增字段
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI statusText; // 新增字段
    public GameObject resultTextPanel; // 新增字段
    public GameObject nextButton; // 新增字段
    public GameObject nextStepButton; // 新增字段

    [System.Serializable]
    public class OCRItem
    {
        public string text;
        public float[][] box;
    }

    [System.Serializable]
    public class OCRResponse
    {
        public OCRItem[] results;
    }

    [System.Serializable]
    public class OCRWrapper
    {
        public OCRItem[] results;
    }

    void Start()
    {
        Instance = this; // 新增代码
        // 隐藏初始文字
        resultText.gameObject.SetActive(false);
        if (statusText != null)
            statusText.gameObject.SetActive(false); // 新增代码
    }

    public void OnOcrButtonClicked()
    {
        Debug.Log("点击识别按钮");

        // 删除所有旧的识别结果文本
        foreach (var go in GameObject.FindGameObjectsWithTag("OCRText"))
        {
            Destroy(go);
        }

        resultText.gameObject.SetActive(true);
        if (statusText != null)
        {
            statusText.gameObject.SetActive(true); // 新增代码
            statusText.text = "<color=orange>识别中，请稍等...</color>"; // 新增代码
        }

        // 从 Resources 文件夹加载模拟图片
        Texture2D testImage = Resources.Load<Texture2D>("Images/test3");

        if (testImage != null)
        {
            StartCoroutine(CallOCRAPI(testImage));
        }
        else
        {
            resultText.text = "<color=red>未找到测试图片 test3.png！</color>";
        }

    }

    IEnumerator CallOCRAPI(Texture2D image)
    {
        byte[] imageBytes = image.EncodeToPNG();

        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageBytes, "screenshot.png", "image/png");

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:5001/ocr_paddle", form))
        {
            Debug.Log("📤 准备发送 OCR 请求到：" + www.url);
            Debug.Log("📤 111：" + www.result);

            var request = www.SendWebRequest();
            request.completed += (asyncOp) =>
            {
                Debug.Log("📡 UnityWebRequest 状态: " + www.result);
                Debug.Log("📩 Response Code: " + www.responseCode);
                Debug.Log("📨 Downloaded Text: " + www.downloadHandler.text);
                Debug.Log("🐞 错误内容: " + www.error);

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string json = www.downloadHandler.text;
                    Debug.Log("📦 OCR 接口原始返回 JSON：" + json);

                    if (string.IsNullOrEmpty(json))
                    {
                        Debug.LogError("❌ OCR 返回为空！");
                        resultText.text = "<color=red>服务器返回为空！</color>";
                        return;
                    }

                    List<OCRItem> items = new List<OCRItem>();
                    try
                    {
                        var parsed = JSON.Parse(json);
                        Debug.Log("✅ JSON 解析成功: " + parsed.ToString());

                        var results = parsed["results"];
                        Debug.Log("🧩 提取 results 成功: " + results.ToString());

                        foreach (var entry in results.AsArray)
                        {
                            Debug.Log("🔍 单个 entry: " + entry.ToString());

                            string text = entry.Value["text"];
                            var boxArray = entry.Value["box"].AsArray;

                            float[][] box = new float[boxArray.Count][];
                            for (int i = 0; i < boxArray.Count; i++)
                            {
                                var point = boxArray[i].AsArray;
                                box[i] = new float[] { point[0].AsFloat, point[1].AsFloat };
                            }

                            Debug.Log("✅ 添加识别项: " + text);
                            items.Add(new OCRItem { text = text, box = box });
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("❌ JSON 解析失败: " + e.Message);
                    }

                    if (items.Count == 0)
                    {
                        Debug.LogWarning("⚠️ 未识别到任何 OCR 项目");
                        resultText.text = "<color=red>未识别到文字内容</color>";
                    }
                    else
                    {
                        string combined = "";
                        foreach (var item in items)
                        {
                            Debug.Log("✅ 识别文字：" + item.text);
                            combined += item.text + "\n";
                        }
                        resultText.text = combined; // 修改代码

                        if (statusText != null)
                        {
                            statusText.gameObject.SetActive(true); // 新增代码
                            statusText.text = "<color=green>识别完成！</color>"; // 修改代码
                        }

                        if (nextButton != null) nextButton.SetActive(true);
                        if (resultTextPanel != null) resultTextPanel.SetActive(true);
                        if (resultText != null) resultText.gameObject.SetActive(true);

                        GameObject ocrButton = GameObject.Find("OCRButton");
                        if (ocrButton != null) ocrButton.SetActive(false);
                    }
                }
                else
                {
                    resultText.text = "<color=red> 识别失败：</color>\n" + www.error;
                }
            };

            // 为防止协程提前结束，增加等待标志
            yield return new WaitUntil(() => request.isDone);
        }
    }

    public void OnNextButtonClicked()
    {
        if (statusText != null)
            statusText.text = "";
        if (nextButton != null)
            nextButton.SetActive(false);

        if (AIAssistantHandler.Instance != null && resultText != null)
        {
            AIAssistantHandler.Instance.GenerateExplanation(resultText.text);
            if (nextStepButton != null)
                nextStepButton.SetActive(true);
        }
    }

    public void OnNextStepButtonClicked()
    {
        if (AIAssistantHandler.Instance != null && resultText != null)
        {
            AIAssistantHandler.Instance.GenerateStepExplanation(resultText.text);
            nextStepButton.SetActive(false);

        }
    }

    IEnumerator HideResultAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        resultText.gameObject.SetActive(false);
    }
}