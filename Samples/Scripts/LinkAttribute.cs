using UnityEngine;
using System.Collections;

public class LinkAttribute : MonoBehaviour
{

	public string linkId;
	public float length;
	public float minWidth;
	public bool hasPedestrianRoad;
	public int numPedstriansWeekday;
	public int numPedstriansHoliday;
	//public bool hasRoof;
	public float roofCoverage;
	public int material;
	public int numHoles700;
	public int numHoles1400;
	public int numHolesMax;
	public int numGratings700;
	public int numGratings1400;
	public int numGratingsMax;
	public int numStuddedPavings;
	public int numLights700;
	public int numLights1400;
	public int numLightsMax;
	public int numElectricityBoxes700;
	public int numElectricityBoxes1400;
	public int numElectricityBoxesMax;
	public int numTrees700;
	public int numTrees1400;
	public int numTreesMax;
	public int numMovableObstacles700;
	public int numMovableObstacles1400;
	public int numMovableObstaclesMax;
	public int numStaticObstacles700;
	public int numStaticObstacles1400;
	public int numStaticObstaclesMax;
	public int numPowerPoles700;
	public int numPowerPoles1400;
	public int numPowerPolesMax;
	public int numSignPoles700;
	public int numSignPoles1400;
	public int numSignPolesMax;
	public int numUnMaintenanced700;
	public int numUnMaintenanced1400;
	public int numUnMaintenancedMax;

	//old things -these should be deleted soon
	public float bicepsBrachii;
	public float tricepsBrachii;
	public float deltoidAnterior;
	public float deltoidMiddle;
	public float deltoidPosterior;
	public float brachioradialis;

	public float rightBicepsBrachii;
	public float rightTricepsBrachii;
	public float rightDeltoid;
	public float rightBrachioradialis;

	public float leftBicepsBrachii;
	public float leftTricepsBrachii;
	public float leftDeltoid; 
	public float leftBrachioradialis;

	public float goProTime;

	public bool stepUp;
	public bool stepDown;
	public float vibrationRate;

	public string imageId;
	//[HideInInspector]
	public Texture2D image;
	
	public bool crossGrade;
	public float crossDegree;
	public int crossType;
	public int inclineDirection;
	public bool carRode;
	public bool parkingOnStreet;
	public bool vibration;

	public string[] values;

	void Start(){
		image = ImageMaster.Instance.Get(imageId);
	}

	public string latestLogicalDistance;


}
