using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoidManager : MonoBehaviour {
    private List<Transform> _boids = new List<Transform>();
    [Header("Geomtery")]
    public Transform Prefab;
    public Transform Target;
    [Header("Boids")]
    public int NumberOfBoids;
    public float NeighborDistance;          // 0.8
    public float MaxVelocty;
    public float MaxRotationAngle;
    // public Vector3 InitialVelocity;
    [Header("Cohesion")]
    [Tooltip("Arbitary text message")]
    public float CohesionStep;              // 100
    public float CohesionWeight;            // 0.05
    [Header("Separation")]
    public float SeparationWeight;          // 0.01`
    [Header("Alignment")]
    public float AlignmentWeight;           // 0.01
    [Header("Seek")]
    public float SeekWeight;                // 0
    [Header("Socialize")]
    public float SocializeWeight;           // 0
    [Header("Arrival")]
    public float ArrivalSlowingDistance;    // 2
    public float ArrivalMaxSpeed;           // 0.2
    [Header("Edge Avoidance")]
    public Transform cornersParent;         // Parent of the corners of the world
    private Vector3 center = Vector3.zero;
    public float EdgeAvoidanceWeight;       // 1
    private float[,] bounds;
    [Header("Boid Speed Management")]
    public float averageSpeed;
    public float speedDeviation;
    public float SpeedManagementWeight;
    [Header("Noise")]
    public float NoiseWeight;

    // Use this for initialization
    private void Start ()
	{
        CalculateBounds();

	    for (var i = 0; i < NumberOfBoids; ++i)
	    {
            /*
	        var position = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(0f, 2.0f), Random.Range(-1.0f, 1.0f));
	        var transform = Instantiate(Prefab, position, Quaternion.identity);
            var speed = averageSpeed + speedDeviation * Random.Range(-1.0f, 1.0f);

            transform.GetComponent<Boid>().UpdateBoid(position, InitialVelocity, speed);
	        _boids.Add(transform);
            */
            _boids.Add(GenerateBoid());
	    }

//	    StartCoroutine(UpdateOnFrame());

	    for (var i = 0; i < _boids.Count; ++i)
	    {
	        var boid = _boids[i].GetComponent<Boid>();
	        boid.UpdateNeighbors(_boids, NeighborDistance);
//	        boid.PrintNeighbors();
	    }
	}

    private Transform GenerateBoid() {
        // Chose random corner
        var randCorner = cornersParent.GetChild(Random.Range(0, cornersParent.childCount));
        var dir2center = (center - randCorner.position).normalized;
        var pos = randCorner.position + 3 * dir2center + Random.insideUnitSphere * .5f;

        // var pos = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(0f, 2.0f), Random.Range(-1.0f, 1.0f));
        var tra = Instantiate(Prefab, pos, Quaternion.identity);
        var spe = averageSpeed + speedDeviation * Random.Range(-1.0f, 1.0f);

        tra.GetComponent<Boid>().UpdateBoid(pos, dir2center, spe);
        return tra;
    }

    private void UpdateBoids()
    {
        for (var i = 0; i < _boids.Count; ++i)
        {
            /*
            // Check if position is out of bounds
            if(!WithinBounds(_boids[i].position)) {
                Debug.Log("Destroyed boid");
                Destroy(_boids[i].gameObject);
                _boids[i] = GenerateBoid();
            }
            */


            var boid = _boids[i].GetComponent<Boid>();
            // Update its neighbors within a distance
            boid.UpdateNeighbors(_boids, NeighborDistance);

            // Steering Behaviors
            var cohesionVector = boid.Cohesion(CohesionStep, CohesionWeight);
            var separationVector = boid.Separation(SeparationWeight);
            var alignmentVector = boid.Alignment(AlignmentWeight);

            var speedManagementVector = boid.ManageSpeed(SpeedManagementWeight);

            // var seekVector = boid.Seek(Target, SeekWeight);
            var seekVector = Vector3.zero;
            var socializeVector = boid.Socialize(_boids, SocializeWeight);
            // var socializeVector = Vector3.zero;

            // var arrivalVector = boid.Arrival(Target, ArrivalSlowingDistance, ArrivalMaxSpeed);
            var arrivalVector = Vector3.zero;


            // Edge avoidance
            var edgeAvoidanceVector = boid.EdgeAvoidance(bounds, EdgeAvoidanceWeight);
            // Debug.Log("Edge avoidance:" + edgeAvoidanceVector.magnitude);

            /*
            if(i == 0) {
                // Debug.Log("CV: " + cohesionVector.magnitude);
                // Debug.Log("SV: " + separationVector.magnitude);
                // Debug.Log("AV: " + alignmentVector.magnitude);
                // Debug.Log("EAV: " + edgeAvoidanceVector.magnitude);
            }
            */
            var noiseVector = boid.Noise(NoiseWeight);

            // Update Boid's Position and Velocity
            var velocity =
                boid.Velocity +
                cohesionVector +
                separationVector +
                alignmentVector +
                // seekVector +
                socializeVector +
                // arrivalVector +
                edgeAvoidanceVector +
                speedManagementVector +
                noiseVector +
                Vector3.zero;
            velocity = boid.LimitVelocity(velocity, MaxVelocty);
            // velocity = boid.LimitRotation(velocity, MaxRotationAngle, MaxVelocty);
            var prev = boid.Position;
            var position = boid.Position + velocity * Time.deltaTime;
            boid.UpdateBoid(position, velocity);
            Debug.DrawLine(prev, boid.Position, Color.magenta, 40);

        }
    }

    private void CalculateBounds() {
        bounds = new float[3, 2];
        Transform[] corners = new Transform[cornersParent.childCount];
        for (int i = 0; i < cornersParent.childCount; i++) corners[i] = cornersParent.GetChild(i);

        // default values
        bounds[0, 0] = corners[0].position.x;
        bounds[0, 1] = corners[0].position.x;
        bounds[1, 0] = corners[0].position.y;
        bounds[1, 1] = corners[0].position.y;
        bounds[2, 0] = corners[0].position.z;
        bounds[2, 1] = corners[0].position.z;

        // actual values
        foreach (Transform c in corners) {
            bounds[0, 0] = Mathf.Min(bounds[0, 0], c.position.x);
            bounds[0, 1] = Mathf.Max(bounds[0, 1], c.position.x);
            bounds[1, 0] = Mathf.Min(bounds[1, 0], c.position.y);
            bounds[1, 1] = Mathf.Max(bounds[1, 1], c.position.y);
            bounds[2, 0] = Mathf.Min(bounds[2, 0], c.position.z);
            bounds[2, 1] = Mathf.Max(bounds[2, 1], c.position.z);

            center += c.position;
        }
        center /= corners.Length;

        /*
        Debug.Log(bounds[0, 0]);
        Debug.Log(bounds[0, 1]);
        Debug.Log(bounds[1, 0]);
        Debug.Log(bounds[1, 1]);
        Debug.Log(bounds[2, 0]);
        Debug.Log(bounds[2, 1]);
        */
    }

	// Update is called once per frame
	private void FixedUpdate()
	{
        UpdateBoids();
	}

    private IEnumerator UpdateOnFrame()
    {
        while (true)
        {
            UpdateBoids();
            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// Checks whether the given position is within the bounds of the corners
    /// </summary>
    /// <param name="pos">Queried position</param>
    /// <returns></returns>
    private bool WithinBounds(Vector3 pos) {
        var offset = 1f;
        return
            bounds[0, 0] - offset < pos.x &&
            bounds[0, 1] + offset > pos.x &&
            bounds[1, 0] - offset < pos.y &&
            bounds[1, 1] + offset > pos.y &&
            bounds[2, 0] - offset < pos.z &&
            bounds[2, 1] + offset > pos.z;
    }
}
