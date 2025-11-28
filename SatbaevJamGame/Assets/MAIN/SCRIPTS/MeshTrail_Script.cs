using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TrailComponent : IComponent
{
    public MeshTrail_Script meshTrail;
}
public class MeshTrail_Script : MonoBehaviour
{
    public float activetime = 7f;
    [Header("Mesh Related")]
    public float meshRefreshRate = 0.1f;
    public float meshDestroyDelay = 3f;
    // Start is called before the first frame update
    private bool isTrailActive = false;
    public string shaderVarRef;
    public float shaderVarRate = 0.1f;
    public float shaderVarRefreshRate = 0.05f;
    [Header("Shader Related")]
    public Material mat;
    public Transform positionToSpawn;
    public SkinnedMeshRenderer[] skinnedMeshRenderers;

    void Start()
    {
        
    }

    public void Activate()
    {
        if (!isTrailActive)
        {
            isTrailActive = true;
            StartCoroutine(ActiveTrail(activetime));
        }
    }
    public void Activate(float time)
    {
        if (!isTrailActive)
        {
            isTrailActive = true;
            StartCoroutine(ActiveTrail(time));
        }
    }

    IEnumerator ActiveTrail(float timeActive)
    {
        while(timeActive>0)
        {
            timeActive -= meshRefreshRate;

            if(skinnedMeshRenderers == null)
            {
                skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            }

            for(int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                GameObject gObj = new GameObject();

                gObj.transform.SetPositionAndRotation(positionToSpawn.position, Quaternion.Euler(-90f, positionToSpawn.rotation.eulerAngles.y, positionToSpawn.rotation.eulerAngles.z));

                MeshRenderer mr = gObj.AddComponent<MeshRenderer>();
                MeshFilter mf = gObj.AddComponent<MeshFilter>();

                Mesh mesh = new Mesh();
                skinnedMeshRenderers[i].BakeMesh(mesh);

                mf.mesh = mesh;
                mr.material = mat;

                StartCoroutine(AnimateMaterialFloat(mr.material,0,shaderVarRate,shaderVarRefreshRate));
                Destroy(gObj,meshDestroyDelay);
            }

            yield return new WaitForSeconds(meshRefreshRate);
        }
        isTrailActive = false;
    }
    
    IEnumerator AnimateMaterialFloat(Material mat, float goal, float rate, float refreshrate)
    {
        float valueToAnimate = mat.GetFloat(shaderVarRef);
        while(valueToAnimate > goal) 
        {
            valueToAnimate -= rate;
            mat.SetFloat(shaderVarRef, valueToAnimate);
            yield return new WaitForSeconds(refreshrate);
        }
    }
}
