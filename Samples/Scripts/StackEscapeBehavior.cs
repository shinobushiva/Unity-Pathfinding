using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StackEscapeBehavior : ABehavior
{

	//
	public float timeToCheckStacking = 5f;
	public float distanceToJudgeStacking = 1f;
	private float nextTimeToCheckStacking = 0;
	private float nextTimeToNormal = 0;
	private float collisionRadius;
	private CharacterController cc;

	
	//
	private Queue<Vector3> recentPositions = new Queue<Vector3> ();

	private void RegisterRecentPositions (Vector3 cur)
	{
		recentPositions.Enqueue (cur);
	}
	
	private float GetRecentDistanceMoved ()
	{
		float d = 0;
		Vector3[] rps = recentPositions.ToArray ();
		for (int i=0; i<recentPositions.Count-1; i++) {
			d += Vector3.Distance (rps [i], rps [i + 1]);
		}
		
		return d;
	}


	void Initialize ()
	{
		
		nextTimeToCheckStacking = Time.time + timeToCheckStacking;
		
		cc = GetComponent<CharacterController> ();
		if (cc)
			collisionRadius = cc.radius;
	}
	
	void Begin ()
	{
	}

	void Step ()
	{
		//save the position history
		RegisterRecentPositions (transform.position);

		float dist = GetRecentDistanceMoved ();

		if (nextTimeToCheckStacking <= Time.time) {
		
			if (dist < distanceToJudgeStacking) {
				cc.radius = 0.01f;
				nextTimeToNormal = Time.time + timeToCheckStacking;
			}
					
			recentPositions.Clear ();
			nextTimeToCheckStacking = Time.time + timeToCheckStacking;
		}
		
		if (nextTimeToNormal < Time.time) {
			cc.radius = collisionRadius;
		}
	}
	
	void Commit ()
	{
	}
	
	void End ()
	{
	}
	
	void Dispose ()
	{
	}
}
