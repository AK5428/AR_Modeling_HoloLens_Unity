using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Rhino.Compute;
using Photon.Pun;

public class RhinoModelBasic : PhotonMonoBehaviour
{
    #region In inspector
    [Header("Prefabs and GameObjects")]
    [SerializeField, FormerlySerializedAs("Rhino Compute Authentication Token")]
    public string authToken;

    [SerializeField, FormerlySerializedAs("Modeling Material")]
    public Material mat;

    [SerializeField, FormerlySerializedAs("Photon Object Prefab")]
    public GameObject photonObjectPrefab;

    [SerializeField, FormerlySerializedAs("Paint Model Switcher")]
    public GameObject switcherObject;

    #endregion

    #region Public variables hide in inspector
    [HideInInspector]
    public PhotonView photonView;
    // [HideInInspector]
    // public List<int> modelObjectIds = new List<int>();
    [HideInInspector]
    public GameObject currentFatherObj;
    [HideInInspector]
    public int currentFatherObjId;
    #endregion

    #region Private variables hide in inspector
    // private GameObject currentFatherObj;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        ComputeServer.AuthToken = authToken;
        ComputeServer.WebAddress = "http://localhost:8081/";

        if (PhotonNetwork.PrefabPool is DefaultPool pool)
        {
            if (photonObjectPrefab != null && !pool.ResourceCache.ContainsKey(photonObjectPrefab.name))
            {
                pool.ResourceCache.Add(photonObjectPrefab.name, photonObjectPrefab);
            }
        }

        photonView = GetComponent<PhotonView>();

    }

    // Update is called once per frame
    public virtual void Update()
    {
        
    }

    public virtual void TryStartModeling()
    {
        try
        {
            StartModeling();
        }
        catch (System.Exception e)
        {
            // Debug error
            Debug.LogError(e);

            // End the modeling
            switcherObject.GetComponent<PaintModelSwitcherBasic>().SwitchMode();
        }
    }

    public virtual void StartModeling()
    {
        GameObject fatherObj = PhotonNetwork.Instantiate(photonObjectPrefab.name, Vector3.zero, Quaternion.identity, 0);
        // fatherObj.name = "RhinoModel";
        // fatherObj.tag = "RhinoModel";
        currentFatherObj = fatherObj;
        currentFatherObjId = FindPhotonView(currentFatherObj).ViewID;

        GetComponent<PhotonView>().RPC("SetFatherObj", RpcTarget.All, currentFatherObjId);
    }

    [PunRPC]
    public void SetFatherObj(int fatherObjId)
    {
        GameObject fatherObj = FindPunObject(fatherObjId);
        fatherObj.name = "RhinoModel";
        fatherObj.tag = "RhinoModel";
    }


    public virtual void EndModeling()
    {
        // Add bound for the model
        var boundingManagerCaller = new BoundingManagerCaller();
        boundingManagerCaller.CreateBounding(FindPhotonView(currentFatherObj).ViewID);
        
        // Switch to paint again
        switcherObject.GetComponent<PaintModelSwitcherBasic>().SwitchMode();
    }

    public float[] Vector3ToFloatList(List<Vector3> vector3List)
    {
        var floatList = new List<float>();
        foreach(var vector3 in vector3List)
        {
            floatList.Add(vector3.x);
            floatList.Add(vector3.y);
            floatList.Add(vector3.z);
        }
        return floatList.ToArray();
    }

    public List<Vector3> FloatListToVector3(float[] floatList)
    {
        var vector3List = new List<Vector3>();
        for(int i = 0; i < floatList.Length; i += 3)
        {
            vector3List.Add(new Vector3(floatList[i], floatList[i + 1], floatList[i + 2]));
        }
        return vector3List;
    }

    // to create the model in the unity, at first, we need to turn the Rhino class into normal data structure
    public virtual void GetMeshDataAndCreate(List<Rhino.Geometry.Mesh> meshList, Vector3 objectPosition, int getFatherObjId)
    {
        GameObject fatherObj = PhotonView.Find(getFatherObjId).gameObject;

        // convert the rhino mesh into Unity mesh
        foreach(var mesh in meshList)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var normals = new List<Vector3>();
            
            foreach(var meshVertex in mesh.Vertices)
            {
                var vertex = new Vector3((float)meshVertex.X, (float)meshVertex.Y, (float)meshVertex.Z);
                vertices.Add(vertex);
            }
            
            foreach(var meshFace in mesh.Faces)
            {
                if(meshFace.IsTriangle)
                {
                    // if start with A, then the mesh would be flipped
                    triangles.Add(meshFace.C);
                    triangles.Add(meshFace.B);
                    triangles.Add(meshFace.A);
                }else if(meshFace.IsQuad)
                {
                    // the quad in the unity would be split into two triangles
                    triangles.Add(meshFace.C);
                    triangles.Add(meshFace.B);
                    triangles.Add(meshFace.A);
                    triangles.Add(meshFace.D);
                    triangles.Add(meshFace.C);
                    triangles.Add(meshFace.A);
                }
            }

            foreach(var meshNormal in mesh.Normals)
            {
                var normal = new Vector3((float)meshNormal.X, (float)meshNormal.Z, (float)meshNormal.Y);
                // flip the normal
                // normal = -normal;
                normals.Add(normal);
            }
        
            // create the mesh object
            CreateRhinoMesh(vertices, triangles, normals, fatherObj);
        }
    }

    public void CreateRhinoMesh(List<Vector3> vertices, List<int> triangles, List<Vector3> normals, GameObject fatherObj)
    {
        // get the position of the first vertex
        // initialize the mesh object as the root of this single mesh
        Vector3 position = Vector3.zero;
        GameObject singleMeshObject = new GameObject("RhinoMesh");
        singleMeshObject.transform.parent = fatherObj.transform;

        // create the mesh object
        Mesh meshObj = new Mesh();
        meshObj.vertices = vertices.ToArray();
        meshObj.triangles = triangles.ToArray();
        meshObj.normals = normals.ToArray();

        // add the mesh object to the single mesh object
        singleMeshObject.AddComponent<MeshFilter>().mesh = meshObj;
        singleMeshObject.AddComponent<MeshRenderer>().material = mat;
    }

}