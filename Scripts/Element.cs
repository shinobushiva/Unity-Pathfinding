using System;
using System.Collections;
using System.Collections.Generic;

/**
 * 経路ネットワーク構成要素である事を示すクラスです。
 * 
 * @author Shinobu Izumi (Kyushu Institute of Technology)
 */
[System.Serializable]
public abstract class Element
{

	public string id;
	
	public string Id {
		get {
			return id;
		}
		set {
			id = value;
		}
	}

	public override string ToString(){
		return id;
	}
}
