using UnityEngine;

public class Globe : MonoBehaviour
{
    [Header("Globe Settings")]
    public int resolution = 10; // Number of cubes along one axis of the sphere
    public float radius = 5f; // Radius of the globe

    [Header("Material Settings")]
    public Material waterMaterial; // Default water material
    public Material landMaterial; // Land material for continents
    public float landThreshold = 0.5f; // Perlin noise threshold for land
    public int noiseSeed = 42; // Seed for Perlin noise

    [Header("Rotation Settings")]
    public float rotationSpeed = 100f; // Speed of rotation when dragging
    public Vector3 defaultSpin = new Vector3(0, 10, 0); // Default spin axis and speed
    public float inertiaDamping = 0.95f; // Damping factor for inertia

    private Vector3 currentSpin;
    private Vector3 dragSpin;
    private bool isDragging;

    private void Start()
    {
        currentSpin = defaultSpin;
        Random.InitState(noiseSeed);
        GenerateGlobe();
    }

    private void Update()
    {
        HandleRotation();
        ApplyInertia();
    }

    private void GenerateGlobe()
    {
        // Clear existing children
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        float cubeSize = (2f * radius) / resolution;
        Vector3 center = transform.position;

        for (int x = -resolution; x <= resolution; x++)
        {
            for (int y = -resolution; y <= resolution; y++)
            {
                for (int z = -resolution; z <= resolution; z++)
                {
                    Vector3 cubePosition = new Vector3(x, y, z) * cubeSize + center;

                    // Check if the cube is on or inside the sphere's surface
                    float distanceToCenter = Vector3.Distance(cubePosition, center);
                    if (distanceToCenter > radius || distanceToCenter < radius - cubeSize)
                        continue;

                    // Calculate Perlin noise for land placement
                    float perlinValue = Mathf.PerlinNoise((x + resolution + noiseSeed) * 0.1f, (z + resolution + noiseSeed) * 0.1f);
                    Material selectedMaterial = perlinValue > landThreshold ? landMaterial : waterMaterial;

                    CreateCube(cubePosition, cubeSize, selectedMaterial);
                }
            }
        }
    }

    private void CreateCube(Vector3 position, float size, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = position;
        cube.transform.localScale = new Vector3(size, size, size);
        cube.transform.parent = transform;

        if (material != null)
        {
            Renderer renderer = cube.GetComponent<Renderer>();
            renderer.material = material;
        }
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(0)) // Left mouse button for rotation
        {
            isDragging = true;
            float horizontal = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float vertical = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            dragSpin = new Vector3(-vertical, horizontal, 0);
            transform.Rotate(dragSpin, Space.World);
        }
        else
        {
            isDragging = false;
        }
    }

    private void ApplyInertia()
    {
        if (!isDragging)
        {
            dragSpin *= inertiaDamping;
            transform.Rotate(dragSpin * Time.deltaTime, Space.World);

            if (dragSpin.magnitude < 0.01f)
            {
                dragSpin = Vector3.zero;
                transform.Rotate(currentSpin * Time.deltaTime, Space.World);
            }
        }
    }
}