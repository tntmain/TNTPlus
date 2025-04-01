using System;
using System.Collections;
using System.Collections.Generic;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using TNTPlus.Models;

namespace TNTPlus.Managers
{
    public class NavigationManager
    {
        private List<Vector3> roadPoints = new List<Vector3>();
        private List<Vector3> currentRoute = new List<Vector3>();
        private float worldSize = 0;
        private int scanAccuracy = 5;
        private float maxDistanceToRoad = 100f;
        private float minDistanceBetweenPoints = 25f;
        private Dictionary<Vector2Int, List<Vector3>> roadGrid = new Dictionary<Vector2Int, List<Vector3>>();
        private int roadLayerMask;
        private Vector3 lastPlayerPosition = Vector3.zero;
        private bool isRouteActive = false;

        public bool IsInitialized { get; private set; }

        public NavigationManager()
        {
        }

        public void Initialize()
        {
            if (IsInitialized)
            {
                Rocket.Core.Logging.Logger.LogWarning("NavigationManager уже инициализирован.");
                return;
            }

            Level.onPostLevelLoaded += OnLevelLoaded;
            roadLayerMask = RayMasks.GROUND | RayMasks.GROUND2 | RayMasks.TRANSPARENT_FX | RayMasks.CHART | RayMasks.STRUCTURE | RayMasks.STRUCTURE_INTERACT;
            IsInitialized = true;
            Rocket.Core.Logging.Logger.Log("NavigationManager инициализирован.");
        }

        public void Unload()
        {
            if (!IsInitialized)
                return;

            Level.onPostLevelLoaded -= OnLevelLoaded;
            isRouteActive = false;
            currentRoute.Clear();
            roadPoints.Clear();
            roadGrid.Clear();
            IsInitialized = false;
            Rocket.Core.Logging.Logger.Log("NavigationManager выгружен.");
        }

        public void BuildRoute(UnturnedPlayer player, Vector3 start, Vector3 end, Action<bool, string> callback = null)
        {
            if (!IsInitialized)
            {
                callback?.Invoke(false, "NavigationManager не инициализирован.");
                return;
            }

            if (roadPoints.Count == 0)
            {
                callback?.Invoke(false, "Карта дорог не загружена. Попробуйте позже.");
                return;
            }

            Main.TNTPlus.Core.StartCoroutine(BuildRouteCoroutine(player, start, GetSurfacePoint(end.x, end.z), callback));
        }

        public void CancelRoute(UnturnedPlayer player, Action<string> callback = null)
        {
            if (isRouteActive)
            {
                isRouteActive = false;
                currentRoute.Clear();
                string message = "Маршрут отменён.";
                callback?.Invoke(message);
            }
        }

        private IEnumerator BuildRouteCoroutine(UnturnedPlayer player, Vector3 start, Vector3 end, Action<bool, string> callback)
        {
            Vector3 closestStart = FindNearestRoadPoint(start, roadPoints);
            Vector3 closestEnd = FindNearestRoadPoint(end, roadPoints);

            if (Vector3.Distance(start, closestStart) > maxDistanceToRoad || Vector3.Distance(end, closestEnd) > maxDistanceToRoad)
            {
                string message = "Начало или конец слишком далеко от дороги!";
                callback?.Invoke(false, message);
                yield break;
            }

            Dictionary<Vector3, Vector3> forwardPath = new Dictionary<Vector3, Vector3>();
            Dictionary<Vector3, float> forwardGScore = new Dictionary<Vector3, float>();
            Dictionary<Vector3, float> forwardFScore = new Dictionary<Vector3, float>();
            List<Vector3> forwardOpenSet = new List<Vector3> { closestStart };
            HashSet<Vector3> forwardClosedSet = new HashSet<Vector3>();

            Dictionary<Vector3, Vector3> backwardPath = new Dictionary<Vector3, Vector3>();
            Dictionary<Vector3, float> backwardGScore = new Dictionary<Vector3, float>();
            Dictionary<Vector3, float> backwardFScore = new Dictionary<Vector3, float>();
            List<Vector3> backwardOpenSet = new List<Vector3> { closestEnd };
            HashSet<Vector3> backwardClosedSet = new HashSet<Vector3>();

            forwardGScore[closestStart] = 0;
            forwardFScore[closestStart] = EstimateDistance(closestStart, closestEnd);
            backwardGScore[closestEnd] = 0;
            backwardFScore[closestEnd] = EstimateDistance(closestEnd, closestStart);

            int maxIterations = 1000;
            int iteration = 0;

            while (forwardOpenSet.Count > 0 && backwardOpenSet.Count > 0 && iteration < maxIterations)
            {
                Vector3 currentForward = SelectBestPoint(forwardOpenSet, forwardFScore);
                forwardOpenSet.Remove(currentForward);
                forwardClosedSet.Add(currentForward);

                List<Vector3> forwardNeighbors = FindNearbyPoints(currentForward);
                foreach (var neighbor in forwardNeighbors)
                {
                    if (forwardClosedSet.Contains(neighbor)) continue;

                    float tentativeGScore = forwardGScore[currentForward] + Vector3.Distance(currentForward, neighbor);
                    if (!forwardGScore.ContainsKey(neighbor) || tentativeGScore < forwardGScore[neighbor])
                    {
                        forwardPath[neighbor] = currentForward;
                        forwardGScore[neighbor] = tentativeGScore;
                        forwardFScore[neighbor] = forwardGScore[neighbor] + EstimateDistance(neighbor, closestEnd);

                        if (!forwardOpenSet.Contains(neighbor))
                        {
                            forwardOpenSet.Add(neighbor);
                        }
                    }

                    if (backwardClosedSet.Contains(neighbor))
                    {
                        currentRoute = MergePaths(forwardPath, backwardPath, neighbor, start, end);
                        string successMessage = $"Маршрут построен! Осталось точек: {currentRoute.Count}";
                        isRouteActive = true;
                        lastPlayerPosition = player.Position;
                        Main.TNTPlus.Core.StartCoroutine(FollowRouteCoroutine(player));
                        callback?.Invoke(true, successMessage);
                        yield break;
                    }
                }

                Vector3 currentBackward = SelectBestPoint(backwardOpenSet, backwardFScore);
                backwardOpenSet.Remove(currentBackward);
                backwardClosedSet.Add(currentBackward);

                List<Vector3> backwardNeighbors = FindNearbyPoints(currentBackward);
                foreach (var neighbor in backwardNeighbors)
                {
                    if (backwardClosedSet.Contains(neighbor)) continue;

                    float tentativeGScore = backwardGScore[currentBackward] + Vector3.Distance(currentBackward, neighbor);
                    if (!backwardGScore.ContainsKey(neighbor) || tentativeGScore < backwardGScore[neighbor])
                    {
                        backwardPath[neighbor] = currentBackward;
                        backwardGScore[neighbor] = tentativeGScore;
                        backwardFScore[neighbor] = backwardGScore[neighbor] + EstimateDistance(neighbor, closestStart);

                        if (!backwardOpenSet.Contains(neighbor))
                        {
                            backwardOpenSet.Add(neighbor);
                        }
                    }

                    if (forwardClosedSet.Contains(neighbor))
                    {
                        currentRoute = MergePaths(forwardPath, backwardPath, neighbor, start, end);
                        string successMessage = $"Маршрут построен! Осталось точек: {currentRoute.Count}";
                        isRouteActive = true;
                        lastPlayerPosition = player.Position;
                        Main.TNTPlus.Core.StartCoroutine(FollowRouteCoroutine(player));
                        callback?.Invoke(true, successMessage);
                        yield break;
                    }
                }

                iteration++;
                if (iteration % 100 == 0) yield return null;
            }

            string errorMessage = "Не удалось построить маршрут! Путь слишком длинный или преграждён.";
            callback?.Invoke(false, errorMessage);
        }

        private Vector3 FindNearestRoadPoint(Vector3 target, List<Vector3> points)
        {
            if (points.Count == 0) return target;
            Vector3 nearest = points[0];
            float minDistance = Vector3.Distance(target, nearest);

            foreach (var point in points)
            {
                float distance = Vector3.Distance(target, point);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = point;
                }
            }
            return nearest;
        }

        private float EstimateDistance(Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b);
        }

        private Vector3 SelectBestPoint(List<Vector3> openSet, Dictionary<Vector3, float> fScore)
        {
            Vector3 best = openSet[0];
            float bestScore = fScore.ContainsKey(best) ? fScore[best] : float.MaxValue;

            foreach (var point in openSet)
            {
                float score = fScore.ContainsKey(point) ? fScore[point] : float.MaxValue;
                if (score < bestScore)
                {
                    best = point;
                    bestScore = score;
                }
            }
            return best;
        }

        private List<Vector3> FindNearbyPoints(Vector3 point)
        {
            List<Vector3> neighbors = new List<Vector3>();
            Vector2Int gridPos = new Vector2Int(Mathf.FloorToInt(point.x / maxDistanceToRoad), Mathf.FloorToInt(point.z / maxDistanceToRoad));
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
                            if (distance <= maxDistanceToRoad && distance > minDistanceBetweenPoints && IsPathClear(point, p))
                            {
                                neighbors.Add(p);
                            }
                        }
                    }
                }
            }
            return neighbors;
        }

        private bool IsPathClear(Vector3 from, Vector3 to)
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

        private List<Vector3> MergePaths(Dictionary<Vector3, Vector3> forwardPath, Dictionary<Vector3, Vector3> backwardPath, Vector3 intersection, Vector3 start, Vector3 end)
        {
            List<Vector3> forwardSegment = TracePath(forwardPath, intersection);
            forwardSegment.Insert(0, start);

            List<Vector3> backwardSegment = TracePath(backwardPath, intersection);
            backwardSegment.Reverse();
            backwardSegment.RemoveAt(0);
            backwardSegment.Add(end);

            forwardSegment.AddRange(backwardSegment);
            return forwardSegment;
        }

        private List<Vector3> TracePath(Dictionary<Vector3, Vector3> cameFrom, Vector3 current)
        {
            List<Vector3> path = new List<Vector3> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }

        private void OnLevelLoaded(int level)
        {
            worldSize = Level.size;
            Main.TNTPlus.Core.StartCoroutine(ScanMapCoroutine());
            Rocket.Core.Logging.Logger.Log($"Высота карты: {Level.HEIGHT} | Размер: {Level.size}");
        }

        private void HighlightNextPoints(UnturnedPlayer player)
        {
            if (currentRoute.Count == 0 || !isRouteActive) return;

            for (int i = 0; i < Math.Min(3, currentRoute.Count); i++)
            {
                EffectManager.sendEffect(125, player.CSteamID, currentRoute[i]);
            }
        }

        private IEnumerator FollowRouteCoroutine(UnturnedPlayer player)
        {
            while (currentRoute.Count > 0)
            {
                Vector3 playerPos = player.Position;
                float nearestDist = Vector3.Distance(playerPos, currentRoute[0]);
                int nearestIndex = 0;

                for (int i = 1; i < currentRoute.Count; i++)
                {
                    float dist = Vector3.Distance(playerPos, currentRoute[i]);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearestIndex = i;
                    }
                }

                if (nearestDist < 10f)
                {
                    currentRoute.RemoveRange(0, nearestIndex + 1);
                    MessageManager.Say(player, $"Осталось точек: {currentRoute.Count}", EMessageType.Notification);
                    HighlightNextPoints(player);

                    if (currentRoute.Count == 0)
                    {
                        MessageManager.Say(player, "Вы достигли цели!", EMessageType.Succes);
                        isRouteActive = false;
                    }
                }
                HighlightNextPoints(player);
                yield return new WaitForSeconds(1.5f);
            }
        }

        private IEnumerator ScanMapCoroutine()
        {
            roadPoints.Clear();
            roadGrid.Clear();

            for (float x = -worldSize; x < worldSize; x += scanAccuracy)
            {
                for (float z = -worldSize; z < worldSize; z += scanAccuracy)
                {
                    Vector3 point = DetectRoadPoint(x, z);
                    if (point != Vector3.zero && IsSufficientlySpaced(point, roadPoints))
                    {
                        roadPoints.Add(point);
                        AddPointToGrid(point);
                    }
                }
                yield return null;
            }
            Rocket.Core.Logging.Logger.Log("Кол-во точек на карте: " + roadPoints.Count);
        }

        private void AddPointToGrid(Vector3 point)
        {
            Vector2Int gridPos = new Vector2Int(Mathf.FloorToInt(point.x / maxDistanceToRoad), Mathf.FloorToInt(point.z / maxDistanceToRoad));
            if (!roadGrid.ContainsKey(gridPos))
            {
                roadGrid[gridPos] = new List<Vector3>();
            }    
            roadGrid[gridPos].Add(point);
        }

        private bool IsSufficientlySpaced(Vector3 point, List<Vector3> existingPoints)
        {
            foreach (var existing in existingPoints)
            {
                if (Vector3.Distance(point, existing) < minDistanceBetweenPoints)
                {
                    return false;
                }
            }
            return true;
        }

        private Vector3 DetectRoadPoint(float x, float z)
        {
            if (Physics.Raycast(new Vector3(x, 1024, z), Vector3.down, out var hit, 2048, roadLayerMask))
            {
                if (IsRoadSurface(hit))
                {
                    return new Vector3(x, hit.point.y, z);
                }
            }
            return Vector3.zero;
        }

        private bool IsRoadSurface(RaycastHit hit)
        {
            foreach (var item in Main.TNTPlus.Core.Configuration.Instance.RoadNames)
            {
                if (hit.collider != null && hit.collider.name.StartsWith(item))
                {
                    return true;
                }
            }
            return false;
        }

        private Vector3 GetSurfacePoint(float x, float z)
        {
            if (Physics.Raycast(new Vector3(x, 1024, z), Vector3.down, out var hit, 2048, roadLayerMask))
            {
                return hit.point;
            }
            return Vector3.zero;
        }
    }
}