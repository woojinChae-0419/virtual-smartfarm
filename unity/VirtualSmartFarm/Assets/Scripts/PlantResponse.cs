using UnityEngine;
using System.Collections.Generic;

public class PlantResponse : MonoBehaviour
{
    private Dictionary<int, GameObject> activeEffects = new Dictionary<int, GameObject>();

    public void TriggerResponse(int plantId, string plantClass, GameObject plant)
    {
        // 기존 효과 제거
        if (activeEffects.ContainsKey(plantId) && activeEffects[plantId] != null)
        {
            Destroy(activeEffects[plantId]);
            activeEffects.Remove(plantId);
        }

        GameObject fx = null;
        switch (plantClass)
        {
            case "water_shortage":
                fx = CreateWaterSpray(plant); break;
            case "disease":
                fx = CreateSterilizationMist(plant); break;
            case "growth_stage":
                fx = CreateGrowthLight(plant); break;
            // healthy는 효과 없음
        }

        if (fx != null) activeEffects[plantId] = fx;
    }

    // 수분부족 → 파란 물방울 (스프링클러)
    GameObject CreateWaterSpray(GameObject plant)
    {
        GameObject fx = new GameObject("WaterSpray");
        fx.transform.position = plant.transform.position + Vector3.up * 2.2f;
        fx.transform.parent = plant.transform;

        ParticleSystem ps = fx.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(0.3f, 0.65f, 1f, 0.9f);
        main.startSize = 0.12f;
        main.startSpeed = 2.0f;
        main.startLifetime = 1.2f;
        main.gravityModifier = 2.5f;
        main.maxParticles = 100;

        var emission = ps.emission;
        emission.rateOverTime = 30;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 20f;
        shape.radius = 0.15f;
        shape.rotation = new Vector3(180, 0, 0); // 아래로 분사

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));

        return fx;
    }

    // 병해 → 흰 살균제 안개 (구체)
    GameObject CreateSterilizationMist(GameObject plant)
    {
        GameObject fx = new GameObject("SterilMist");
        fx.transform.position = plant.transform.position + Vector3.up * 0.8f;
        fx.transform.parent = plant.transform;

        ParticleSystem ps = fx.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(0.95f, 0.95f, 0.95f, 0.55f);
        main.startSize = 0.35f;
        main.startSpeed = 0.4f;
        main.startLifetime = 2.0f;
        main.gravityModifier = -0.1f; // 살짝 위로 떠오름
        main.maxParticles = 80;

        var emission = ps.emission;
        emission.rateOverTime = 18;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.6f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));

        return fx;
    }

    // 성장중 → 노란 LED 빛
    GameObject CreateGrowthLight(GameObject plant)
    {
        GameObject fx = new GameObject("GrowthLED");
        fx.transform.position = plant.transform.position + Vector3.up * 2.5f;
        fx.transform.parent = plant.transform;

        ParticleSystem ps = fx.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(1f, 0.95f, 0.4f, 0.75f);
        main.startSize = 0.18f;
        main.startSpeed = 0.6f;
        main.startLifetime = 1.5f;
        main.gravityModifier = 0.2f;
        main.maxParticles = 60;

        var emission = ps.emission;
        emission.rateOverTime = 15;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 35f;
        shape.radius = 0.25f;
        shape.rotation = new Vector3(180, 0, 0);

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));

        // 점광원 추가 (실제 빛처럼)
        GameObject lightGO = new GameObject("PointLight");
        lightGO.transform.SetParent(fx.transform, false);
        Light light = lightGO.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.9f, 0.5f);
        light.intensity = 1.5f;
        light.range = 2.5f;

        return fx;
    }
}
