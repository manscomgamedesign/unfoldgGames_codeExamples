using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;

/// <summary>
/// this will spawn the received rewards items on screen
/// </summary>
public class RewardItemsUISpawner : MonoBehaviour
{
    [SerializeField] private int maxItemsAmountInSpawner = 10; // max amount in each spawner
    [SerializeField] private float spawnRange = 250f; // for effect1, item assign on range location in this 250f circle range
    [SerializeField] private float itemsStayInSecond = 0.2f; // <- how long the items will stay on the screen before moving to destination
    [SerializeField] private float itemScaleEffectDuration = 0.2f; // duration of the scaling effect
    [SerializeField] private float itemsMoveSpeed = 300f; // how fast the items move to destination

    [SerializeField] private Vector3 itemStartScaleEffectSize = new Vector3(0.1f, 0.1f, 1.0f); // starting scale of the parent object
    [SerializeField] private Vector3 itemEndScaleEffectSize = new Vector3(1.0f, 1.0f, 1.0f); // ending scale of the parent object

    private RectTransform myTrans;
    private ObjectPool<GameObject> itemSpawner_ObjectPool;

    private void Awake()
    {
        myTrans = transform.GetComponent<RectTransform>();

        // create an Object pool for this effect spawner
        itemSpawner_ObjectPool = new ObjectPool<GameObject>(() =>
        {
            // this will create the 1 itemSpawner with 10 image child objects
            // the pool will gnereate this object structure for 1 insatnce
            var itemSpawner = new GameObject();
            itemSpawner.name = "RewardItem_Spawner";
            itemSpawner.transform.SetParent(transform);
            itemSpawner.transform.SetLocalPositionAndRotation(
                Vector3.zero,
                Quaternion.identity);
            itemSpawner.transform.localScale = Vector3.one;
            for (int i = 0; i < maxItemsAmountInSpawner; i++)
            {
                GameObject itemObj = new GameObject();
                itemObj.transform.SetParent(itemSpawner.transform);
                itemObj.transform.SetLocalPositionAndRotation(
                    Vector3.zero,
                    Quaternion.identity);
                itemObj.transform.localScale = Vector3.one;
                itemObj.AddComponent<Image>().raycastTarget = false;
            }
            return itemSpawner;
        },
        effect =>
        {
            effect.SetActive(true);
        },
        effect =>
        {
            effect.SetActive(false);
        },
        effect =>
        {
            Destroy(effect);
        },
        false,
        1, 10);
    }

    /// <summary>
    /// use to to spawn received reward items on screen
    /// </summary>
    public IEnumerator SpawnReceivedRewardsSpriteEffect(
        int spawnItemAmount,
        Sprite itemSprite,
        RectTransform moveTranformLocation)
    {
        // spawn reward group
        var spwnerObj = itemSpawner_ObjectPool.Get(); // <- instance

        // assign image on every item in the reward group
        for (int i = 0; i < spwnerObj.transform.childCount; i++)
        {
            var item = spwnerObj.transform.GetChild(i);
            item.GetComponent<Image>().sprite = itemSprite;
            item.transform.SetLocalPositionAndRotation(
                myTrans.anchoredPosition + UnityEngine.Random.insideUnitCircle * spawnRange,
                Quaternion.identity);
            item.transform.localScale = Vector3.one;
        }

        // Scale the parent object from 0.1 to 1 over time Effect
        float scaleStartTime = Time.time;
        while (Time.time - scaleStartTime < itemScaleEffectDuration)
        {
            float scaleSpeed = (Time.time - scaleStartTime) / itemScaleEffectDuration;
            spwnerObj.transform.localScale = Vector3.Lerp(
                itemStartScaleEffectSize,
                itemEndScaleEffectSize,
                scaleSpeed);

            yield return null;
        }

        // items stay on the scene for a while
        yield return new WaitForSeconds(itemsStayInSecond);

        // item moving Effect
        bool allItemsReachedDestination = false;
        while (!allItemsReachedDestination)
        {
            allItemsReachedDestination = true;
            for (int i = 0; i < spwnerObj.transform.childCount; i++)
            {
                var item = spwnerObj.transform.GetChild(i);
                var itemPos = item.GetComponent<RectTransform>();

                itemPos.anchoredPosition = Vector2.MoveTowards(itemPos.anchoredPosition, moveTranformLocation.anchoredPosition, itemsMoveSpeed);

                if (itemPos.anchoredPosition != moveTranformLocation.anchoredPosition)
                {
                    allItemsReachedDestination = false;
                    //break; // <- Effect if use break, item will move to the inventory one by one
                }
            }

            // release the pool when all items have reached destination
            if (allItemsReachedDestination)
            {
                itemSpawner_ObjectPool.Release(spwnerObj);
                break;
            }
            yield return null;
        }
    }
}
