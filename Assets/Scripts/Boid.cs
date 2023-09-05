using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Boid : MonoBehaviour
{
    public Vector3 Position;
    public Vector3 Velocity;
    public List<Transform> Neighbors;
    public float averageSpeed = 1;

    // Constructor with args
    public Boid(Vector3 position, Vector3 velocity)
    {
        Position = position;
        Velocity = velocity;
        gameObject.transform.position = position;
        gameObject.transform.rotation = Quaternion.LookRotation(Vector3.Normalize(velocity));
    }

    // Constructor with args
    public Boid(Vector3 position, Vector3 velocity, float averageSpeed) {
        Position = position;
        Velocity = velocity;
        gameObject.transform.position = position;
        gameObject.transform.rotation = Quaternion.LookRotation(Vector3.Normalize(velocity));
        this.averageSpeed = averageSpeed;
    }

    //-------------
    // UpdateBoid
    //-------------
    // Updates Position and Velocity of the Boid and moves the transform with the geometry attached.
    //-------------
    public void UpdateBoid(Vector3 position, Vector3 velocity)
    {
        Position = position;
        Velocity = velocity;
        gameObject.transform.position = position;
        gameObject.transform.rotation = Quaternion.LookRotation(Vector3.Normalize(velocity));
    }

    public void UpdateBoid(Vector3 position, Vector3 velocity, float averageSpeed) {
        Position = position;
        Velocity = velocity;
        gameObject.transform.position = position;
        gameObject.transform.rotation = Quaternion.LookRotation(Vector3.Normalize(velocity));
        this.averageSpeed = averageSpeed;
    }

    //------------------
    // UpdateNeighbors
    //------------------
    // Finds neighbors by distance and updates Neighbor List.
    //------------------
    public void UpdateNeighbors(List<Transform> boids, float distance)
    {
        var neighbors = new List<Transform>();

        for (var i = 0; i < boids.Count; ++i)
        {
            if (Position != boids[i].position)
            {
                if (Vector3.Distance(boids[i].position, Position) < distance)
                {
                    neighbors.Add(boids[i]);
                }
            }
        }
        Neighbors = neighbors;
    }

    //===========
    // Behaviors
    //===========

    //-----------
    // Cohesion
    //-----------
    public Vector3 Cohesion(float steps, float weight)
    {
        var pc = Vector3.zero;    // Perceived Center of Neighbors

        if (Neighbors.Count == 0 || steps < 1) return pc;

        // Add up the positions of the neighbors
        for (var i = 0; i < Neighbors.Count; ++i)
        {
            var neighbor = Neighbors[i].GetComponent<Boid>();
            if (pc == Vector3.zero)
            {
                pc = neighbor.Position;
            }
            else
            {
                pc = pc + neighbor.Position;
            }
        }
        // Average the neighbor's positions
        pc = pc / Neighbors.Count;
        // Return the offset vector, divide by steps (100 would mean 1% towards center) and multiply by weight
        return (pc - Position) / steps * weight;
    }

    //-------------
    // Separation
    //-------------
    public Vector3 Separation(float weight)
    {
        var c = Vector3.zero;    // Center point of a move away from close neighbors

        for (var i = 0; i < Neighbors.Count; ++i)
        {
            var neighbor = Neighbors[i].GetComponent<Boid>();
            var distance = Vector3.Distance(Position, neighbor.Position);

            c = c + Vector3.Normalize(Position - neighbor.Position) / Mathf.Pow(distance, 2);
        }
        return c * weight;
    }

    //-------------
    // Edge Avoidance
    //-------------
    public Vector3 EdgeAvoidance(float[,] bounds, float weight) {
        // Averages
        float[] averages = new float[3];
        for (int i = 0; i < averages.Length; i++)
            averages[i] = (bounds[i, 0] + bounds[i, 1]) / 2;

        
        // Aquarium size scale
        float[] sizeScale = new float[3];
        for (int i = 0; i < sizeScale.Length; i++)
            sizeScale[i] = Mathf.Abs(bounds[i, 0] - bounds[i, 1]);
        

        // Directions
        var x_dir = Mathf.Sign(averages[0] - transform.position.x);
        var y_dir = Mathf.Sign(averages[1] - transform.position.y);
        var z_dir = Mathf.Sign(averages[2] - transform.position.z);

        // Avoidance Scale
        Vector3 eav = weight * new Vector3(
            x_dir * Mathf.Pow(sizeScale[0] / ((transform.position.x + bounds[0, 0] - averages[0]) * (transform.position.x + bounds[0, 1] - averages[0])), 2f),
            y_dir * Mathf.Pow(sizeScale[1] / ((transform.position.y + bounds[1, 0] - averages[1]) * (transform.position.y + bounds[1, 1] - averages[1])), 2f),
            z_dir * Mathf.Pow(sizeScale[2] / ((transform.position.z + bounds[2, 0] - averages[2]) * (transform.position.z + bounds[2, 1] - averages[2])), 2f));

        return eav;
    }

    //------------
    // Alignment
    //------------
    public Vector3 Alignment(float weight)
    {
        Vector3 pv = Vector3.zero;    // Perceived Velocity of Neighbors

        if (Neighbors.Count == 0) return pv;

        for (var i = 0; i < Neighbors.Count; ++i)
        {
            var neighbor = Neighbors[i].GetComponent<Boid>();
            pv = pv + neighbor.Velocity;
        }
        // Average the velocities
        if (Neighbors.Count > 1)
        {
            pv = pv / (Neighbors.Count);
        }
        // Return the offset vector multiplied by weight
        return (pv - Velocity) * weight;
    }

    //-------
    // Seek
    //-------
    public Vector3 Seek(Transform target, float weight)
    {
        if (weight < 0.0001f) return Vector3.zero;

        var desiredVelocity = (target.position - Position) * weight;
        return desiredVelocity - Velocity;
    }

    //------------
    // Socialize
    //------------
    public Vector3 Socialize(List<Transform> boids, float weight)
    {
         var pc = Vector3.zero;    // Perceived Center of the rest of the flock

        if (Neighbors.Count != 0) return pc;

        // Add up the positions of all other boids
        for (var i = 0; i < boids.Count; ++i)
        {
            var boid = boids[i].GetComponent<Boid>();
            if (Position != boid.Position)
            {
                if (pc == Vector3.zero)
                {
                    pc = boid.Position;
                }
                else
                {
                    pc = pc + boid.Position;
                }
            }
        }
        // Average the positions
        if (boids.Count > 1)
        {
            pc = pc / (boids.Count - 1);
        }
        // Normalize the offset vector, divide by steps (100 would mean 1% towards center) and multiply by weight
        return Vector3.Normalize(pc - Position) * weight;
    }

    //----------
    // Arrival
    //----------
    public Vector3 Arrival(Transform target, float slowingDistance, float maxSpeed)
    {
        var desiredVelocity = Vector3.zero;
        if (slowingDistance < 0.0001f) return desiredVelocity;

        var targetOffset = target.position - Position;
        var distance = Vector3.Distance(target.position, Position);
        var rampedSpeed = maxSpeed * (distance / slowingDistance);
        var clippedSpeed = Mathf.Min(rampedSpeed, maxSpeed);
        if (distance > 0)
        {
            desiredVelocity = (clippedSpeed / distance) * targetOffset;
        }
        return desiredVelocity - Velocity;
    }

    // ------------------
    // Speed Management
    // ------------------
    public Vector3 ManageSpeed(float weight) {
        return weight * (averageSpeed - Velocity.magnitude) * Velocity.normalized;
    }

    // --------
    // Random
    // --------
    public Vector3 Noise(float weight) {
        // return weight * new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
        return weight * Random.onUnitSphere;
    }

    // --------------------
    // Adjust Orientation
    // --------------------
    public Vector3 AdjustOrientation(float weight)
    {
        // find projection of orientation
        var projection = new Vector3(Velocity.x, 0, Velocity.z);

        // scale projection based on y magnitude
        var aov = Velocity.y * projection.normalized;

        return weight * aov;
    }

    //===========
    // Utilities
    //===========

    //--------------------
    // GetNeighborsArray
    //--------------------
    public Transform[] GetNeighborsArray()
    {
        return Neighbors.ToArray();
    }

    //-----------------
    // PrintNeighbors
    //-----------------
    public void PrintNeighbors()
    {
        Debug.LogFormat("Neighbors: {0}", Neighbors.Count);
        for (var i = 0; i < Neighbors.Count; ++i)
        {
            var neighbor = Neighbors[i].GetComponent<Boid>();
            Debug.LogFormat("X: {0}  Y: {1}  Z: {2}", neighbor.Position.x, neighbor.Position.y, neighbor.Position.z);
        }
    }

    public Vector3 LimitVelocity(Vector3 v, float limit)
    {
        if (v.magnitude > limit)
        {
            v = v / v.magnitude * limit;
        }
        return v;
    }

    public Vector3 LimitRotation(Vector3 v, float maxAngle, float maxSpeed)
    {
        return Vector3.RotateTowards(Velocity, v, maxAngle * Mathf.Deg2Rad, maxSpeed);
    }
}
