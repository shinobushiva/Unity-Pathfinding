using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NodeObject : ABehavior
{
	//[HideInInspector]
	public Node node;
	public string id;
	public List<LinkObject> connectedLinks = new List<LinkObject> ();

	// Update is called once per frame
	void Update ()
	{
		//Adjust ();
		
	}
	
	public void Adjust ()
	{
		//print ("Adjust : " + name);
		
		if (node == null) {
			node = new Node ();
		}

		node.Id = id;
		node.Position = transform.position;
	}

	
}
