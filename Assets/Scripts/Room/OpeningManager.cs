using System.Collections.Generic;
using UnityEngine;

public class OpeningManager : MonoBehaviour
{
    public static OpeningManager Instance;

    [SerializeField] private List<Opening> _allOpenings = new List<Opening>();

    public Opening _currentSelectedOpening;

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
        const float maxReattachDistance = 1.0f;

        for (int i = _allOpenings.Count - 1; i >= 0; i--)
        {
            Opening opening = _allOpenings[i];
            if (opening == null) continue;

            if (opening.ParentWall == null)
            {
                Wall nearestWall = WallManager.Instance.FindNearestWall(opening.transform.position, out Vector3 proj);

                if (nearestWall != null)
                {
                    proj.y = 3f;
                    if (Vector3.Distance(opening.transform.position, proj) < maxReattachDistance)
                    {
                        opening.Initialize(nearestWall, proj);
                    }
                }
                else
                {
                    _allOpenings.RemoveAt(i);
                    Destroy(opening.gameObject);
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

    public void DeleteOpening(Opening opening)
    {
        opening.ParentWall._allOpenings.Remove(opening);
        _allOpenings.Remove(opening);
        Destroy(opening.gameObject);
    }

}
