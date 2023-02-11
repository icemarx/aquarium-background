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
    public float NeighborDistance;        // 0.8
    public float MaxVelocty;
    public float MaxRotationAngle;
    public Vector3 InitialVelocity;
    [Header("Cohesion")]
    [Tooltip("Arbitary text message")]
    public float CohesionStep;            // 100
    public float CohesionWeight;          // 0.05
    [Header("Separation")]
    public float SeparationWeight;        // 0.01`
    [Header("Alignment")]
    public float AlignmentWeight;         // 0.01
    [Header("Seek")]
    public float SeekWeight;              // 0
    [Header("Socialize")]
    public float SocializeWeight;         // 0
    [Header("Arrival")]
    public float ArrivalSlowingDistance;  // 2
    public float ArrivalMaxSpeed;         // 0.2

	// Use this for initialization
	private void Start ()
	{
	    for (var i = 0; i < NumberOfBoids; ++i)
	    {
	        var position = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(0f, 2.0f), Random.Range(-1.0f, 1.0f));
	        var transform = Instantiate(Prefab, position, Quaternion.identity);

	        transform.GetComponent<Boid>().UpdateBoid(position, InitialVelocity);
	        _boids.Add(transform);
	    }

//	    StartCoroutine(UpdateOnFrame());

	    for (var i = 0; i < _boids.Count; ++i)
	    {
	        var boid = _boids[i].GetComponent<Boid>();
	        boid.UpdateNeighbors(_boids, NeighborDistance);
//	        boid.PrintNeighbors();
	    }
	}

    private void UpdateBoids()
    {
        for (var i = 0; i < _boids.Count; ++i)
        {
            var boid = _boids[i].GetComponent<Boid>();
            // Update its neighbors within a distance
            boid.UpdateNeighbors(_boids, NeighborDistance);
            // Steering Behaviors
            var cohesionVector = boid.Cohesion(CohesionStep, CohesionWeight);
            var separationVector = boid.Separation(SeparationWeight);
            var alignmentVector = boid.Alignment(AlignmentWeight);
            // var seekVector = boid.Seek(Target, SeekWeight);
            var seekVector = Vector3.zero;
            var socializeVector = boid.Socialize(_boids, SocializeWeight);
            // var arrivalVector = boid.Arrival(Target, ArrivalSlowingDistance, ArrivalMaxSpeed);
            var arrivalVector = Vector3.zero;
            // Update Boid's Position and Velocity
            var velocity = boid.Velocity + cohesionVector + separationVector + alignmentVector + seekVector + socializeVector + arrivalVector;
            velocity = boid.LimitVelocity(velocity, MaxVelocty);
            velocity = boid.LimitRotation(velocity, MaxRotationAngle, MaxVelocty);
            var position = boid.Position + velocity;
            boid.UpdateBoid(position, velocity);
        }
    }

	// Update is called once per frame
	private void Update()
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

}
