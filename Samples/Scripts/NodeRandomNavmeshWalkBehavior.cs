using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NodeRandomNavmeshWalkBehavior : ABehavior
{
	private List<Element> resultPath;
	private List<Element> nodeResult;
	private Node currentTarget;

	public Node CurrentTarget {
		set {
			currentTarget = value;
		}

		get {
			return currentTarget;
		}
	}

	private LinkObject[] links;
	private NodeObject[] nodes;
	
	//
	//private  SpeedDirectionBehavior speedDirectionBehavior;
	public float distanceArrival = 1f;
	//
	public float timeToCheckStacking = 5f;
	public float distanceToJudgeStacking = 1f;
	private float nextTimeToCheckStacking = 0;
	private float nextTimeToNormal = 0;
	private float collisionRadius;
	private CharacterController cc;
	//
	private Queue<Vector3> recentPositions = new Queue<Vector3> ();

	private MecanimNavmeshWalkBehavior navmeshWalker;
	
	void Start ()
	{
		//speedDirectionBehavior = GetComponent<SpeedDirectionBehavior> ();
		navmeshWalker = GetComponent<MecanimNavmeshWalkBehavior> ();
		nextTimeToCheckStacking = Time.time + timeToCheckStacking;

		cc = GetComponent<CharacterController> ();
		if (cc)
			collisionRadius = cc.radius;
	
	}
	
	public float UpdateRecentPositions (Vector3 cur)
	{
		recentPositions.Enqueue (cur);

		float d = 0;
		Vector3[] rps = recentPositions.ToArray ();
		for (int i=0; i<recentPositions.Count-1; i++) {
			d += Vector3.Distance (rps [i], rps [i + 1]);
		}

		return d;

	}
	
	override public void Begin ()
	{
		//print ("NodeRandomWalkBehavior#Begin");
		nodes = NodeLinkMaster.Instance.nodeObjectMap.Values.ToArray ();
		links = NodeLinkMaster.Instance.linkObjectMap.Values.ToArray ();

		if (currentTarget == null) {
			currentTarget = FindNearest ();
		}
		
	}
	
	private Node FindNearest ()
	{
		NodeObject[] ns = nodes.OrderBy (
			n => Vector3.Distance (n.transform.position, transform.position)
		).ToArray ();
		if (ns [0].node == currentTarget)
			return ns [Random.Range (0, ns.Length)].node;
		else
			return ns [0].node;
	}
	
	public float range = 1f;
	public float viewDist = 1f;
	public float viewHeightOffset = 1f;
	private float rightLeft = Random.Range (0, 1f) > 0.5f ? 1 : -1;

	public override void Step ()
	{
		float distFromTarget = Vector3.Distance (transform.position, currentTarget.Position);
		if (distFromTarget == 0)
			return;

		//GetComponent<PlaceIndicator>().text = ""+(int)(distFromTarget);

		RaycastHit[] hits = Physics.RaycastAll (transform.position, transform.forward, 
			Vector3.Distance (transform.position, currentTarget.Position));
		foreach (RaycastHit h in hits) {
			if (h.collider.gameObject.tag == "Wall") {
				currentTarget = FindNearest ();
				navmeshWalker.SetDestination(currentTarget.Position);

				//print ("Found Nearest : " + currentTarget.Id);

				Vector3 p1 = transform.position;
				Vector3 p2 = currentTarget.Position;
				transform.LookAt (p2);
				//speedDirectionBehavior.Direction = (p2 - p1).normalized;

				return;
			}
		}

		List<SpeedDirectionBehavior> list = GetAgentsAroundPosition<SpeedDirectionBehavior> (
			AttachedAgent.World, transform.position + Vector3.up * viewHeightOffset + transform.forward * viewDist, range);
		list.Remove (GetComponent<SpeedDirectionBehavior> ());
		if (list.Count > 0) {
			transform.Rotate (Vector3.up * 20 * rightLeft);
			//speedDirectionBehavior.Direction = transform.forward;
		} 

		
		float dist = UpdateRecentPositions (transform.position);

		if (nextTimeToCheckStacking > Time.time) {

		} else {

			if (dist < distanceToJudgeStacking) {
				//Stacking;
				currentTarget = FindNearest ();
				navmeshWalker.SetDestination(currentTarget.Position);

				Vector3 p1 = transform.position;
				Vector3 p2 = currentTarget.Position;
				transform.LookAt (p2);
				//speedDirectionBehavior.Direction = (p2 - p1).normalized;

				cc.radius = 0.01f;
				nextTimeToNormal = Time.time + timeToCheckStacking;

				return;
			}
			
			recentPositions.Clear ();
			nextTimeToCheckStacking = Time.time + timeToCheckStacking;
		}

		if (nextTimeToNormal < Time.time) {
			cc.radius = collisionRadius;
		}

		if (distFromTarget < distanceArrival) {

			List<NodeObject> nl = GetAgentsAroundPosition<NodeObject> (AttachedAgent.World, transform.position, 1f);

			nl = nl.OrderBy (n => Vector3.Distance (transform.position, n.transform.position)).ToList ();

			Dictionary<LinkObject, int> numMap = new Dictionary<LinkObject, int> ();
			foreach (LinkObject lo in links) {
				LinkAttribute la = lo.gameObject.GetComponent<LinkAttribute> ();
				if (la != null) {
					int np = (la.numPedstriansHoliday + la.numPedstriansWeekday) / 2;
					if (np <= 0)
						np = 1;
					numMap [lo] = np;
				} else {
					numMap [lo] = 1;
				}
			}

			List<Node> candidates = new List<Node> ();
			foreach (LinkObject lo in links) {
				if (lo.head.node == currentTarget) {
					if (!candidates.Contains (lo.tail.node)) {
						for (int i=0; i<numMap[lo]; i++)
							candidates.Add (lo.tail.node);
					}
				} else if (lo.tail.node == currentTarget) {
					if (!candidates.Contains (lo.head.node)) {
						for (int i=0; i<numMap[lo]; i++)
							candidates.Add (lo.head.node);
					}
				}
			}
			candidates.Remove (currentTarget);

			Node next = candidates [Random.Range (0, candidates.Count)];
			Vector3 p1 = transform.position;
			Vector3 p2 = next.Position;
			transform.LookAt (p2);
			//speedDirectionBehavior.Direction = (p2 - p1).normalized;
			currentTarget = next;
			navmeshWalker.SetDestination(currentTarget.Position);

		} else {
			Vector3 p1 = transform.position;
			Vector3 p2 = currentTarget.Position;
			transform.LookAt (p2);
			//speedDirectionBehavior.Direction = (p2 - p1).normalized;
		}

	}
	
	void OnDrawGizmos ()
	{
		
		Gizmos.color = Color.white;
		if (currentTarget != null)
			Gizmos.DrawLine (transform.position, currentTarget.Position);
		
		/*
		if (speedDirectionBehavior) {
			Gizmos.color = Color.green;
			Gizmos.DrawLine (transform.position, transform.position + speedDirectionBehavior.Direction * distanceArrival);
		}
		*/

		Gizmos.DrawWireSphere (transform.position + transform.forward * viewDist + Vector3.up * viewHeightOffset, range);

	}
}
