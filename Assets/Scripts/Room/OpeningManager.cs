using System.Collections.Generic;
using UnityEngine;

public class OpeningManager : MonoBehaviour
{
    public static OpeningManager Instance;

    private List<Opening> _allOpenings = new List<Opening>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<Opening> GetAllOpenings()
    {
        return _allOpenings;
    }

    /// <summary>
    /// Try to reattach all stranded openings to the nearest wall.
    /// Should be called after walls are rebuilt.
    /// </summary>
    public void TryReattachAll()
    {
        foreach (Opening opening in _allOpenings)
        {
            if (opening == null) continue;

            // Only stranded ones (no parent wall)
            if (opening.ParentWall == null)
            {
                Wall nearestWall = WallManager.Instance.FindNearestWall(opening.transform.position, out Vector3 proj);

                if (nearestWall != null)
                {
                    // Reinitialize on the new wall
                    opening.Initialize(nearestWall, proj);
                }
                else
                {
                    Debug.LogWarning($"Opening {opening.name} could not find a nearby wall to attach to.");
                }
            }
        }
    }

    public void AddOpening(Opening opening)
    {
        if (!_allOpenings.Contains(opening))
            _allOpenings.Add(opening);
    }

    /// <summary>
    /// Remove destroyed/null openings from the list.
    /// </summary>
    public void Cleanup()
    {
        _allOpenings.RemoveAll(o => o == null);
    }
}
