using UnityEngine;
using BS;

namespace TelekinesisPlus
{
    // This create an level module that can be referenced in the level JSON
    public class TelekinesisPlusModule : LevelModule
    {

        public Telekinesis rightTele;
        public Telekinesis leftTele;

        public LineRenderer leftLine = null;
        public LineRenderer rightLine = null;

        public float maxReach = 0.65f;
        public float minReach = 0.27f;

        public float reach;

        public bool justCatchedRight = false;
        public bool justCatchedLeft = false;

        public float forceMultiplier = 300;

        private Vector3[] direction = new Vector3[2];

        public float maxDistance;
        public float maxDistanceStatic = 15f;

        public Handle rightHandle;
        public Handle leftHandle;

        public Transform neck;
        public Transform rightElbow;
        public Transform leftElbow;
        public Transform rightShoulder;
        public Transform leftShoulder;
        public Transform leftFinger;
        public Transform rightFinger;


        public Vector3 point;

        public override void OnLevelLoaded(LevelDefinition levelDefinition)
        {
            
        }


        public override void Update(LevelDefinition levelDefinition)
        {
            if(Creature.player && Player.local && !initialized)
            {
                initialized = true;
                Initialize();
                                                          
            }

            if (!Creature.player)
            {
                initialized = false;
            }

            if (initialized)
            {                              
                if (rightTele)
                {
                    justCatchedRight = TeleStuff(rightTele, rightElbow, rightShoulder, justCatchedRight, rightHandle, rightFinger);
                }

                if (leftTele)
                {
                    justCatchedLeft = TeleStuff(leftTele, leftElbow, leftShoulder, justCatchedLeft, leftHandle, leftFinger);
                }
            }
        }

        public bool TeleStuff(Telekinesis tele, Transform elbow, Transform shoulder, bool justCatched, Handle teleHandle, Transform finger)
        {
            reach = Vector3.Distance(tele.transform.position, shoulder.position);

            if (reach < minReach)
            {
                point = tele.transform.position;
            }
            else
            {
                point = tele.transform.position + (finger.position - elbow.position).normalized * ((reach - minReach) / (maxReach - minReach)) * maxDistance;
            }

            /*
            direction[0] = finger.position;
            direction[1] = point;

            if (tele.transform.position != point)
            {
                line.SetPositions(direction);
            }*/

            if (maxDistance != maxDistanceStatic)
            {
                maxDistance = Mathf.Lerp(maxDistance, maxDistanceStatic, Time.deltaTime / 1.75f);
            }

            if (tele.catchedHandle)
            {
                if (!justCatched)
                {
                    maxDistance = Vector3.Distance(tele.catchedHandle.transform.position, tele.transform.position) * (maxReach - minReach) / (reach - minReach);
                    justCatched = true;
                    teleHandle = tele.catchedHandle;
                  
                }

                

                tele.catchedHandle.rb.AddForce((point - tele.catchedHandle.transform.position) * forceMultiplier, ForceMode.Force);


            }
            else
            {
                justCatched = false;
                if (teleHandle)
                {
                    
                    teleHandle = null;
                }

            }

            return justCatched;
        }

        public void Initialize()
        {
            maxDistance = maxDistanceStatic;
            neck = Creature.player.ragdoll.GetPart(HumanBodyBones.Neck).transf;


            leftTele = Player.local.handLeft.bodyHand.telekinesis;            
            leftTele.pullAndRepelMaxSpeed = 0f;       
            leftTele.positionSpring = 0f;           
            leftElbow = Creature.player.animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);            
            leftShoulder = Creature.player.animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
            leftFinger = Creature.player.animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
                                    
            /*leftLine = leftTele.gameObject.AddComponent<LineRenderer>();
            leftLine.receiveShadows = false;
            leftLine.material.color = Color.white;
            leftLine.startWidth = 0.005f;
            leftLine.endWidth = 0.005f;            
            leftLine.startColor = Color.white;
            leftLine.endColor = Color.white*/




            rightTele = Player.local.handRight.bodyHand.telekinesis;
            rightTele.pullAndRepelMaxSpeed = 0f;
            rightTele.positionSpring = 0f;
            rightElbow = Creature.player.animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            rightShoulder = Creature.player.animator.GetBoneTransform(HumanBodyBones.RightShoulder);
            rightFinger = Creature.player.animator.GetBoneTransform(HumanBodyBones.RightIndexDistal);

            /*rightLine = rightTele.gameObject.AddComponent<LineRenderer>();
            rightLine.receiveShadows = false;
            rightLine.material.color = Color.white;
            rightLine.startWidth = 0.005f;
            rightLine.endWidth = 0.005f;
            rightLine.startColor = Color.white;
            rightLine.endColor = Color.white;*/            
        }

    }
}
