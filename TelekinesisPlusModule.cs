using UnityEngine;
using ThunderRoad;

namespace TelekinesisPlus
{
    // This create an level module that can be referenced in the level JSON
    public class TelekinesisPlusModule : LevelModule
    {

        public SpellCaster rightTele;
        public SpellCaster leftTele;

        public LineRenderer leftLine = null;
        public LineRenderer rightLine = null;

        public float maxReach = 0.65f;
        public float minReach = 0.27f;

        public float reach = 0f;

        public bool justCatchedRight = false;
        public bool justCatchedLeft = false;

        public float forceMultiplier = 300;

        private Vector3[] direction = new Vector3[2];

        public float maxDistanceStatic = 15f;
        public float maxDistance;
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

        public bool linesActive = false;

        public override void OnLevelLoaded(LevelDefinition levelDefinition)
        {
            Debug.Log("TelekinesisPlus Loaded");
            base.OnLevelLoaded(levelDefinition);
            EventManager.onPossessionEvent += new EventManager.PossessionEvent(EventManager_onPossessionEvent);
        }

        private void EventManager_onPossessionEvent(Body oldBody, Body newBody)
        {
            if (!newBody)
            {
                initialized = false;
                return;
            }

            initialized = true;
            Initialize();
        }
        public override void Update(LevelDefinition levelDefinition)
        {
            if (initialized)
            {
                if (rightTele != null)
                {
                    justCatchedRight = TeleStuff(rightTele, rightElbow, rightShoulder, justCatchedRight, rightHandle, rightFinger, rightLine);
                }

                if (leftTele != null)
                {
                    justCatchedLeft = TeleStuff(leftTele, leftElbow, leftShoulder, justCatchedLeft, leftHandle, leftFinger, leftLine);
                }
            }
        }

        public bool TeleStuff(SpellCaster tele, Transform elbow, Transform shoulder, bool justCatched, Handle teleHandle, Transform finger, LineRenderer line)
        {
            justCatched = TeleStuff(tele, elbow, shoulder, justCatched, teleHandle, finger);
            if (linesActive)
            {
                direction[0] = finger.position;
                direction[1] = point;

                if (tele.transform.position != point)
                {
                    line.SetPositions(direction);
                }
            }

            return justCatched;
        }
        public bool TeleStuff(SpellCaster tele, Transform elbow, Transform shoulder, bool justCatched, Handle teleHandle, Transform finger)
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

            if (maxDistance != maxDistanceStatic)
            {
                maxDistance = Mathf.Lerp(maxDistance, maxDistanceStatic, Time.deltaTime / 2f);
            }

            if (tele.telekinesis.catchedHandle)
            {
                if (!justCatched)
                {
                    maxDistance = Vector3.Distance(tele.telekinesis.catchedHandle.transform.position, tele.transform.position) * (maxReach - minReach) / (reach - minReach);
                    justCatched = true;
                    teleHandle = tele.telekinesis.catchedHandle;

                }

                tele.telekinesis.catchedHandle.rb.AddForce((point - tele.telekinesis.catchedHandle.transform.position) * forceMultiplier, ForceMode.Force);
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

        private void MakeLines()
        {
            leftLine = leftTele.gameObject.AddComponent<LineRenderer>();
            leftLine.receiveShadows = false;
            leftLine.material.color = Color.white;
            leftLine.startWidth = 0.005f;
            leftLine.endWidth = 0.005f;
            leftLine.startColor = Color.white;
            leftLine.endColor = Color.white;

            rightLine = rightTele.gameObject.AddComponent<LineRenderer>();
            rightLine.receiveShadows = false;
            rightLine.material.color = Color.white;
            rightLine.startWidth = 0.005f;
            rightLine.endWidth = 0.005f;
            rightLine.startColor = Color.white;
            rightLine.endColor = Color.white;
        }

        public void Initialize()
        {
            maxDistance = maxDistanceStatic;
            neck = Creature.player.animator.GetBoneTransform(HumanBodyBones.Neck);

            // Left telekinesis
            leftTele = Player.local.handLeft.bodyHand.caster;
            leftTele.telekinesis.pullAndRepelMaxSpeed = 0f;       
            leftTele.telekinesis.positionSpring = 0f;           
            leftElbow = Creature.player.animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);            
            leftShoulder = Creature.player.animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
            leftFinger = Creature.player.animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
            
            // Right telekinesis
            rightTele = Player.local.handRight.bodyHand.caster;
            rightTele.telekinesis.pullAndRepelMaxSpeed = 0f;
            rightTele.telekinesis.positionSpring = 0f;
            rightElbow = Creature.player.animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            rightShoulder = Creature.player.animator.GetBoneTransform(HumanBodyBones.RightShoulder);
            rightFinger = Creature.player.animator.GetBoneTransform(HumanBodyBones.RightIndexDistal);

            if (linesActive) MakeLines();
        }

    }
}
