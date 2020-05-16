// Date Created:        May 13, 2020
// Created By:          Peter Reynolds

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartMatList : MonoBehaviour
{
    [SerializeField]
    private ComputeShader shader;

    [SerializeField]
    private GameObject particlePrefab;

    [SerializeField]
    private Material[] partMats;

    [SerializeField]
    private Material matBase;

    public List<Material> PartMats { get; private set; }

    public static PartMatList partMatList { get; private set; }

    private float last_reset;

    // Start is called before the first frame update
    void Awake()
    {
        Application.targetFrameRate = 60;

        Reset();
    }

    void Update()
    {
        RunShader();

        //if (Input.GetKeyDown(KeyCode.R))
        //    Reset();

        if (Input.GetKeyDown(KeyCode.S) || (ParticleController.particles != null && ParticleController.particles.Count < 512))
            for (int i=0; i<256; i++)
                Instantiate(particlePrefab, new Vector3(Random.Range(-50f, 50f), Random.Range(-30f, 30f), 0f), Quaternion.identity);

        if (Time.time > last_reset + 240f)
            Reset();
    }

    private void Reset()
    {
        last_reset = Time.time;

        ParticleController.Reset();

        PartMats = new List<Material>();

        int partCount = Random.Range(7, 15);
        for (int i = 0; i < partCount; i++)
        {
            Material newMat = new Material(matBase);
            newMat.color = Random.ColorHSV(0, 1, 0.7f, 1f, 0.5f, 1f, 1f, 1f);
            PartMats.Add(newMat);
        }

        Debug.Log(partCount);

        partMatList = this;

        for (int i = 0; i < 256; i++)
            Instantiate(particlePrefab, new Vector3(Random.Range(-50f, 50f), Random.Range(-30f, 30f), 0f), Quaternion.identity);
    }

    private void RunShader()    {
        

        Vector2[] posData = ParticleController.GetParticlePositions();
        ComputeBuffer pPos = new ComputeBuffer(posData.Length, 8);
        pPos.SetData(posData);

        Vector2[] velData = ParticleController.GetParticleVelocities();
        ComputeBuffer pVels = new ComputeBuffer(velData.Length, 8);
        pVels.SetData(velData);

        uint[] typeData = ParticleController.GetParticleTypes();
        ComputeBuffer pTypes = new ComputeBuffer(typeData.Length, 4);
        pTypes.SetData(typeData);

        Vector2[] newPosData = new Vector2[ParticleController.particles.Count];
        ComputeBuffer nPos = new ComputeBuffer(newPosData.Length, 8);
        nPos.SetData(newPosData);

        Vector3[] propertyData = ParticleController.GetProperties();
        ComputeBuffer pProps = new ComputeBuffer(propertyData.Length, 12);
        pProps.SetData(propertyData);

        int kernelHandle = shader.FindKernel("PartPhys");

        shader.SetBuffer(kernelHandle, "pPos", pPos);
        shader.SetBuffer(kernelHandle, "pVels", pVels);
        shader.SetBuffer(kernelHandle, "pTypes", pTypes);
        shader.SetBuffer(kernelHandle, "nPos", nPos);
        shader.SetBuffer(kernelHandle, "pProps", pProps);
        shader.SetInt("pCount", ParticleController.particles.Count);
        shader.SetInt("pTypeCount", PartMats.Count);
        shader.Dispatch(kernelHandle, posData.Length, 1, 1);

        pPos.Dispose();
        pProps.Dispose();
        pTypes.Dispose();

        Vector2[] newPosOutput = new Vector2[ParticleController.particles.Count];
        Vector2[] newVelsOutput = new Vector2[ParticleController.particles.Count];

        nPos.GetData(newPosOutput);
        pVels.GetData(newVelsOutput);

        
        pVels.Dispose();
        nPos.Dispose();

        ParticleController.SetParticlePositions(newPosOutput);
        ParticleController.SetParticleVelocities(newVelsOutput);
    }
}
