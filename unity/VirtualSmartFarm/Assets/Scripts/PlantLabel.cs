using UnityEngine;

public class PlantLabel : MonoBehaviour
{
    public int plantId;
    private TextMesh textMesh;
    private GameObject labelGO;
    private Camera mainCam;

    void Awake()
    {
        labelGO = new GameObject("Label");
        labelGO.transform.SetParent(transform, false);
        labelGO.transform.localPosition = new Vector3(0, 1.7f, 0);

        textMesh = labelGO.AddComponent<TextMesh>();
        textMesh.fontSize = 48;
        textMesh.characterSize = 0.04f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;
        textMesh.text = $"#{plantId:D2}\nINITIALIZING";

        MeshRenderer mr = labelGO.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 100;
    }

    public void SetState(string state)
    {
        if (textMesh == null) return;

        string stateText, actionText;
        Color color;

        switch (state)
        {
            case "healthy":
                stateText = "HEALTHY"; actionText = "";
                color = new Color(0.5f, 1f, 0.5f); break;
            case "disease":
                stateText = "DISEASE"; actionText = "STERILIZING";
                color = new Color(1f, 0.4f, 0.4f); break;
            case "water_shortage":
                stateText = "LOW WATER"; actionText = "WATERING";
                color = new Color(1f, 0.9f, 0.3f); break;
            case "growth_stage":
                stateText = "GROWING"; actionText = "LED ON";
                color = new Color(0.7f, 1f, 0.5f); break;
            default:
                stateText = state.ToUpper(); actionText = "";
                color = Color.white; break;
        }

        string display = $"#{plantId:D2}  {stateText}";
        if (!string.IsNullOrEmpty(actionText))
            display += $"\n>> {actionText}";

        textMesh.text = display;
        textMesh.color = color;
    }

    void LateUpdate()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam != null && labelGO != null)
        {
            labelGO.transform.rotation = Quaternion.LookRotation(
                labelGO.transform.position - mainCam.transform.position
            );
        }
    }
}
