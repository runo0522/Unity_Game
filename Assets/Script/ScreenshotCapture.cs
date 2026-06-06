using UnityEngine;
using System.IO;

public class ScreenshotCapture : MonoBehaviour
{
    void Update()
    {
        // Pキーを押したらスクショを保存
        if (Input.GetKeyDown(KeyCode.P))
        {
            string folderPath = Path.Combine(Application.dataPath, "../Screenshots");

            // Screenshotsフォルダがなければ自動で作成
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName =
                "Screenshot_" +
                System.DateTime.Now.ToString("yyyyMMdd_HHmmss") +
                ".png";

            string filePath = Path.Combine(folderPath, fileName);

            ScreenCapture.CaptureScreenshot(filePath);

            Debug.Log("スクリーンショットを保存しました: " + filePath);
        }
    }
}