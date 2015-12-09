using UnityEngine;
using System.Collections;

public class LinkObject : MonoBehaviour
{
	
	//[HideInInspector]
	public Link link;
	//
	public NodeObject head;
	public NodeObject tail;
	
	void Start ()
	{
		if (head == null || tail == null) {
			Debug.Log ("Error@" + gameObject.name);
		}

		if (!head.connectedLinks.Contains (this)) {
			head.connectedLinks.Add (this);
		}
		
		if (!tail.connectedLinks.Contains (this)) {
			tail.connectedLinks.Add (this);
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		//Adjust ();
	}
	
	public void Adjust ()
	{
		
		if (link == null) {
			link = new Link ();
		}
		
		if (link.HeadNode == null || link.TailNode == null) {
			if (head == null || tail == null) {
				//print ("returning");
				return;
			} else {
				link.HeadNode = head.node;
				link.TailNode = tail.node;
			}
		}
		
		//print ("Adjust : " + name);
		
		Vector3 pos1 = link.HeadNode.Position;
		Vector3 pos2 = link.TailNode.Position;
		
		float d = Vector3.Distance (pos1, pos2);
		transform.localScale = new Vector3 (1, 1, d);
		transform.LookAt (pos2, Vector3.up);
		transform.position = pos1 + ((pos2 - pos1).normalized * d / 2);

		//gameObject.name = head.name + "-" + tail.name;
	}

	
}
