using UnityEngine;
using System.Collections.Generic;

public class PlantResponse : MonoBehaviour
{
    private Dictionary<int, GameObject> activeEffects = new Dictionary<int, GameObject>();

    public void TriggerResponse(int plantId, string plantClass, GameObject plant)
    {
        if (activeEffects.ContainsKey(plantId) && activeEffects[plantId] != null)
        {
            Destroy(activeEffects[plantId]);
            activeEffects.Remove(plantId);
        }
        if (plantClass == "water_shortage")
            activeEffects[plantId] = CreateEffect(plant, new Color(0.4f, 0.7f, 1f, 0.8f), 0.1f, 1f, 1f, 25f, 15);
        else if (plantClass == "disease")
            activeEffects[plantId] = CreateEffect(plant, new Color(1f, 0.8f, 0.2f, 0.7f), 0.05f, 0.5f, 0.8f, 360f, 20);
    }

    GameObject CreateEffect(GameObject plant, Color color, float size, float speed, float lifetime, float angle, float rate)
    {
        GameObject fx = new GameObject("FX");
        fx.transform.position = plant.transform.position + Vector3.up * 1.5f;
        fx.transform.parent = plant.transform;
        ParticleSystem ps = fx.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = color;
        main.startSize = size;
        main.startSpeed = speed;
        main.startLifetime = lifetime;
        main.gravityModifier = (angle > 180f) ? 0f : 1f;
        var emission = ps.emission;
        emission.rateOverTime = rate;
        var shape = ps.shape;
        shape.shapeType = (angle > 180f) ? ParticleSystemShapeType.Sphere : ParticleSystemShapeType.Cone;
        shape.angle = Mathf.Min(angle, 89f);
        shape.radius = 0.3f;
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        return fx;
    }
}
