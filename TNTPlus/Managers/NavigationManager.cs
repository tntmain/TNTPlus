using System;
using System.Collections.Generic;
using System.Collections;
using Rocket.Unturned.Player;
using Rocket.Unturned.Events;
using SDG.Unturned;
using UnityEngine;
using TNTPlus.Config;
using TNTPlus.Managers;
using TNTPlus.Models;

namespace TNTPlus.Managers
{
    public class NavigationManager
    {
        private List<Vector3> roadsMap = new List<Vector3>();
        private List<Vector3> roadsCache = new List<Vector3>();
        private float SizeWorld = 0;
        private int Accuracy = 5;
        private float maxStepDistance = 100f;
        private float minPointDistance = 25f;
        private Dictionary<Vector2Int, List<Vector3>> roadGrid = new Dictionary<Vector2Int, List<Vector3>>();
        private int roadLayerMask;

        public NavigationManager()
        {
        }

        public void Initialize()
        {
            Level.onPostLevelLoaded += PostLevelLoaded;
            //UnturnedPlayerEvents.OnPlayerUpdateGesture += UnturnedPlayerEvents_OnPlayerUpdateGesture;
            roadLayerMask = RayMasks.GROUND | RayMasks.GROUND2 | RayMasks.TRANSPARENT_FX | RayMasks.CHART | RayMasks.STRUCTURE | RayMasks.STRUCTURE_INTERACT;
        }

        public void Unload()
        {
            Level.onPostLevelLoaded -= PostLevelLoaded;
            //UnturnedPlayerEvents.OnPlayerUpdateGesture -= UnturnedPlayerEvents_OnPlayerUpdateGesture;
        }

        private void UnturnedPlayerEvents_OnPlayerUpdateGesture(UnturnedPlayer player, UnturnedPlayerEvents.PlayerGesture gesture)
        {
            if (gesture == UnturnedPlayerEvents.PlayerGesture.PunchLeft)
            {
                EffCheckPoint(player);
            }
            if (gesture == UnturnedPlayerEvents.PlayerGesture.PunchRight)
            {
                Vector3 start = player.Position;
                Vector3 end = CheckPoint2(player.Player.quests.markerPosition.x, player.Player.quests.markerPosition.z);
                Main.Plugin.Instance.StartCoroutine(BidirectionalFindPathCoroutine(player, start, end));
            }
        }

        private IEnumerator BidirectionalFindPathCoroutine(UnturnedPlayer player, Vector3 start, Vector3 end)
        {
            Vector3 closestStart = FindClosestPoint(start, roadsMap);
            Vector3 closestEnd = FindClosestPoint(end, roadsMap);

            if (Vector3.Distance(start, closestStart) > maxStepDistance || Vector3.Distance(end, closestEnd) > maxStepDistance)
            {
                MessageManager.Say(player, "Старт или конец слишком далеко от дороги!", EMessageType.Error);
                yield break;
            }

            Dictionary<Vector3, Vector3> cameFromForward = new Dictionary<Vector3, Vector3>();
            Dictionary<Vector3, float> gScoreForward = new Dictionary<Vector3, float>();
            Dictionary<Vector3, float> fScoreForward = new Dictionary<Vector3, float>();
            List<Vector3> openSetForward = new List<Vector3> { closestStart };
            HashSet<Vector3> closedSetForward = new HashSet<Vector3>();

            Dictionary<Vector3, Vector3> cameFromBackward = new Dictionary<Vector3, Vector3>();
            Dictionary<Vector3, float> gScoreBackward = new Dictionary<Vector3, float>();
            Dictionary<Vector3, float> fScoreBackward = new Dictionary<Vector3, float>();
            List<Vector3> openSetBackward = new List<Vector3> { closestEnd };
            HashSet<Vector3> closedSetBackward = new HashSet<Vector3>();

            gScoreForward[closestStart] = 0;
            fScoreForward[closestStart] = Heuristic(closestStart, closestEnd);
            gScoreBackward[closestEnd] = 0;
            fScoreBackward[closestEnd] = Heuristic(closestEnd, closestStart);

            int maxIterations = 1000;
            int iteration = 0;

            while (openSetForward.Count > 0 && openSetBackward.Count > 0 && iteration < maxIterations)
            {
                Vector3 currentForward = GetLowestFScore(openSetForward, fScoreForward);
                openSetForward.Remove(currentForward);
                closedSetForward.Add(currentForward);

                List<Vector3> neighborsForward = GetNeighborsFromGrid(currentForward);
                foreach (var neighbor in neighborsForward)
                {
                    if (closedSetForward.Contains(neighbor)) continue;

                    float tentativeGScore = gScoreForward[currentForward] + Vector3.Distance(currentForward, neighbor);
                    if (!gScoreForward.ContainsKey(neighbor) || tentativeGScore < gScoreForward[neighbor])
                    {
                        cameFromForward[neighbor] = currentForward;
                        gScoreForward[neighbor] = tentativeGScore;
                        fScoreForward[neighbor] = gScoreForward[neighbor] + Heuristic(neighbor, closestEnd);

                        if (!openSetForward.Contains(neighbor))
                        {
                            openSetForward.Add(neighbor);
                        }
                    }

                    if (closedSetBackward.Contains(neighbor))
                    {
                        List<Vector3> path = CombinePaths(cameFromForward, cameFromBackward, neighbor, start, end);
                        MessageManager.Say(player, $"Маршрут построен! Точек в пути: {path.Count}", EMessageType.Succes);
                        roadsCache = path;
                        EffCheckPoint(player);
                        yield break;
                    }
                }

                Vector3 currentBackward = GetLowestFScore(openSetBackward, fScoreBackward);
                openSetBackward.Remove(currentBackward);
                closedSetBackward.Add(currentBackward);

                List<Vector3> neighborsBackward = GetNeighborsFromGrid(currentBackward);
                foreach (var neighbor in neighborsBackward)
                {
                    if (closedSetBackward.Contains(neighbor)) continue;

                    float tentativeGScore = gScoreBackward[currentBackward] + Vector3.Distance(currentBackward, neighbor);
                    if (!gScoreBackward.ContainsKey(neighbor) || tentativeGScore < gScoreBackward[neighbor])
                    {
                        cameFromBackward[neighbor] = currentBackward;
                        gScoreBackward[neighbor] = tentativeGScore;
                        fScoreBackward[neighbor] = gScoreBackward[neighbor] + Heuristic(neighbor, closestStart);

                        if (!openSetBackward.Contains(neighbor))
                        {
                            openSetBackward.Add(neighbor);
                        }
                    }

                    if (closedSetForward.Contains(neighbor))
                    {
                        List<Vector3> path = CombinePaths(cameFromForward, cameFromBackward, neighbor, start, end);
                        MessageManager.Say(player, $"Маршрут построен! Точек в пути: {path.Count}", EMessageType.Succes);
                        roadsCache = path;
                        EffCheckPoint(player);
                        yield break;
                    }
                }

                iteration++;
                if (iteration % 100 == 0) yield return null;
            }

            MessageManager.Say(player, "Не удалось построить маршрут! Возможно, путь слишком длинный или преграждён.", EMessageType.Error);
        }

        private Vector3 FindClosestPoint(Vector3 target, List<Vector3> points)
        {
            if (points.Count == 0) return target;
            Vector3 closest = points[0];
            float minDistance = Vector3.Distance(target, closest);

            foreach (var point in points)
            {
                float distance = Vector3.Distance(target, point);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = point;
                }
            }
            return closest;
        }

        private float Heuristic(Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b);
        }

        private Vector3 GetLowestFScore(List<Vector3> openSet, Dictionary<Vector3, float> fScore)
        {
            Vector3 lowest = openSet[0];
            float lowestScore = fScore.ContainsKey(lowest) ? fScore[lowest] : float.MaxValue;

            foreach (var point in openSet)
            {
                float score = fScore.ContainsKey(point) ? fScore[point] : float.MaxValue;
                if (score < lowestScore)
                {
                    lowest = point;
                    lowestScore = score;
                }
            }
            return lowest;
        }

        private List<Vector3> GetNeighborsFromGrid(Vector3 point)
        {
            List<Vector3> neighbors = new List<Vector3>();
            Vector2Int gridPos = new Vector2Int(Mathf.FloorToInt(point.x / maxStepDistance), Mathf.FloorToInt(point.z / maxStepDistance));
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Vector2Int checkPos = gridPos + new Vector2Int(x, y);
                    if (roadGrid.ContainsKey(checkPos))
                    {
                        foreach (var p in roadGrid[checkPos])
                        {
                            float distance = Vector3.Distance(point, p);
                            if (distance <= maxStepDistance && distance > minPointDistance && IsPathable(point, p))
                            {
                                neighbors.Add(p);
                            }
                        }
                    }
                }
            }
            return neighbors;
        }

        private bool IsPathable(Vector3 from, Vector3 to)
        {
            RaycastHit hit;
            Vector3 direction = (to - from).normalized;
            float distance = Vector3.Distance(from, to);

            if (Physics.Raycast(from + Vector3.up * 5, direction, out hit, distance, RayMasks.WATER | RayMasks.ENVIRONMENT))
            {
                return false;
            }
            return true;
        }

        private List<Vector3> CombinePaths(Dictionary<Vector3, Vector3> cameFromForward, Dictionary<Vector3, Vector3> cameFromBackward, Vector3 intersection, Vector3 start, Vector3 end)
        {
            List<Vector3> forwardPath = ReconstructPath(cameFromForward, intersection);
            forwardPath.Insert(0, start);

            List<Vector3> backwardPath = ReconstructPath(cameFromBackward, intersection);
            backwardPath.Reverse();
            backwardPath.RemoveAt(0);
            backwardPath.Add(end);

            forwardPath.AddRange(backwardPath);
            return forwardPath;
        }

        private List<Vector3> ReconstructPath(Dictionary<Vector3, Vector3> cameFrom, Vector3 current)
        {
            List<Vector3> totalPath = new List<Vector3> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Insert(0, current);
            }
            return totalPath;
        }

        public void PostLevelLoaded(int level)
        {
            SizeWorld = Level.size;
            Main.Plugin.Instance.StartCoroutine(GetInfoMapCoroutine());
            Debug.Log($"Высота карты: {Level.HEIGHT} | Размер: {Level.size}");
        }

        public void EffCheckPoint(UnturnedPlayer player)
        {
            foreach (Vector3 road in roadsCache)
            {
                EffectManager.sendEffect(125, player.CSteamID, road);
            }
        }

        private IEnumerator GetInfoMapCoroutine()
        {
            roadsMap.Clear();
            roadGrid.Clear();

            for (float x = -SizeWorld; x < SizeWorld; x += Accuracy)
            {
                for (float z = -SizeWorld; z < SizeWorld; z += Accuracy)
                {
                    Vector3 point = CheckPoint(x, z);
                    if (point != Vector3.zero && IsFarEnough(point, roadsMap))
                    {
                        roadsMap.Add(point);
                        AddToGrid(point);
                    }
                }
                yield return null;
            }
            Debug.Log("Кол-во точек на карте: " + roadsMap.Count);
        }

        private void AddToGrid(Vector3 point)
        {
            Vector2Int gridPos = new Vector2Int(Mathf.FloorToInt(point.x / maxStepDistance), Mathf.FloorToInt(point.z / maxStepDistance));
            if (!roadGrid.ContainsKey(gridPos))
                roadGrid[gridPos] = new List<Vector3>();
            roadGrid[gridPos].Add(point);
        }

        private bool IsFarEnough(Vector3 point, List<Vector3> existingPoints)
        {
            foreach (var existing in existingPoints)
            {
                if (Vector3.Distance(point, existing) < minPointDistance)
                {
                    return false;
                }
            }
            return true;
        }

        private Vector3 CheckPoint(float x, float z)
        {
            if (Physics.Raycast(new Vector3(x, 1024, z), Vector3.down, out var hit, 2048, roadLayerMask))
            {
                if (FindName(hit))
                {
                    return new Vector3(x, hit.point.y, z);
                }
            }
            return Vector3.zero;
        }

        private bool FindName(RaycastHit hit)
        {
            foreach (var item in Main.Plugin.Instance.Configuration.Instance.RoadNames)
            {
                if (hit.collider != null && hit.collider.name.StartsWith(item))
                {
                    return true;
                }
            }
            return false;
        }

        private Vector3 CheckPoint2(float x, float z)
        {
            if (Physics.Raycast(new Vector3(x, 1024, z), Vector3.down, out var hit, 2048, roadLayerMask))
            {
                return hit.point;
            }
            return Vector3.zero;
        }
    }
}