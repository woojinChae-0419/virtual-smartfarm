using UnityEngine;

public class DashboardUI : MonoBehaviour
{
    private SensorSimulator simulator;
    private PlantStateController stateController;
    private APIClient apiClient;
    private float startTime;
    private Texture2D bgTex;

    void Awake()
    {
        if (GetComponent<EventLogger>() == null) gameObject.AddComponent<EventLogger>();
        if (GetComponent<PlantResponse>() == null) gameObject.AddComponent<PlantResponse>();
    }

    void Start()
    {
        simulator = GetComponent<SensorSimulator>();
        stateController = GetComponent<PlantStateController>();
        apiClient = GetComponent<APIClient>();
        startTime = Time.time;
        bgTex = new Texture2D(1, 1);
        bgTex.SetPixel(0, 0, new Color(0.05f, 0.08f, 0.1f, 0.88f));
        bgTex.Apply();
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = 14;
        float w = 370f, h = 520f, pad = 15f;
        Rect panel = new Rect(Screen.width - w - pad, pad, w, h);
        GUI.DrawTexture(panel, bgTex);

        GUILayout.BeginArea(new Rect(panel.x + 18, panel.y + 15, w - 36, h - 30));

        GUI.contentColor = new Color(0.6f, 1f, 0.6f);
        GUIStyle title = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
        GUILayout.Label("Virtual Smart Farm", title);

        GUI.contentColor = new Color(0.7f, 0.7f, 0.7f);
        float elapsed = Time.time - startTime;
        GUILayout.Label($"경과 {(int)(elapsed/60):D2}:{(int)(elapsed%60):D2}   요청 {apiClient.totalRequests}회");

        GUILayout.Space(10);
        GUI.contentColor = new Color(0.6f, 0.9f, 1f);
        GUILayout.Label("━ 평균 센서값");
        GUI.contentColor = Color.white;
        GUILayout.Label($"  온도 {simulator.lastAvgTemp:F1}°C    습도 {simulator.lastAvgHumidity:F0}%");
        GUILayout.Label($"  토양 {simulator.lastAvgSoil:F0}%      조도 {simulator.lastAvgLight:F0} lx");

        GUILayout.Space(10);
        GUI.contentColor = new Color(0.6f, 0.9f, 1f);
        GUILayout.Label("━ 진단 결과 (실시간)");
        DrawCount("  정상", stateController.healthyCount, new Color(0.4f, 1f, 0.4f));
        DrawCount("  병해", stateController.diseaseCount, new Color(1f, 0.4f, 0.4f));
        DrawCount("  수분부족", stateController.waterShortageCount, new Color(1f, 0.9f, 0.3f));
        DrawCount("  성장중", stateController.growthStageCount, new Color(0.6f, 1f, 0.5f));

        GUILayout.Space(10);
        GUI.contentColor = new Color(0.6f, 0.9f, 1f);
        GUILayout.Label("━ PyTorch AI 추론");
        GUI.contentColor = Color.white;
        GUILayout.Label($"  평균 신뢰도 {stateController.aiAvgConfidence:F3}");
        GUI.contentColor = stateController.ruleAiAgreement > 0.9f ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.7f, 0.3f);
        GUILayout.Label($"  룰-AI 일치율 {stateController.ruleAiAgreement * 100f:F1}%");
        GUI.contentColor = Color.white;

        GUILayout.Space(10);
        GUI.contentColor = new Color(0.6f, 0.9f, 1f);
        GUILayout.Label("━ 시스템 로그");
        GUI.contentColor = new Color(0.85f, 0.85f, 0.85f);
        GUI.skin.label.fontSize = 12;
        if (EventLogger.Instance != null)
            foreach (var log in EventLogger.Instance.logs)
                GUILayout.Label(log);
        GUI.skin.label.fontSize = 14;

        GUILayout.EndArea();

        // Status bar
        bool ok = apiClient.lastStatus.Contains("OK");
        GUI.contentColor = ok ? Color.green : Color.red;
        GUIStyle status = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        GUI.Label(new Rect(0, Screen.height - 40, Screen.width, 30), ok ? "● 시스템 정상 작동 중" : "● 통신 오류", status);
        GUI.contentColor = Color.white;
    }

    void DrawCount(string label, int count, Color color)
    {
        GUILayout.BeginHorizontal();
        GUI.contentColor = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.Label(label, GUILayout.Width(110));
        GUI.contentColor = color;
        GUILayout.Label($"{count}주");
        GUILayout.EndHorizontal();
    }
}
