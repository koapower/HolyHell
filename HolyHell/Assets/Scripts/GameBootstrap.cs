using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    private void Awake()
    {
        GameRuntime.Init().Forget();
        Destroy(gameObject);
    }

}
