using System.Collections.Generic;
using UnityEngine;

namespace TNTPlus.Models
{
    public class NavigationPath
    {
        public List<Vector3> Points { get; set; }

        public NavigationPath(List<Vector3> points)
        {
            Points = points;
        }
    }
}