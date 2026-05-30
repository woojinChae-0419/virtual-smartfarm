using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class APIClient : MonoBehaviour
{
    [Header("서버 설정")]
    public string serverUrl = "http://localhost:8000";
    public float requestInterval = 1f;

    [Header("상태 (실시간 확인용)")]
    public string lastStatus = "대기 중";
    public int totalRequests = 0;

    private SensorSimulator simulator;
    private PlantStateController stateController;

    void Start()
    {
        simulator = GetComponent<SensorSimulator>();
        stateController = GetComponent<PlantStateController>();
        InvokeRepeating(nameof(SendSensorData), 1f, requestInterval);
    }

    void SendSensorData()
    {
        if (simulator == null) return;
        BatchSensorReading batch = simulator.GenerateReadings();
        string json = JsonUtility.ToJson(batch);
        StartCoroutine(PostRequest($"{serverUrl}/predict_batch", json));
    }

    IEnumerator PostRequest(string url, string jsonData)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] body = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
        totalRequests++;

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            BatchDiagnosis diag = JsonUtility.FromJson<BatchDiagnosis>(response);
            stateController?.ApplyDiagnoses(diag);
            lastStatus = $"OK ({diag.diagnoses.Count} plants)";
        }
        else
        {
            lastStatus = $"ERROR: {request.error}";
            Debug.LogWarning($"[APIClient] 서버 통신 실패: {request.error}");
        }
    }
}