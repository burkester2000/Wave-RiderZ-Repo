﻿/*-------------------------------------------------------------------*
|  Title:			BeachBall
|
|  Author:		    Thomas Maltezos / Seth Johnston
| 
|  Description:		Handles the beach ball's collision.
*-------------------------------------------------------------------*/

using UnityEngine;

public class BeachBall : MonoBehaviour
{
	public float freezeAmount = 0.05f;	//How long to freeze the game when the beachbomb pops
	public int freezeFrames = 20;		//The minimum amount of frames to freeze for

    private BeachBallAbility m_bbAbility;
    private Rigidbody m_rb;

	public GameObject explosionPrefab = null;	//Reference to the explosion effect to play when popping

    void Awake()
    {
        m_rb = gameObject.GetComponent<Rigidbody>();

        GameObject target = GameObject.FindWithTag("Target"); // Will search for the target with the tag.
        if (target != null)
            m_bbAbility = target.GetComponent<BeachBallAbility>(); // Will get the script from the target.

		Debug.Assert(explosionPrefab != null, "The explosion prefab hasn't been added to the beachball script");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("River")) // Will be called if collision with the river occurs.
        {
            Vector3 explosionPos = transform.position;	// explosion will occur at the impact site.
			explosionPos.y = 0;							//Make sure that there is no y component
            Collider[] colliders = Physics.OverlapSphere(explosionPos, m_bbAbility.radius); // List of colliders within the radius.
            AudioManager.Play("BeachBomb POP");

            foreach (Collider hit in colliders)										//For all the objects in the radius,
            {
				if (hit.CompareTag("Skier"))										//If this object is a skier,
				{
					Tether tether = hit.GetComponent<Tether>();						//Get their tether
					Vector3 distanceToHit = hit.transform.position - explosionPos;	//Get the difference in position between the skier and the explosion point
					distanceToHit.y = 0;                                            //Make sure there is no y compenent
					Vector3 extraForwardsForce = new Vector3(0, 0, m_bbAbility.extraForwardsPower);			//Make an extra force up the river to account for always moving forwards
					Vector3 totalForce = m_bbAbility.power * distanceToHit.normalized + extraForwardsForce;	//Total up the forces
					tether.ForceOverTime(totalForce, m_bbAbility.forceDuration);	//Add a force on the skier, pushing away from the explosion point

                }
                else if (hit.CompareTag("Mine"))
                {
                    Tether tether = hit.GetComponent<Tether>();                     //Get their tether
                    Vector3 distanceToHit = hit.transform.position - explosionPos;  //Get the difference in position between the skier and the explosion point
                    distanceToHit.y = 0;                                            //Make sure there is no y compenent
					Vector3 extraForwardsForce = new Vector3(0, 0, m_bbAbility.extraForwardsPower);				//Make an extra force up the river to account for always moving forwards
					Vector3 totalForce = m_bbAbility.minePower * distanceToHit.normalized + extraForwardsForce;	//Total up the forces
					tether.ForceOverTime(totalForce, m_bbAbility.forceDuration);	//Add a force on the mine, pushing away from the explosion point
                }
            }

            gameObject.SetActive(false); // Deactivates the beachball.
            m_rb.velocity = Vector3.zero; // Resets velocity.
            m_rb.angularVelocity = Vector3.zero; // Resets angular velocity.
            m_rb.transform.rotation = Quaternion.Euler(Vector3.zero); // Resets rotation.

            m_bbAbility.ToggleIsShooting(false); // Player isn't shooting anymore.
            m_bbAbility.ToggleMeshEnable(false); // Disabled target's mesh.

			explosionPrefab.transform.position = explosionPos;	//Make the explosion happen at the right spot
			Instantiate(explosionPrefab);						//Create the explosion

			ControllerVibrate.VibrateAll(0.2f, 0.5f);		//Vibrate all controllers a meduim amount

			GameFreezer.Freeze(freezeAmount, freezeFrames);	//Slow time very briefly for impact
        }
    }
}
