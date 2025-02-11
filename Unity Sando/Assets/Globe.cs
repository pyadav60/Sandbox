using UnityEngine;

public class Globe : MonoBehaviour
{
    [Header("Globe Settings")]
    public int resolution = 10;
    public float radius = 5f;

    [Header("Material Settings")]
    public Material waterMaterial;
    public Material landMaterial;
    public float landThreshold = 0.5f;
    public int noiseSeed = 42;

    [Header("World Settings")]
    [Range(0f, 1f)] public float temperature = 0.5f; // 0 = cold (big ice caps), 1 = hot (small ice caps)

    [Header("Rotation Settings")]
    public float rotationSpeed = 100f; // Drag sensitivity
    public float inertiaDuration = 1.5f; // Time for inertia to decay
    public float inertiaDamping = 2.5f;  // Higher = quicker slowdown
    public Vector3 defaultSpin = new Vector3(0, 10, 0); // Passive rotation

    private Vector3 dragSpin;
    private bool isDragging;
    private float inertiaTimeLeft = 0f;
    private Vector3 lastMousePosition;
    private Vector3 spinVelocity;

    private void Start()
    {
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
        // Remove previous cubes
        foreach (Transform child in transform)
        {
            if (child.gameObject.GetComponent<Renderer>() != null && child.gameObject.GetComponent<Renderer>().material != null)
            {
                // Check if it's a cube by examining the type of its collider (optional check for efficiency)
                if (child.gameObject.GetComponent<Collider>() is BoxCollider)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        // Size of each cube
        float cubeSize = (2f * radius) / resolution;
        Vector3 center = transform.position;

        // Loop through the grid to place cubes
        for (int x = -resolution; x <= resolution; x++)
        {
            for (int y = -resolution; y <= resolution; y++)
            {
                for (int z = -resolution; z <= resolution; z++)
                {
                    Vector3 cubePosition = new Vector3(x, y, z) * cubeSize + center;
                    float distanceToCenter = Vector3.Distance(cubePosition, center);
                    if (distanceToCenter > radius || distanceToCenter < radius - cubeSize)
                        continue;

                    // Perlin Noise for land
                    float perlinValue = Mathf.PerlinNoise(
                        (x * 0.15f + noiseSeed * 0.1f),
                        (z * 0.15f + y * 0.1f + noiseSeed * 0.2f)
                    );

                    Material selectedMaterial = perlinValue > landThreshold ? landMaterial : waterMaterial;

                    // Create the cube with the selected material
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

    private Vector3 smoothedSpinVelocity = Vector3.zero;
    private float rotationLagFactor = 0.1f; // Higher value = more delay effect

    private void HandleRotation()
{
    // Change to right-click (button 1 is the right mouse button)
    if (Input.GetMouseButtonDown(1))
    {
        isDragging = true;
        lastMousePosition = Input.mousePosition;
        smoothedSpinVelocity = Vector3.zero; // Reset on click
    }

    if (Input.GetMouseButton(1)) // Right-click drag
    {
        isDragging = true;
        Vector3 mouseDelta = Input.mousePosition - lastMousePosition;

        // Target velocity based on mouse movement
        Vector3 targetSpinVelocity = new Vector3(-mouseDelta.y, mouseDelta.x, 0) * (rotationSpeed * 0.01f);

        // Smoothly interpolate to target velocity
        smoothedSpinVelocity = Vector3.Lerp(smoothedSpinVelocity, targetSpinVelocity, rotationLagFactor);

        // Apply rotation with the smoothed velocity
        transform.Rotate(smoothedSpinVelocity, Space.World);

        lastMousePosition = Input.mousePosition;

        // Set inertia duration when dragging
        inertiaTimeLeft = inertiaDuration;
    }
    else if (isDragging) // Mouse just released
    {
        isDragging = false;
        spinVelocity = smoothedSpinVelocity; // Transfer to inertia
    }
}


    private Vector3 lastSpinDirection = new Vector3(1, 1, 1).normalized * Mathf.Sqrt(3); // Initial default spin

    private void ApplyInertia()
    {
        if (!isDragging)
        {
            if (inertiaTimeLeft > 0)
            {
                float t = inertiaTimeLeft / inertiaDuration;
                float dampingFactor = Mathf.Pow(t, inertiaDamping);

                transform.Rotate(spinVelocity * dampingFactor * Time.deltaTime, Space.World);
                inertiaTimeLeft -= Time.deltaTime;

                if (inertiaTimeLeft <= 0.05f)
                {
                    lastSpinDirection = spinVelocity.normalized * Mathf.Sqrt(3);
                }
            }
            else
            {
                // Resume default passive rotation
                transform.Rotate(lastSpinDirection * Time.deltaTime, Space.World);
            }
        }
    }
}
