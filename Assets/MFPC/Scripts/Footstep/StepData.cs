using UnityEngine;

using System;
using System.Collections.Generic;

namespace MFPC
{
    /// <summary>
    /// Textures on which the sound of steps will be played
    /// </summary>
    [CreateAssetMenu(fileName = "StepData", menuName = "MFPC/StepData")]
    public class StepData : ScriptableObject, ISerializationCallbackReceiver, IComparable
    {
        [SerializeField, Tooltip("Textures on which the sounds of steps will be played")]
        private Texture[] textures;

        [SerializeField, Utils.CenterHeader("StepsSFX")]
        private AudioClip[] stepSFX;

        public HashSet<Texture> TextureHashSet { get; private set; }
        public Texture[] GetTextures => textures;

        private int previousStepIndex = 0;

        /// <summary>
        /// Getting the sound of a step
        /// </summary>
        /// <returns>Sound of a step</returns>
        public AudioClip GetStepSFX()
        {
            return stepSFX[GetNewRandomValue()];
        }

        /// <summary>
        /// Checks if the texture we need is in StepData
        /// </summary>
        /// <param name="targetTexture">The texture we are looking for</param>
        /// <returns>True if there is a texture match</returns>
        public bool CompareTexture(Texture targetTexture)
        {
            return TextureHashSet.Contains(targetTexture);
        }

        public int CompareTo(object obj)
        {
            if (obj == null || obj.Equals(this)) return 1;

            StepData stepData = (StepData)obj;

            if (this.GetTextures == null || stepData.GetTextures == null) return 1;

            HashSet<Texture> intersection = new HashSet<Texture>(this.TextureHashSet);
            intersection.IntersectWith(stepData.TextureHashSet);

            if (intersection.Count > 0)
            {
                Debug.LogError("In (" + this.name + " => " + stepData.name + ") repeating textures");
            }

            return 0;
        }

        private int GetNewRandomValue()
        {
            int currentValue = UnityEngine.Random.Range(0, stepSFX.Length);

            if (currentValue == previousStepIndex)
            {
                return GetNewRandomValue();
            }

            previousStepIndex = currentValue;
            return currentValue;
        }

        #region SERIALIZATION

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            StepData[] stepData = Resources.LoadAll<StepData>("StepsData") as StepData[];

            Array.ForEach(stepData, x =>
                {
                    foreach (StepData item in stepData)
                    {
                        x.CompareTo(item);
                    }
                });
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.TextureHashSet = new HashSet<Texture>();

            foreach (Texture item in this.textures)
            {
                if (item != null) this.TextureHashSet.Add(item);
            }
        }

        #endregion
    }
}