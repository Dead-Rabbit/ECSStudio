using UnityEngine;

namespace Scenes.E_A_MONO_FISH_ALGORITHM
{
    public class FishSpawner : MonoBehaviour
    {
        public GameObject spawnerFish;
        public GameObject spawnerRoot;
        public float GenerateRange = 10;
        public int GenerateCount = 10;
        public float FishMinSpeed = 40;
        public float FishMaxSpeed = 55;

        public void Start()
        {
            if (null == spawnerRoot || null == spawnerFish)
            {
                return;
            }

            for (int i = 0; i < GenerateCount; i++)
            {
                GameObject newFish = Instantiate(spawnerFish, spawnerRoot.transform, true);
                newFish.transform.name = "Fish_" + i;
                newFish.transform.position = new Vector3(Random.Range(-GenerateRange, GenerateRange),
                    Random.Range(-GenerateRange, GenerateRange),
                    Random.Range(-GenerateRange, GenerateRange));
                newFish.GetComponent<GroupMember>().moveSpeed = Random.Range(FishMinSpeed, FishMaxSpeed);
                newFish.transform.rotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
            }
        }
    }
}
