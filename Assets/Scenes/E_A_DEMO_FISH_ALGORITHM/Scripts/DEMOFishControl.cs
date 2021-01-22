using System;
using System.Collections;
using System.Collections.Generic;
using Packages.Rider.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace E_A_DEMO_FISH_ALGORITHM
{
    public class DEMOFishControl : MonoBehaviour
    {
        public GameObject MonoObjs;
        public GameObject ECSObjs;
        public FishSpawnerData fishSpawnerData;
        public List<GameObject> needHides = new List<GameObject>();
        public InputField inputVal;

        public void ClickBeginMonoBtn()
        {
            int num = GetNumsFromInput();
            if (num > 0)
            {
                fishSpawnerData.GenerateFishNums = num;
                MonoObjs.SetActive(true);
                HideBtns();
            }
        }

        public void ClickBeginECSBtn()
        {
            int num = GetNumsFromInput();
            if (num > 0)
            {
                fishSpawnerData.GenerateFishNums = num;
                ECSObjs.SetActive(true);
                HideBtns();
            }
        }

        private Int32 GetNumsFromInput()
        {
            try
            {
                Int32 nums = Convert.ToInt32(inputVal.text);
                return nums;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }

        private void HideBtns()
        {
            for (int i = 0; i < needHides.Count; i++)
            {
                Destroy(needHides[i]);
            }
        }
    }
}
