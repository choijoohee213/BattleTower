using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private float cameraSpeed;
    private float xMax, yMin;

    private void Awake() {
        LevelManager.Instance.CreateLevel();
    }

    void CameraSetUp() {
        Camera camera = GetComponent<Camera>();
        Rect rect = camera.rect;
        float scaleHeight = ((float)Screen.width / Screen.height) / ((float)16 / 9);
        float scaleWidth = 1f / scaleHeight;
        if(scaleHeight < 1) {
            rect.height = scaleHeight;
            rect.y = (1f - scaleHeight) / 2f;
        }
        else {
            rect.width = scaleWidth;
            rect.x = (1f - scaleWidth) / 2f;
        }
        camera.rect = rect;
    }

    void Update()
    {
        GetInput();
    }

    void GetInput() {
        if (Input.GetKey(KeyCode.W))
            transform.Translate(Vector3.up * cameraSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.A))
            transform.Translate(Vector3.left * cameraSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.S))
            transform.Translate(Vector3.down * cameraSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.D))
            transform.Translate(Vector3.right * cameraSpeed * Time.deltaTime);

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, 0, xMax),
            Mathf.Clamp(transform.position.y,yMin,0),-10);
    }

    public void SetLimits(Vector3 maxTile) {
        Vector3 wp = Camera.main.ViewportToWorldPoint(new Vector3(1,0));
        xMax = maxTile.x - wp.x;
        yMin = maxTile.y - wp.y;
    }
}
