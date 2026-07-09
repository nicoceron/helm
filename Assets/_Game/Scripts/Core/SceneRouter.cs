using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lionrise
{
    public static class SceneRouter
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RouteBootScene()
        {
            if (SceneManager.GetActiveScene().name == "Boot" && Application.CanStreamedLevelBeLoaded("Game"))
                SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
        }
    }
}
