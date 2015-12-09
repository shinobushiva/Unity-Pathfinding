using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NodeWalkBehavior : ABehavior
{
	private List<Element> resultPath;
	private List<Element> nodeResult;
	private Node currentTarget;
	private Node prevTarget;
	private SpeedDirectionBehavior walkerBehavior;
	private float sight = 2f;
	public float sightDistance = 1f;
	
	void Start ()
	{
		walkerBehavior = gameObject.GetComponent<SpeedDirectionBehavior> ();
		if (!walkerBehavior) {
			print ("Error: No WalkerBehavior is attached!");
			Destroy (this);
		} else {
			print ("WalkerBehavior : " + name);
		}
	
	}

	public override void Step ()
	{
		if (currentTarget == null){
			walkerBehavior.Speed = 0;
			return;
		}
	
		if (walkerBehavior.Speed > 0.5f) {
			sight = 2f * sightDistance;
		} else {
			sight = 1f * sightDistance;
		}
	
		RaycastHit[] hits = Physics.RaycastAll (transform.position, walkerBehavior.Direction, 
		Vector3.Distance (transform.position, currentTarget.Position));
		foreach (RaycastHit h in hits) {
			if (h.collider.gameObject.tag == "Wall") {
				Element e = nodeResult.Where (n => n.Id.Equals (currentTarget.Id)).Single ();
				int idx = nodeResult.IndexOf (e);
				//print ("Idx : " + idx);
				if (idx > 0) {
					Node next = (Node)nodeResult [idx - 1];
					walkerBehavior.LookAt(next.Position, true);

					print ("Chaged target because of hitting a wall");
					ChangeTarget (next);
		
					return;
				}
			}
		}

		//print ("Dist TO :"+currentTarget.Id+":"+Vector3.Distance(transform.position, currentTarget.Position));
		if(nodeResult[nodeResult.Count - 1].Id.Equals(currentTarget.Id)){
			if(Vector3.Distance(transform.position, currentTarget.Position) < 1){
				walkerBehavior.Speed = 0f;
				//Goal
				print ("Player reached the goal");
				AttachedAgent.World.EndRequest = true;
				return;
			}
		}
	
		bool b = Vector3.Distance (currentTarget.Position, transform.position) < 0.5f;
		distToTarget = Vector3.Distance (currentTarget.Position, transform.position);
	
		if (b) {
			Element e = nodeResult.Where (n => n.Id.Equals (currentTarget.Id)).Single ();
			int idx = nodeResult.IndexOf (e);
			//print ("Idx : " + idx);
			if (idx == nodeResult.Count - 1) {
				//AttachedAgent.World.ResignAgent (AttachedAgent);
				walkerBehavior.Speed = 0f;
				//Goal
				print ("Player reached the goal");
				AttachedAgent.World.EndRequest = true;
			} else {
				Node next = (Node)nodeResult [idx + 1];
				walkerBehavior.LookAt(next.Position, true);
			
				print ("Heading to the next node : "+(idx+1));
				ChangeTarget (next);
			}
		} else {
			walkerBehavior.LookAt(currentTarget.Position, true);
		}
	
	}

	public float distToTarget = 0;
	
	private void ChangeTarget (Node next)
	{
		prevTarget = currentTarget;
		currentTarget = next;
		
		LinkObject currentLink = GetCurrentLink ();
		if (currentLink != null) {
			//print ("CurrentLink Name:" + currentLink.name);
		}
	}
	
	public LinkObject GetCurrentLink ()
	{
		if (prevTarget == null || currentTarget == null)
			return null;
	
		NodeLinkMaster nlm = NodeLinkMaster.Instance;
		NodeObject no1 = nlm.nodeObjectMap [prevTarget.Id];
		NodeObject no2 = nlm.nodeObjectMap [currentTarget.Id];
		IEnumerable<LinkObject> res = no1.connectedLinks.Intersect (no2.connectedLinks).Where (x => x.head == no1);
		/*
	foreach (LinkObject loo in res) {
		print (loo.name);
	}
	*/
		if (res.Count () > 1 || res.Count () == 0) {
			print ("Invalid current link result : " + res.Count ());
			return null;
		}
	
		LinkObject lo = res.Single ();
	
		return lo;
	
	}

	private int nodeIndex = 0;
	
	public void Setup (List<Element> resultPath)
	{
		this.resultPath = resultPath;
		nodeResult = resultPath.FindAll (n => n is Node);
		if (nodeResult.Count < 2)
			return;

		nodeIndex = 0;
	
		Vector3 p1 = ((Node)nodeResult [nodeIndex]).Position;
		Vector3 p2 = ((Node)nodeResult [nodeIndex+1]).Position;
		transform.position = p1+Vector3.up;
		walkerBehavior.Direction = (p2 - p1).normalized;

		prevTarget = ((Node)nodeResult [nodeIndex]);
		currentTarget = ((Node)nodeResult [nodeIndex+1]);
		nodeIndex++;
	}
	
	void OnDrawGizmos ()
	{
	
		Gizmos.color = Color.white;
		if (currentTarget != null)
			Gizmos.DrawLine (transform.position, currentTarget.Position);
	
		if (walkerBehavior) {
			Gizmos.color = Color.green;
			Gizmos.DrawLine (transform.position, transform.position + walkerBehavior.Direction * sight);
		}
	}
}
