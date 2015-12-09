using UnityEngine;
using System.Collections;

namespace Shiva.PathPlanning{
	public abstract class PathPlanWorker : MonoBehaviour {

		private ExtendedPathPlanner planner;
		public ExtendedPathPlanner Planner{
			get{
				if(planner == null){
					planner = CreatePlanner();
					planner.CalcMinMax ();
				}

				return planner;
			}
		}
		
		public string displayName = "name here";
		public Color color = Color.green;

		/**
		 * This will be called when planner getter is called first time.
		 */
		public abstract ExtendedPathPlanner CreatePlanner ();

		/**
		 * This will be called soon before PathPlanner#Search called.
		 */
		public virtual void SetupForSeraching(){
		}


	}
}
