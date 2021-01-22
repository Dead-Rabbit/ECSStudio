using UnityEngine;

namespace Scenes.E_A_MONO_FISH_ALGORITHM
{
    public class FishSpawner : MonoBehaviour
    {
        public GameObject spawnerFish;
        public GameObject spawnerRoot;

        public float GenerateRange = 10;

        private int GenerateCount = 10;

        public void Start()
        {
            if (null == spawnerRoot || null == spawnerFish)
            {
                return;
            }

            for (int i = 0; i < GenerateCount; i++)
            {
                GameObject newFish = Instantiate(spawnerFish);
                newFish.transform.parent = spawnerRoot.transform;
                newFish.transform.name = "Fish_" + i;
            }
        }
    }
}
