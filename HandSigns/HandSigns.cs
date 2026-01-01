using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using ThunderRoad;
using UnityEngine;
using static ThunderRoad.ModManager;
namespace HandSigns
{
    public class HandPoseLoader : ThunderScript
    {
        private string folderPath;
        public static List<ModOptionString> handSigns;
        public static ModOptionString[] handPoses;
        private ModData modData;
        public override void ScriptLoaded(ModData modData)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                folderPath = Path.Combine(Application.persistentDataPath, "Mods", "HandSigns", "HandPoses");
            }
            else
            {
                folderPath = Path.Combine(Application.streamingAssetsPath, "Mods", "HandSigns", "HandPoses");
            }
            this.modData = modData;
            EventManager.onLevelLoad += EventManager_onLevelLoad;
        }

        private void EventManager_onLevelLoad(LevelData levelData, LevelData.Mode mode, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart) return;
            UpdateLoadedHandSigns();
        }

        public void UpdateLoadedHandSigns()
        {
            handSigns = new List<ModOptionString>();

            string[] files = Directory.GetFiles(folderPath, "*.json");
            foreach (string file in files)
            {
                string json = File.ReadAllText(file);

                HandPoseData pose = JsonConvert.DeserializeObject<HandPoseData>(json);
                handSigns.Add(new ModOptionString(pose.id, pose.id));
                Debug.Log($"Loaded HandPose: {pose.id}");
            }
            handPoses = handSigns.ToArray();
            foreach(ModOption modOption in modData.modOptions)
            {
                modOption.origParameterValues = handPoses;
                modOption.parameterValues = handPoses;
                modOption.Reload();
                modOption.Apply(modOption.currentValueIndex);
            }
        }
    }
    public class HandSignsSpell : SpellCastCharge
    {
        [ModOption("Trigger (R)", valueSourceName = nameof(HandPoseLoader.handPoses), defaultValueIndex = 1, category = "Right Hand")]
        private static string triggerRightPose = null;

        [ModOption("Trigger + Grip (R)", valueSourceName = nameof(HandPoseLoader.handPoses), defaultValueIndex = 3, category = "Right Hand")]
        private static string triggerRightGripPose = null;

        [ModOption("Trigger + Alt Use (R)", valueSourceName = nameof(HandPoseLoader.handPoses), defaultValueIndex = 4, category = "Right Hand")]
        private static string triggerRightAltUsePose = null;

        [ModOption("Trigger (L)", valueSourceName = nameof(HandPoseLoader.handPoses), defaultValueIndex = 1, category = "Left Hand")]
        private static string triggerLeftPose = null;

        [ModOption("Trigger + Grip (L)", valueSourceName = nameof(HandPoseLoader.handPoses), defaultValueIndex = 3, category = "Left Hand")]
        private static string triggerLeftGripPose = null;

        [ModOption("Trigger + Alt Use (L)", valueSourceName = nameof(HandPoseLoader.handPoses), defaultValueIndex = 4, category = "Left Hand")]
        private static string triggerLeftAltUsePose = null;

        public override void Load(SpellCaster spellCaster)
        {
            base.Load(spellCaster);
            Player.currentCreature.GetHand(spellCaster.ragdollHand.side).playerHand.controlHand.OnButtonPressEvent += ButtonPressedEvent;
        }
        public override void Unload()
        {
            base.Unload();
            Player.currentCreature.GetHand(spellCaster.ragdollHand.side).playerHand.controlHand.OnButtonPressEvent -= ButtonPressedEvent;
        }

        private void ButtonPressedEvent(PlayerControl.Hand.Button button, bool pressed)
        {
            if (button == PlayerControl.Hand.Button.Grip && pressed && triggerHeld)
            {
                SetPose(Catalog.GetData<HandPoseData>(spellCaster.ragdollHand.side == ThunderRoad.Side.Right ? triggerRightGripPose : triggerLeftGripPose));
            }
            if (button == PlayerControl.Hand.Button.AlternateUse && pressed && triggerHeld)
            {
                SetPose(Catalog.GetData<HandPoseData>(spellCaster.ragdollHand.side == ThunderRoad.Side.Right ? triggerRightAltUsePose : triggerLeftAltUsePose));
            }
        }

        private bool triggerHeld = false;

        public override void Fire(bool active)
        {
            base.Fire(active);
            triggerHeld = active;
            if (active)
            {
                spellCaster.DisableSpellWheel(this);
                SetPose(Catalog.GetData<HandPoseData>(spellCaster.ragdollHand.side == ThunderRoad.Side.Right ? triggerRightPose : triggerLeftPose));
            }
            else
            {
                spellCaster.AllowSpellWheel(this);
            }
        }

        private void SetPose(HandPoseData data)
        {
            spellCaster.ragdollHand.poser.SetTargetPose(data);
            spellCaster.ragdollHand.poser.SetDefaultPose(data);
        }
    }
}