using Fractalscape;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Skybox : MonoBehaviour
{
    [SerializeField] private float _boxSize;
    [SerializeField] private Vector3 _boxCenter;
    [SerializeField] private int _tileSize;
    [SerializeField] private int _tilePadding;
    [SerializeField] private Material _skyboxMaterial;

    private Texture2D _skyboxTexture;
    private Mesh _skyboxMesh;
    private Renderer _skyboxRenderer;

    private void Awake()
    {
        AppSession.Skybox = this;
        GetComponent<MeshFilter>().mesh = _skyboxMesh = new Mesh();
        _skyboxRenderer = GetComponent<Renderer>();
        GenerateMesh();
        _skyboxRenderer.sharedMaterial = _skyboxMaterial;
    }

    public void LoadTexture(Material mat)
    {
        _skyboxRenderer.sharedMaterial = mat;
    }

    public void GenerateMesh()
    {
        // Generate vertices

        Vector3[] skyboxVertices =
        {
            // Left
            new Vector3( 0.5f,-0.5f, 0.5f),
            new Vector3( 0.5f, 0.5f, 0.5f),
            new Vector3( 0.5f, 0.5f,-0.5f),
            new Vector3( 0.5f,-0.5f,-0.5f),
            // Front
            new Vector3( 0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f,-0.5f),
            new Vector3( 0.5f, 0.5f,-0.5f),
            // Right
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f,-0.5f, 0.5f),
            new Vector3(-0.5f,-0.5f,-0.5f),
            new Vector3(-0.5f, 0.5f,-0.5f),
            // Back
            new Vector3(-0.5f,-0.5f, 0.5f),
            new Vector3( 0.5f,-0.5f, 0.5f),
            new Vector3( 0.5f,-0.5f,-0.5f),
            new Vector3(-0.5f,-0.5f,-0.5f),
            // Bottom
            new Vector3( 0.5f,-0.5f,-0.5f),
            new Vector3( 0.5f, 0.5f,-0.5f),
            new Vector3(-0.5f, 0.5f,-0.5f),
            new Vector3(-0.5f,-0.5f,-0.5f),
            // Top
            new Vector3(-0.5f,-0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3( 0.5f, 0.5f, 0.5f),
            new Vector3( 0.5f,-0.5f, 0.5f),
        };

        Matrix4x4 rot = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(-90f, Vector3.right), Vector3.one);
        for (int i = 0; i < skyboxVertices.Length; i++)
        {
            skyboxVertices[i] *= _boxSize;
            skyboxVertices[i] += _boxCenter;
            skyboxVertices[i] = rot.MultiplyPoint(skyboxVertices[i]);
        }

        _skyboxMesh.vertices = skyboxVertices;

        // Generate UV coordinates

        var wO = _tilePadding / (3f * _tileSize); // 0.0006502f
        var hO = _tilePadding / (2f * _tileSize); // 0.00195312f

        Debug.Log(hO);

        const float third = 1f / 3f;
        const float half = 0.5f;

        Vector2[] leftUv =
        {
            //left
            new Vector2(third+wO,1-hO),
            new Vector2(third*2-wO, 1-hO),
            new Vector2(third*2-wO, half+hO),
            new Vector2(third+wO, half+hO),

            //front
            new Vector2(third+wO, half-hO),
            new Vector2(third*2-wO, half-hO),
            new Vector2(third*2-wO, 0f+hO),
            new Vector2(third+wO, 0f+hO),

            //right
            new Vector2(0+wO, 1f-hO),
            new Vector2(third-wO, 1f-hO),
            new Vector2(third-wO, half+hO),
            new Vector2(0+wO, half+hO),

            //back
            new Vector2(third*2+wO, half-hO),
            new Vector2(1-wO, half-hO),
            new Vector2(1-wO, 0+hO),
            new Vector2(third*2+wO, 0+hO),

            //bottom
            new Vector2(0+wO, 0+hO),
            new Vector2(0+wO, half-hO),
            new Vector2(third-wO, half-hO),
            new Vector2(third-wO, 0+hO),

            //top
            new Vector2(1-wO, 1-hO),
            new Vector2(1-wO, half+hO),
            new Vector2(third*2+wO, half+hO),
            new Vector2(third*2+wO, 1-hO)
        };

        _skyboxMesh.uv = leftUv;

        // Generate Triangles

        int[] leftTriangles =
        {
            0,1,2,
            0,2,3,
            4,5,6,
            4,6,7,
            8,9,10,
            8,10,11,
            12,13,14,
            12,14,15,
            16,17,18,
            16,18,19,
            20,21,22,
            20,22,23
        };
        _skyboxMesh.triangles = leftTriangles;
        _skyboxMesh.RecalculateNormals();
    }
}