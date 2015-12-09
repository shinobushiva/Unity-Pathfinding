using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NodeRandomWalkBehavior : ABehavior
{
	private List<Element> resultPath;
	private List<Element> nodeResult;
	private LinkObject[] links;
	private NodeObject[] nodes;
	private Dictionary<LinkObject, int> pedestrianNumMap;

	//
	private Element prevTarget;
	private Element currentTarget;
	private Node currentTargetNode;
	private Vector3 prevDestination;
	private Vector3 currentDestination;

	public Vector3 CurrentDestination {
		get {
			return currentDestination;
		}
		set {
			prevDestination = currentDestination;
			currentDestination = value;
		}
	}
	
	//
	private SpeedDirectionBehavior speedDirectionBehavior;
	private float speedMax;
	public float distanceArrival = 1f;

	private Transform hitObject;


	//
	public State state;
	public enum State
	{
		Walking,
		ArriveAtDestination,
		ArriveOnLink,
		ArriveOnNode,
		Turning,
		Avoiding
	}

	void Initialize ()
	{
		speedDirectionBehavior = GetComponent<SpeedDirectionBehavior> ();
		speedMax = speedDirectionBehavior.Speed;
	}
	
	override public void Begin ()
	{
		//print ("NodeRandomWalkBehavior#Begin");
		nodes = NodeLinkMaster.Instance.nodeObjectMap.Values.ToArray ();
		links = NodeLinkMaster.Instance.linkObjectMap.Values.ToArray ();

		pedestrianNumMap = new Dictionary<LinkObject, int> ();
		foreach (LinkObject lo in links) {
			LinkAttribute la = lo.gameObject.GetComponent<LinkAttribute> ();
			if (la != null) {
				int np = (la.numPedstriansHoliday + la.numPedstriansWeekday) / 2;
				if (np <= 0)
					np = 1;
				pedestrianNumMap [lo] = np;
			} else {
				pedestrianNumMap [lo] = 1;
			}
		}

		state = State.ArriveAtDestination;
	}
	
	private Node FindNearestNode ()
	{
		NodeObject[] ns = nodes.OrderBy (
			n => Vector3.Distance (n.transform.position, transform.position)
		).ToArray ();
		if (ns [0].node == currentTargetNode)
			return ns [1].node;
		else
			return ns [0].node;
	}

	private Link FindNearestLink ()
	{
		LinkObject[] ls = links.OrderBy (
			n => DistancePointLine (transform.position, n.link.HeadNode.Position, n.link.TailNode.Position)
		).ToArray ();
		return ls [0].link;
	}
	
	public float range = 1f;
	public float viewDist = 1f;
	public float viewHeightOffset = 1f;
	//private float rightLeft;

	private Node GetNextTargetNode ()
	{
		if (currentTargetNode == null) {
			return FindNearestNode ();
		}

		List<Node> candidates = new List<Node> ();
		foreach (LinkObject lo in links) {
			if (lo.head.node == currentTargetNode) {
				if (!candidates.Contains (lo.tail.node)) {
					for (int i = 0; i < pedestrianNumMap [lo]; i++)
						candidates.Add (lo.tail.node);
				}
			} else if (lo.tail.node == currentTargetNode) {
				if (!candidates.Contains (lo.head.node)) {
					for (int i = 0; i < pedestrianNumMap [lo]; i++)
						candidates.Add (lo.head.node);
				}
			}
		}
		candidates.Remove (currentTargetNode);
		Node next = candidates [Random.Range (0, candidates.Count)];

		return next;
	}

	#region State actions

	private void DoAvoidingState ()
	{
		speedDirectionBehavior.Speed = Mathf.Max(speedMax*0.5f, speedDirectionBehavior.Speed - speedMax*0.1f);

		if(hitObject == null)
			return;

		Vector3 p1 = hitObject.position;
		Vector3 p2 = transform.position;
		float ang = Mathf.Asin(Vector3.Cross (transform.forward, p1-p2).magnitude);
		if(ang > 0){
			ang = 30;
		}else{
			ang = -30;
		}

		speedDirectionBehavior.Turn(0, ang, 0);
		CurrentDestination = transform.position+speedDirectionBehavior.Direction*viewDist*2;
		speedDirectionBehavior.Turn(0, -ang, 0);
		if(currentTarget != null){
			prevTarget = currentTarget;
			currentTarget = null;
		}

		state = State.Turning;
	}

	private void DoArriveAtDestination ()
	{
		if(prevTarget != null){
			currentTarget = prevTarget;
			prevTarget = null;
			if(currentTarget is Node) {
				CurrentDestination = ((Node)currentTarget).Position;
			} else if (currentTarget is Link) {
				Link l = (Link)currentTarget;
				CurrentDestination = ProjectPointLine (transform.position, l.HeadNode.Position, l.TailNode.Position);
			}
		}else{
			Link l = FindNearestLink ();
			CurrentDestination = ProjectPointLine (transform.position, l.HeadNode.Position, l.TailNode.Position);
			currentTarget = l;
		}
		
		state = State.Turning;
	}

	private void DoArriveOnLink ()
	{
		Link l = (Link)currentTarget;
		if (Random.value > 0.5f) {
			CurrentDestination = l.HeadNode.Position;
			currentTarget = l.HeadNode;
		} else {
			CurrentDestination = l.TailNode.Position;
			currentTarget = l.TailNode;
		}

		state = State.Turning;
	}

	private void DoArriveOnNode ()
	{

		currentTargetNode = GetNextTargetNode ();
		CurrentDestination = currentTargetNode.Position;
		currentTarget = currentTargetNode;

		state = State.Turning;
	}

	private void DoTurningState ()
	{
		speedDirectionBehavior.LookAt (CurrentDestination, true);

		state = State.Walking;
	}

	private void DoWalkingState ()
	{

		//Check obstacles in front
		{
			CharacterController charCtrl = GetComponent<CharacterController> ();
			Vector3 pp1 = transform.position + charCtrl.center + Vector3.up * (-charCtrl.height * 0.5f);
			Vector3 pp2 = pp1 + Vector3.up * charCtrl.height;
			RaycastHit[] hits = Physics.CapsuleCastAll (pp1, pp2, charCtrl.radius, transform.forward, Vector3.Distance (transform.position, CurrentDestination));
			
			foreach (RaycastHit h in hits) {
				if(Vector3.Distance (h.point, transform.position) < viewDist){
					if(h.collider.GetComponent<SpeedDirectionBehavior> () != null) {
						state = State.Avoiding;
						hitObject = h.transform;
						break;
					} else if (h.collider.gameObject.tag == "Wall") {
						state = State.ArriveAtDestination;
						hitObject = h.transform;
						break;
					}
				}
			}
		}

		//get the distance form current target
		float distToDestination = Vector3.Distance (transform.position, CurrentDestination);

		bool arrived = false;
		if (distToDestination < distanceArrival) {
			arrived = true;
		} else if (Vector3.Angle (currentDestination - prevDestination, currentDestination - transform.position) > 90) {
			arrived = true;
		}

		if(arrived){
			if (currentTarget is Node)
				state = State.ArriveOnNode;
			else if (currentTarget is Link)
				state = State.ArriveOnLink;
			else
				state = State.ArriveAtDestination;
		}
				
		if(state == State.Walking){
			speedDirectionBehavior.Speed = Mathf.Min(speedMax, speedDirectionBehavior.Speed + speedMax*0.05f);
		}

	}
	#endregion

	void Step ()
	{

		switch (state) {
		case State.ArriveAtDestination:
			DoArriveAtDestination ();
			break;
		case State.ArriveOnLink:
			DoArriveOnLink ();
			break;
		case State.ArriveOnNode:
			DoArriveOnNode ();
			break;
		case State.Turning:
			DoTurningState ();
			break;
		case State.Walking:
			DoWalkingState ();
			break;
		case State.Avoiding:
			DoAvoidingState ();
			break;
		}
	}

	void Commit(){
		PlaceIndicator pi = GetComponent<PlaceIndicator> ();
		if (pi != null) {
			pi.text = state.ToString ();
		}
	}

	public static float DistancePointLine (Vector3 point, Vector3 lineStart, Vector3 lineEnd)
	{
		return (ProjectPointLine (point, lineStart, lineEnd) - point).magnitude;
	}
	
	public static Vector3 ProjectPointLine (Vector3 point, Vector3 lineStart, Vector3 lineEnd)
	{
		Vector3 rhs = point - lineStart;
		Vector3 vector2 = lineEnd - lineStart;
		float magnitude = vector2.magnitude;
		Vector3 lhs = vector2;
		if (magnitude > 1E-06f) {
			lhs = (Vector3)(lhs / magnitude);
		}
		float num2 = Mathf.Clamp (Vector3.Dot (lhs, rhs), 0f, magnitude);
		return (lineStart + ((Vector3)(lhs * num2)));
	}
	
	void OnDrawGizmos ()
	{
		
		Gizmos.color = Color.white;
		if (CurrentDestination != null)
			Gizmos.DrawLine (transform.position, CurrentDestination);

		if (speedDirectionBehavior) {
			Gizmos.color = Color.green;
			Gizmos.DrawLine (speedDirectionBehavior.Position, 
			                 speedDirectionBehavior.Position + speedDirectionBehavior.Direction * distanceArrival);
		}

		TextGizmo.Draw (transform.position + Vector3.up, state.ToString ());

	}


}
