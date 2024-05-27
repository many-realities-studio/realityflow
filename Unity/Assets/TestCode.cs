using UnityEngine;
using System.Collections;
using StarterAssets; 

public class LoadPlayerArmature : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(LoadPlayerArmatureCoroutine());
    }

    private IEnumerator LoadPlayerArmatureCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            GameObject playerArmaturePrefab = Resources.Load<GameObject>("PlayerArmature");
            GameObject playerArmatureInstance = Instantiate(playerArmaturePrefab);

            StarterAssetsInputs starterInputs = playerArmatureInstance.GetComponent<StarterAssetsInputs>();

            starterInputs.move = new Vector2(1.0f, 0);
            starterInputs.jump = true;
        }
    }
}