using UnityEngine;
using System.Collections.Generic;

public class GreenhouseGenerator : MonoBehaviour
{
    [Header("밭 구조 (시연용 축소)")]
    public int bedCount = 1;
    public int plantsPerBed = 6;
    public float bedLength = 9f;
    public float bedWidth = 1.5f;
    public float bedHeight = 0.25f;
    public float bedSpacing = 3f;

    [Header("식물 설정")]
    public int leafCountMin = 4;
    public int leafCountMax = 6;
    public float plantBaseHeight = 0.9f;

    [Header("머티리얼")]
    public Material soilMaterial;
    public Material plantMaterial;
    public Material stemMaterial;

    [HideInInspector]
    public List<GameObject> plants = new List<GameObject>();

    void Start() { GenerateFarm(); }

    void GenerateFarm()
    {
        plants.Clear();
        float totalDepth = (bedCount - 1) * bedSpacing;
        float startZ = -totalDepth / 2f;

        for (int b = 0; b < bedCount; b++)
        {
            float z = startZ + b * bedSpacing;
            CreateBed(z, b);
            CreatePlantsOnBed(z, b);
        }
        Debug.Log($"농장 생성 완료: 베드 {bedCount}개, 총 식물 {plants.Count}개");
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
            CreatePlant(x, bedHeight, z, bedIndex, p);
        }
    }

    void CreatePlant(float x, float baseY, float z, int bedIndex, int plantIndex)
    {
        int plantId = bedIndex * plantsPerBed + plantIndex;
        GameObject plant = new GameObject($"Plant_{plantId:D2}");
        plant.transform.position = new Vector3(x, baseY, z);
        plant.transform.parent = this.transform;

        // 줄기
        GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stem.name = "Stem";
        stem.transform.parent = plant.transform;
        stem.transform.localPosition = new Vector3(0, plantBaseHeight / 2f, 0);
        stem.transform.localScale = new Vector3(0.1f, plantBaseHeight / 2f, 0.1f);
        if (stemMaterial != null)
            stem.GetComponent<Renderer>().material = stemMaterial;
        else
            stem.GetComponent<Renderer>().material.color = new Color(0.35f, 0.22f, 0.1f);

        // 잎사귀들
        int leafCount = Random.Range(leafCountMin, leafCountMax + 1);
        float baseAngleOffset = Random.Range(0f, 360f);

        for (int i = 0; i < leafCount; i++)
        {
            GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leaf.name = $"Leaf_{i}";
            leaf.transform.parent = plant.transform;

            float angleDeg = baseAngleOffset + (360f / leafCount) * i + Random.Range(-20f, 20f);
            float rad = angleDeg * Mathf.Deg2Rad;
            float radius = 0.2f + Random.Range(-0.04f, 0.06f);
            float heightOffset = plantBaseHeight * 0.65f + Random.Range(-0.08f, 0.2f);

            leaf.transform.localPosition = new Vector3(
                Mathf.Cos(rad) * radius,
                heightOffset,
                Mathf.Sin(rad) * radius
            );

            leaf.transform.localRotation = Quaternion.Euler(
                Random.Range(35f, 65f),
                angleDeg,
                Random.Range(-15f, 15f)
            );

            float leafSize = Random.Range(0.18f, 0.26f);
            leaf.transform.localScale = new Vector3(leafSize * 0.6f, leafSize, leafSize * 0.6f);

            if (plantMaterial != null)
                leaf.GetComponent<Renderer>().material = new Material(plantMaterial);
        }

        // 라벨 컴포넌트 추가
        PlantLabel label = plant.AddComponent<PlantLabel>();
        label.plantId = plantId;

        plants.Add(plant);
    }
}
