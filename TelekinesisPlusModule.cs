using System;
using ThunderRoad;
using UnityEngine;

namespace TelekinesisPlus
{
    // This create an level module that can be referenced in the level JSON
    public class TelekinesisPlusScript : ThunderScript
    {
        public static ModOptionBool[] booleanOption = {
            new ModOptionBool("Disabled", false),
            new ModOptionBool("Enabled", true)
        };

        public static int floatOptionIndex(float minValue, float step, float defaultValue)
        {
            return (int)((defaultValue - minValue) / step);
        }

        public static ModOptionFloat[] reachOption = ModOptionFloat.CreateArray(0.01f, 3.00f, 0.01f);

        public static ModOptionFloat[] forceMultiplierOption = ModOptionFloat.CreateArray(1, 1000, 0.1f);

        public static ModOptionFloat[] maxDistanceOption = ModOptionFloat.CreateArray(0.1f, 50, 0.1f);

        public SpellCaster rightTele;
        public SpellCaster leftTele;

        public LineRenderer leftLine = null;
        public LineRenderer rightLine = null;

        [ModOptionSlider]
        [ModOptionSave]
        [ModOption(name: "Maximum Reach", tooltip: "Maximum distance from shoulder to hand", valueSourceName: nameof(reachOption), defaultValueIndex = (int) ((0.65f - 0.01f) / 0.01f))]
        public static float maxReach = 0.65f;
        [ModOptionSlider]
        [ModOptionSave]
        [ModOption(name: "Minimum Reach", tooltip: "Minimum distance from shoulder to hand", valueSourceName: nameof(reachOption), defaultValueIndex = (int) ((0.27f - 0.01f) / 0.01f))]
        public static float minReach = 0.27f;

        public float reach = 0f;

        public bool justCatchedRight = false;
        public bool justCatchedLeft = false;

        [ModOptionSlider]
        [ModOptionSave]
        [ModOption(name: "Force Multiplier", tooltip: "Multiply against the default telekinesis force", valueSourceName: nameof(forceMultiplierOption), defaultValueIndex = (int) ((300f - 0.1f) / 0.1f))]
        public static float forceMultiplier = 300;

        private Vector3[] direction = new Vector3[2];

        [ModOptionSlider]
        [ModOptionSave]
        [ModOption(name: "Maximum Distance", tooltip: "How far away an item can be held", valueSourceName: nameof(maxDistanceOption), defaultValueIndex = (int) ((15f - 0.1f) / 0.1f))]
        public static float maxDistanceStatic = 15f;
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

        [ModOptionButton]
        [ModOptionSave]
        [ModOption(name: "Make Lines (not working)", tooltip: "Draws lines depicting telekinesis targets. Currently seems to be broken.", valueSourceName: nameof(booleanOption))]
        public static bool linesActive = false;
        public bool linesCreated = false;
        public bool initialized = false;

        public EventManager.PossessEvent possessEvent;

        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData);
            possessEvent = new EventManager.PossessEvent(EventManager_onPossessEvent);
            Debug.Log("TelekinesisPlus Loaded");
        }

        public override void ScriptEnable()
        {
            base.ScriptEnable();
            EventManager.onPossess += EventManager_onPossessEvent;
            Debug.Log("TelekinesisPlus Enabled");
        }

        public override void ScriptDisable()
        {
            base.ScriptDisable();
            initialized = false;
            EventManager.onPossess -= EventManager_onPossessEvent;
            Debug.Log("TelekinesisPlus Disabled");
        }

        private void EventManager_onPossessEvent(Creature creature, EventTime eventTime)
        {
            if (creature.player != null && creature.player.creature != null)
            {
                Initialize(creature.player.creature);
                initialized = true;
                Debug.Log("TelekinesisPlus Initialized");
            }
        }

        public override void ScriptFixedUpdate()
        {
            base.ScriptFixedUpdate();
            if (initialized)
            {
                if (linesActive)
                {
                    if (!linesCreated) MakeLines();
                }
                else if (linesCreated) RemoveLines();

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

                tele.telekinesis.catchedHandle.physicBody.AddForce((point - tele.telekinesis.catchedHandle.transform.position) * forceMultiplier, ForceMode.Force);
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

            linesCreated = true;
        }

        private void RemoveLines()
        {
            GameObject.Destroy(leftLine);
            GameObject.Destroy(rightLine);
            leftLine = null;
            rightLine = null;

            linesCreated = false;
        }

        public void Initialize(Creature player)
        {
            maxDistance = maxDistanceStatic;
            neck = player.animator.GetBoneTransform(HumanBodyBones.Neck);

            // Left telekinesis
            leftTele = player.handLeft.caster;
            leftTele.telekinesis.pullAndRepelMaxSpeed = 0f;
            leftTele.telekinesis.positionSpring = 0f;
            leftElbow = player.animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            leftShoulder = player.animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
            leftFinger = player.animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal);

            // Right telekinesis
            rightTele = player.handRight.caster;
            rightTele.telekinesis.pullAndRepelMaxSpeed = 0f;
            rightTele.telekinesis.positionSpring = 0f;
            rightElbow = player.animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            rightShoulder = player.animator.GetBoneTransform(HumanBodyBones.RightShoulder);
            rightFinger = player.animator.GetBoneTransform(HumanBodyBones.RightIndexDistal);

            if (linesActive) MakeLines();
        }

    }
}
