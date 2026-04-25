using UnityEngine;

public class GreenhouseGenerator : MonoBehaviour
{
    [Header("밭 구조")]
    public int bedCount = 4;
    public int plantsPerBed = 12;
    public float bedLength = 16f;
    public float bedWidth = 1.2f;
    public float bedHeight = 0.2f;
    public float bedSpacing = 3f;

    [Header("식물")]
    public float plantHeight = 0.8f;
    public float plantScale = 0.3f;

    [Header("색상 (선택)")]
    public Material soilMaterial;
    public Material plantMaterial;

    void Start()
    {
        GenerateFarm();
    }

    void GenerateFarm()
    {
        float totalDepth = (bedCount - 1) * bedSpacing;
        float startZ = -totalDepth / 2f;

        for (int b = 0; b < bedCount; b++)
        {
            float z = startZ + b * bedSpacing;
            CreateBed(z, b);
            CreatePlantsOnBed(z, b);
        }

        Debug.Log($"농장 생성 완료: 베드 {bedCount}개, 총 식물 {bedCount * plantsPerBed}개");
    }

    void CreateBed(float z, int bedIndex)
    {
        GameObject bed = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bed.name = $"Bed_{bedIndex}";
        bed.transform.position = new Vector3(0, bedHeight / 2f, z);
        bed.transform.localScale = new Vector3(bedLength, bedHeight, bedWidth);
        bed.transform.parent = this.transform;

        if (soilMaterial != null)
            bed.GetComponent<Renderer>().material = soilMaterial;
    }

    void CreatePlantsOnBed(float z, int bedIndex)
    {
        float spacing = bedLength / (plantsPerBed + 1);
        float startX = -bedLength / 2f + spacing;

        for (int p = 0; p < plantsPerBed; p++)
        {
            float x = startX + p * spacing;

            GameObject plant = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            plant.name = $"Plant_{bedIndex}_{p}";
            plant.transform.position = new Vector3(
                x,
                bedHeight + plantHeight / 2f,
                z
            );
            plant.transform.localScale = new Vector3(plantScale, plantHeight / 2f, plantScale);
            plant.transform.parent = this.transform;

            if (plantMaterial != null)
                plant.GetComponent<Renderer>().material = plantMaterial;
        }
    }
}