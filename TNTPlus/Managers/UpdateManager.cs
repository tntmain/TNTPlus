using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
