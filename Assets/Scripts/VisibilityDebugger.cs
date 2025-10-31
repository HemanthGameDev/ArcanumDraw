using UnityEngine;

public class VisibilityDebugger : MonoBehaviour
{
    private void OnBecameVisible()
    {
        Debug.Log($"<color=lime>✓ {gameObject.name} BECAME VISIBLE to camera!</color>");
    }

    private void OnBecameInvisible()
    {
        Debug.Log($"<color=red>✗ {gameObject.name} BECAME INVISIBLE to camera!</color>");
    }

    private void Update()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null && Time.frameCount % 60 == 0)
        {
            Debug.Log($"<color=yellow>👁 {gameObject.name} Visibility Check:</color>");
            Debug.Log($"   Position: {transform.position}");
            Debug.Log($"   Scale: {transform.localScale}");
            Debug.Log($"   Renderer.enabled: {mr.enabled}");
            Debug.Log($"   Renderer.isVisible: {mr.isVisible}");
            Debug.Log($"   GameObject.activeSelf: {gameObject.activeSelf}");
            Debug.Log($"   GameObject.layer: {LayerMask.LayerToName(gameObject.layer)}");
            
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);
                Debug.Log($"   Viewport Position: {viewportPos} (visible if x,y in 0-1 and z > 0)");
                Debug.Log($"   Distance to Camera: {Vector3.Distance(transform.position, cam.transform.position):F2}");
            }
        }
    }
}
