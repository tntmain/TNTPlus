using System;
using System.Collections;
using UnityEngine;

namespace TNTPlus.Managers
{
    public class UpdateManager
    {
        public event Action OnSecondTick;

        public IEnumerator SecondTickRoutine()
        {
            while (true)
            {
                OnSecondTick?.Invoke();
                yield return new WaitForSeconds(1f);
            }
        }
    }
}