using UnityEngine;

public class CourseManager : MonoBehaviour
{
    public CourseData courseData;
    public Transform courseRoot;
    public Transform player;

    void Start()
    {
        BuildCourse();
    }

    public void BuildCourse()
    {
        if (courseData == null || courseRoot == null)
        {
            Debug.LogError("CourseManager: courseData / courseRoot が未設定");
            return;
        }

        // 既存クローン掃除（テスト時に便利）
        for (int i = courseRoot.childCount - 1; i >= 0; i--)
            Destroy(courseRoot.GetChild(i).gameObject);

        Vector3 cursor = Vector3.zero;

        foreach (var prefab in courseData.sections)
        {
            if (prefab == null) continue;

            // いったん cursor に仮置き
            var section = Instantiate(prefab, cursor, Quaternion.identity, courseRoot);

            var start = section.transform.Find("StartAnchor");
            var end   = section.transform.Find("EndAnchor");

            if (start == null || end == null)
            {
                Debug.LogError($"Section {prefab.name} に StartAnchor / EndAnchor が無い");
                continue;
            }

            // ★重要：StartAnchor の localPosition を基準に、StartAnchor が cursor に来るようにずらす
            // start.localPosition は “セクション内の相対位置” なので安定する
            section.transform.position = cursor - start.localPosition;

            // ★次の cursor：セクションを動かした後の EndAnchor のワールド座標
            cursor = end.position;
        }

        if (player != null)
            player.position = courseData.playerStartOffset;
    }
}