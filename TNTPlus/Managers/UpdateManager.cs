using System.Collections;
using UnityEngine;
using Action = System.Action;

namespace TNTPlus.Managers
{
    public class UpdateManager
    {
        public static event Action OnSecondTick;

        public static IEnumerator SecondTickRoutine()
        {
            while (true)
            {
                OnSecondTick?.Invoke();
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
