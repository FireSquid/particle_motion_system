// Date Created:        May 13, 2020
// Created By:          Peter Reynolds

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    private const float WIDTH = 75f;
    private const float HEIGHT = 35f;

    public static List<ParticleController> particles { get; private set; }

    [SerializeField]
    private SpriteRenderer spriteRend;

    private Vector2 velocity;

    private static bool initialized = false;
    private static List<List<Vector2>> forceEdges;
    private static List<List<float>> meanForceDist;
    private static List<List<float>> forceDistHalfLength;
    private static List<List<float>> forceMult;
    private static List<float> closeForce;

    private float maxForce;

    private int partType;

    private void Initialize()
    {
        initialized = true;

        particles = new List<ParticleController>();

        closeForce = new List<float>();
        forceEdges = new List<List<Vector2>>();
        meanForceDist = new List<List<float>>();
        forceDistHalfLength = new List<List<float>>();
        forceMult = new List<List<float>>();

        for (int c = 0; c < PartMatList.partMatList.PartMats.Count; c++)
        {
            closeForce.Add(Random.Range(0.005f, 0.01f));

            forceMult.Add(new List<float>());
            forceEdges.Add(new List<Vector2>());
            meanForceDist.Add(new List<float>());
            forceDistHalfLength.Add(new List<float>());

            for (int i = 0; i < PartMatList.partMatList.PartMats.Count; i++)
            {
                forceMult[c].Add(0.012f * Random.Range(-1.5f, 1.0f));
                float minDist = Random.Range(1.1f, 1.8f);
                forceEdges[c].Add(new Vector2(minDist, Random.Range(minDist, 5f)));
                meanForceDist[c].Add((forceEdges[c][i].x + forceEdges[c][i].y) / 2f);
                forceDistHalfLength[c].Add((forceEdges[c][i].y - forceEdges[c][i].x) / 2f);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!initialized)
            Initialize();

        partType = Random.Range(0, PartMatList.partMatList.PartMats.Count);
        spriteRend.material = PartMatList.partMatList.PartMats[partType];

        particles.Add(this);
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        foreach (ParticleController part in particles)
        {
            if (part != this)
            {
                if (blockDist(transform.position, part.transform.position) < 12f)
                {
                    float partDist = (transform.position - part.transform.position).magnitude;

                    if (partDist < 1f)
                    {
                        Vector2 newForce = (Vector2)(part.transform.position - transform.position).normalized * (1.0f / (forceEdges[partType][part.partType].x + 3.0f) - 1.0f / (partDist + 3.0f));
                        velocity += newForce;
                    }
                    else if (partDist > forceEdges[partType][part.partType].x && partDist < forceEdges[partType][part.partType].y)
                    {
                        Vector2 newForce = (Vector2)(part.transform.position - transform.position).normalized * (1 - Mathf.Abs(partDist - meanForceDist[partType][part.partType]) / forceDistHalfLength[partType][part.partType]) * forceMult[partType][part.partType];
                        velocity += newForce;
                    }
                }
            }
        }

        velocity *= 0.9f;
        */
        if (transform.position.x < -WIDTH)
        {
            velocity.x = Mathf.Abs(velocity.x);
        }
        else if (transform.position.x > WIDTH)
        {
            velocity.x = -Mathf.Abs(velocity.x);
        }

        if (transform.position.y < -HEIGHT)
        {
            velocity.y = Mathf.Abs(velocity.y);
        }
        else if (transform.position.y > HEIGHT)
        {
            velocity.y = -Mathf.Abs(velocity.y);
        }

        transform.position = new Vector3(Mathf.Clamp(transform.position.x , -WIDTH, WIDTH), Mathf.Clamp(transform.position.y, -HEIGHT, HEIGHT), 0);

        //transform.position += (Vector3)velocity;
    }

    private void OnDestroy()
    {
        particles.Remove(this);
    }

    private static float blockDist(Vector3 A, Vector3 B)
    {
        return (Mathf.Abs(A.x - B.x) + Mathf.Abs(A.y - B.y));
    }

    public static void Reset()
    {
        if (particles != null)
            foreach (ParticleController part in particles)
            {
                Destroy(part.gameObject);
            }

        initialized = false;
    }

    public static Vector2[] GetParticlePositions()
    {
        Vector2[] output = new Vector2[particles.Count];
        for (int p=0; p< particles.Count; p++)
        {
            output[p] = (Vector2)particles[p].transform.position;
        }
        return output;
    }

    public static void SetParticlePositions(Vector2[] pos)
    {
        for (int p = 0; p < particles.Count; p++)
        {
            particles[p].transform.position = pos[p];
        }
    }

    public static Vector2[] GetParticleVelocities()
    {
        Vector2[] output = new Vector2[particles.Count];
        for (int p = 0; p < particles.Count; p++)
        {
            output[p] = particles[p].velocity;
        }
        return output;
    }

    public static void SetParticleVelocities(Vector2[] vels)
    {
        for (int p = 0; p < particles.Count; p++)
        {
            particles[p].velocity = vels[p];
        }
    }

    public static uint[] GetParticleTypes()
    {
        uint[] output = new uint[particles.Count];
        for (int p = 0; p < particles.Count; p++)
        {
            output[p] = (uint)particles[p].partType;
        }
        return output;
    }

    public static void SetParticleTypes(uint[] types)
    {
        for (int p = 0; p < particles.Count; p++)
        {
            particles[p].partType = (int)types[p];
        }
    }

    public static Vector3[] GetProperties()
    {
        Vector3[] pProps = new Vector3[PartMatList.partMatList.PartMats.Count * PartMatList.partMatList.PartMats.Count];

        for (int p = 0; p < PartMatList.partMatList.PartMats.Count; p++)
        {
            for (int q = 0; q < PartMatList.partMatList.PartMats.Count; q++)
            {                
                pProps[p * PartMatList.partMatList.PartMats.Count + q] = new Vector3(forceEdges[p][q].x, forceEdges[p][q].y, forceMult[p][q]);
            }            
        }

        return pProps;
    }
}
