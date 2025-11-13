using UnityEngine;

public class Grounder : MonoBehaviour
{
    // Offset vertical respecto al suelo
    [SerializeField] float offsetY = 0f;
    // Altura desde la que disparamos el rayo hacia abajo
    [SerializeField] float raycastHeight = 2f;
    // Distancia máxima del raycast hacia abajo
    [SerializeField] float raycastDistance = 100f;
    // Capa(s) consideradas como suelo (si no se asigna, usará todas las capas)
    [SerializeField] LayerMask groundMask = default;

    // Suavizado opcional del movimiento vertical
    [SerializeField] bool smooth = true;
    [SerializeField] float smoothTime = 0.05f; // tiempo de amortiguación para SmoothDamp
    float _yVel; // velocidad usada por SmoothDamp

    // Si tu suelo es un mesh sin collider, puedes asignar un root del suelo y añadir MeshCollider en runtime
    [Header("Soporte para mallas sin collider")]
    [SerializeField] Transform groundRoot; // raíz que contiene los MeshFilter del suelo
    [SerializeField] bool autoAddMeshColliders = true; // añade MeshCollider a cada MeshFilter hijo si no existe

    // Alternativa sin colliders: raycast manual contra un MeshFilter concreto (coste O(tris))
    [SerializeField] bool useManualMeshRaycast = false;
    [SerializeField] MeshFilter meshForManualRaycast; // usar si no hay colliders y quieres raycast manual

    Vector3 _lastHitPoint;
    bool _hadHit;

    void Awake()
    {
        // Si se pide, añade MeshCollider a las mallas del suelo en runtime
        if (autoAddMeshColliders && groundRoot != null)
        {
            AddMeshCollidersRecursively(groundRoot);
        }
    }

    void LateUpdate()
    {
        ActualizarAlturaSobreSuelo();
    }

    void ActualizarAlturaSobreSuelo()
    {
        var pos = transform.position;

        // Origen del rayo por encima del objeto
        Vector3 rayOrigin = new Vector3(pos.x, pos.y + raycastHeight, pos.z);
        Ray ray = new Ray(rayOrigin, Vector3.down);

        int mask = groundMask.value == 0 ? ~0 : groundMask.value; // si no hay máscara, usar todas las capas

        // 1) Intentar con Physics.Raycast (requiere colliders)
        if (Physics.Raycast(ray, out var hit, raycastDistance, mask, QueryTriggerInteraction.Ignore))
        {
            _hadHit = true;
            _lastHitPoint = hit.point;
            float targetY = hit.point.y + offsetY;
            MoverY(pos, targetY);
            return;
        }

        // 2) Si no hay colliders y se habilitó el modo manual contra un mesh concreto
        if (useManualMeshRaycast && meshForManualRaycast != null && meshForManualRaycast.sharedMesh != null)
        {
            if (RaycastMesh(meshForManualRaycast, ray, out var mHit))
            {
                _hadHit = true;
                _lastHitPoint = mHit.point;
                float targetY = mHit.point.y + offsetY;
                MoverY(pos, targetY);
                return;
            }
        }

        // Si no encontramos suelo, no movemos en Y (opcional: podrías dejar caer el objeto)
        _hadHit = false;
    }

    void MoverY(Vector3 currentPos, float targetY)
    {
        float newY = targetY;
        if (smooth)
        {
            newY = Mathf.SmoothDamp(currentPos.y, targetY, ref _yVel, smoothTime);
        }
        transform.position = new Vector3(currentPos.x, newY, currentPos.z);
    }

    void AddMeshCollidersRecursively(Transform root)
    {
        var filters = root.GetComponentsInChildren<MeshFilter>(includeInactive: true);
        foreach (var f in filters)
        {
            if (f.sharedMesh == null) continue;
            var col = f.GetComponent<MeshCollider>();
            if (col == null)
            {
                col = f.gameObject.AddComponent<MeshCollider>();
            }
            // usar la malla compartida; no convexo para superficies grandes
            col.sharedMesh = f.sharedMesh;
            col.convex = false;
        }
    }

    // Raycast manual contra un MeshFilter usando Möller–Trumbore (sin aceleración espacial)
    struct MeshHit
    {
        public Vector3 point;
        public Vector3 normal;
        public float distance;
    }

    bool RaycastMesh(MeshFilter mf, Ray worldRay, out MeshHit bestHit)
    {
        bestHit = new MeshHit { distance = float.PositiveInfinity };
        var mesh = mf.sharedMesh;
        if (mesh == null) return false;

        // Transformar rayo a espacio local del mesh
        var t = mf.transform;
        Vector3 ro = t.InverseTransformPoint(worldRay.origin);
        Vector3 rd = t.InverseTransformDirection(worldRay.direction);

        var verts = mesh.vertices;
        var tris = mesh.triangles;

        bool hitAny = false;
        const float EPS = 1e-6f;

        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector3 v0 = verts[tris[i]];
            Vector3 v1 = verts[tris[i + 1]];
            Vector3 v2 = verts[tris[i + 2]];

            Vector3 e1 = v1 - v0;
            Vector3 e2 = v2 - v0;
            Vector3 pvec = Vector3.Cross(rd, e2);
            float det = Vector3.Dot(e1, pvec);
            if (det > -EPS && det < EPS) continue; // paralelo
            float invDet = 1f / det;
            Vector3 tvec = ro - v0;
            float u = Vector3.Dot(tvec, pvec) * invDet;
            if (u < 0f || u > 1f) continue;
            Vector3 qvec = Vector3.Cross(tvec, e1);
            float v = Vector3.Dot(rd, qvec) * invDet;
            if (v < 0f || u + v > 1f) continue;
            float tHit = Vector3.Dot(e2, qvec) * invDet;
            if (tHit > EPS)
            {
                // Punto/normal en local
                Vector3 localPoint = ro + rd * tHit;
                Vector3 localNormal = Vector3.Normalize(Vector3.Cross(e1, e2));
                // Convertir a mundo
                Vector3 worldPoint = t.TransformPoint(localPoint);
                Vector3 worldNormal = t.TransformDirection(localNormal).normalized;
                float worldDist = Vector3.Distance(worldRay.origin, worldPoint);

                if (worldDist < bestHit.distance)
                {
                    bestHit = new MeshHit { point = worldPoint, normal = worldNormal, distance = worldDist };
                    hitAny = true;
                }
            }
        }
        return hitAny;
    }

    void OnDrawGizmosSelected()
    {
        // Dibuja el raycast y el punto de impacto
        var pos = transform.position;
        Vector3 rayOrigin = new Vector3(pos.x, pos.y + raycastHeight, pos.z);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * raycastDistance);
        if (_hadHit)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_lastHitPoint + Vector3.up * 0.02f, 0.05f);
        }
    }
}
