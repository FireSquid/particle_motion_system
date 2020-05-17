// Date Created:        May 13, 2020
// Created By:          Peter Reynolds

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    private const float WIDTH = 79.5f;
    private const float HEIGHT = 44.5f;

    [SerializeField]
    private Material partMat;
    [SerializeField]
    private Mesh partMesh;

    private Vector2 velocity;

    private List<Vector2> particlePositions;
    private List<Vector2> particleVelocities;
    private List<uint> particleTypes;

    private bool initialized = false;
    private List<List<Vector2>> forceEdges;
    private List<List<float>> meanForceDist;
    private List<List<float>> forceDistHalfLength;
    private List<List<float>> forceMult;

    private float maxForce;

    private int partType;

    [SerializeField]
    private ComputeShader shader;

    [SerializeField]
    private GameObject particlePrefab;

    public List<Material> PartMats { get; private set; }

    //public PartMatList partMatList { get; private set; }

    private float last_reset;

    private void Initialize()
    {
        initialized = true;

        particlePositions = new List<Vector2>();
        particleVelocities = new List<Vector2>();
        particleTypes = new List<uint>();

        forceEdges = new List<List<Vector2>>();
        meanForceDist = new List<List<float>>();
        forceDistHalfLength = new List<List<float>>();
        forceMult = new List<List<float>>();

        for (int c = 0; c < PartMats.Count; c++)
        {

            forceMult.Add(new List<float>());
            forceEdges.Add(new List<Vector2>());
            meanForceDist.Add(new List<float>());
            forceDistHalfLength.Add(new List<float>());

            for (int i = 0; i < PartMats.Count; i++)
            {
                forceMult[c].Add(0.012f * Random.Range(-1.5f, 1.0f));
                float minDist = Random.Range(1.1f, 1.8f);
                forceEdges[c].Add(new Vector2(minDist, Random.Range(minDist, 5f)));
                meanForceDist[c].Add((forceEdges[c][i].x + forceEdges[c][i].y) / 2f);
                forceDistHalfLength[c].Add((forceEdges[c][i].y - forceEdges[c][i].x) / 2f);
            }
        }
    }

    private void Reset()
    {
        initialized = false;

        last_reset = Time.time;

        PartMats = new List<Material>();

        int partCount = Random.Range(5, 12);
        for (int i = 0; i < partCount; i++)
        {
            Material newMat = new Material(partMat);
            newMat.color = Random.ColorHSV(0, 1, 0.7f, 1f, 0.5f, 1f, 1f, 1f);
            PartMats.Add(newMat);
        }

        Initialize();
    }

    void Awake()
    {
        Application.targetFrameRate = 60;
    }

    // Start is called before the first frame update
    void Start()
    {
        Reset();
    }

    // Update is called once per frame
    void Update()
    {

        //transform.position += (Vector3)velocity;

        RunShader();

        if (initialized && (Input.GetKeyDown(KeyCode.S) || particlePositions.Count < 512))
            for (int i = 0; i < 256; i++)
            {
                particlePositions.Add(new Vector2(Random.Range(-WIDTH + 5f, WIDTH - 5f), Random.Range(-HEIGHT + 5f, HEIGHT - 5f)));
                particleVelocities.Add(Vector2.zero);
                particleTypes.Add((uint)Random.Range(0, PartMats.Count));
            }

        if (Input.GetKeyDown(KeyCode.R) || Time.time > last_reset + 240f)
            Reset();

        for (int p = 0; p < GetParticleCount(); p++)
        {
            Graphics.DrawMesh(partMesh, Matrix4x4.TRS(particlePositions[p], Quaternion.Euler(-90f,0f,0f), new Vector3(0.1f, 0.1f, 0.1f)), PartMats[(int)particleTypes[p]], 0);
        }
    }

    private static float blockDist(Vector3 A, Vector3 B)
    {
        return (Mathf.Abs(A.x - B.x) + Mathf.Abs(A.y - B.y));
    }

    public Vector2[] GetParticlePositions()
    {
        return particlePositions.ToArray();
    }

    public void SetParticlePositions(Vector2[] pos)
    {
        particlePositions = new List<Vector2>(pos);
    }

    public Vector2[] GetParticleVelocities()
    {
        return particleVelocities.ToArray();
    }

    public void SetParticleVelocities(Vector2[] vels)
    {
        particleVelocities = new List<Vector2>(vels);
    }

    public uint[] GetParticleTypes()
    {
        return particleTypes.ToArray();
    }

    public void SetParticleTypes(uint[] types)
    {
        particleTypes = new List<uint>(types);
    }

    public Vector3[] GetProperties()
    {
        Vector3[] pProps = new Vector3[PartMats.Count * PartMats.Count];

        for (int p = 0; p < PartMats.Count; p++)
        {
            for (int q = 0; q < PartMats.Count; q++)
            {                
                pProps[p * PartMats.Count + q] = new Vector3(forceEdges[p][q].x, forceEdges[p][q].y, forceMult[p][q]);
            }            
        }

        return pProps;
    }

    private void RunShader()
    {
        if (!initialized || GetParticleCount() <= 0)
            return;

        Vector2[] posData = GetParticlePositions();
        ComputeBuffer pPos = new ComputeBuffer(posData.Length, 8);
        pPos.SetData(posData);

        Vector2[] velData = GetParticleVelocities();
        ComputeBuffer pVels = new ComputeBuffer(velData.Length, 8);
        pVels.SetData(velData);

        uint[] typeData = GetParticleTypes();
        ComputeBuffer pTypes = new ComputeBuffer(typeData.Length, 4);
        pTypes.SetData(typeData);

        Vector2[] newPosData = new Vector2[particlePositions.Count];
        ComputeBuffer nPos = new ComputeBuffer(newPosData.Length, 8);
        nPos.SetData(newPosData);

        Vector3[] propertyData = GetProperties();
        ComputeBuffer pProps = new ComputeBuffer(propertyData.Length, 12);
        pProps.SetData(propertyData);

        int kernelHandle = shader.FindKernel("PartPhys");

        shader.SetBuffer(kernelHandle, "pPos", pPos);
        shader.SetBuffer(kernelHandle, "pVels", pVels);
        shader.SetBuffer(kernelHandle, "pTypes", pTypes);
        shader.SetBuffer(kernelHandle, "nPos", nPos);
        shader.SetBuffer(kernelHandle, "pProps", pProps);
        shader.SetInt("pCount", particlePositions.Count);
        shader.SetInt("pTypeCount", PartMats.Count);
        shader.SetVector("boardSize", new Vector4(-WIDTH, -HEIGHT, WIDTH, HEIGHT));
        shader.Dispatch(kernelHandle, posData.Length, 1, 1);

        pPos.Dispose();
        pProps.Dispose();
        pTypes.Dispose();

        Vector2[] newPosOutput = new Vector2[particlePositions.Count];
        Vector2[] newVelsOutput = new Vector2[particlePositions.Count];

        nPos.GetData(newPosOutput);
        pVels.GetData(newVelsOutput);

        pVels.Dispose();
        nPos.Dispose();

        SetParticlePositions(newPosOutput);
        SetParticleVelocities(newVelsOutput);
    }

    private int GetParticleCount()
    {
        if (!initialized)
            return 0;

        return particlePositions.Count;
    }

    public static int SGetParticleCount()
    {
        return GameObject.FindGameObjectWithTag("ParticleController").GetComponent<ParticleController>().GetParticleCount();
    }

    void OnDrawGizmos()
    {
        for (int p = 0; p < GetParticleCount(); p++)
        {
            Gizmos.DrawSphere(particlePositions[p], 1);
        }
    }
}
