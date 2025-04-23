using UnityEngine;
using UnityEngine.Tilemaps;

public static class HoleUtils
{
    public static void TryFallInHole(GameObject unit, Vector3Int tilePosition, Tilemap tilemap)
    {
        Vector3 center = tilemap.GetCellCenterWorld(tilePosition);
        Debug.Log($"[HoleUtils] Checking for hole at {tilePosition} (world pos: {center}) for {unit.name}");

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, new Vector2(0.9f, 0.9f), 0f);

        if (hits.Length == 0)
        {
            Debug.LogWarning("[HoleUtils] No colliders found at tile. Hole not detected.");
            return;
        }

        foreach (var hit in hits)
        {
            Debug.Log($"[HoleUtils] Hit: {hit.name}, Tag: {hit.tag}, Z: {hit.transform.position.z}, IsTrigger: {hit.isTrigger}");

            if (hit.CompareTag("Hole"))
            {
                Debug.Log($"[HoleUtils] {unit.name} landed in a HOLE!");

                unit.transform.position = center;

                GoonFatigue goonFatigue = unit.GetComponent<GoonFatigue>();
                if (goonFatigue != null)
                {
                    goonFatigue.prone = true;

                    GoonMove goonMove = unit.GetComponent<GoonMove>();
                    if (goonMove != null && goonMove.InPuddle)
                    {
                        goonFatigue.UseGetUpFatigue();
                        goonMove.InPuddle = false;
                        Debug.Log("[HoleUtils] Goon used fatigue to get up immediately (swing behavior).");
                    }
                    else
                    {
                        Debug.Log("[HoleUtils] Goon is prone. Will use fatigue next turn (push behavior).");
                    }
                }

                GoonMove punchCheck = unit.GetComponent<GoonMove>();
                if (punchCheck != null && punchCheck.IsPunchCharging)
                {
                    punchCheck.CancelPunch();
                }

                return;
            }
        }

        Debug.LogWarning("[HoleUtils] No object with tag 'Hole' found in hits.");
    }
}
