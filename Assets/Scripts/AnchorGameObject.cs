using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AnchorGameObject : MonoBehaviour
{
    public enum AnchorType
    {
        BottomLeft,
        BottomCenter,
        BottomRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        TopLeft,
        TopCenter,
        TopRight,
    };

    public bool executeInUpdate;

    public AnchorType anchorType;
    public Vector3 anchorOffset;

    IEnumerator updateAnchorRoutine; //Coroutine handle so we don't start it if it's already running

    // Use this for initialization
    void Start()
    {
        updateAnchorRoutine = UpdateAnchorAsync();
        StartCoroutine(updateAnchorRoutine);
    }

    /// <summary>
    /// Coroutine to update the anchor only once CameraFit.Instance is not null.
    /// </summary>
    IEnumerator UpdateAnchorAsync()
    {

        uint cameraWaitCycles = 0;

        while (CameraviewportHandler.Instance == null)
        {
            ++cameraWaitCycles;
            yield return new WaitForEndOfFrame();
        }

        if (cameraWaitCycles > 0)
        {
            print(string.Format("CameraAnchor found CameraFit instance after waiting {0} frame(s). " +
                "You might want to check that CameraFit has an earlie execution order.", cameraWaitCycles));
        }

        UpdateAnchor();
        updateAnchorRoutine = null;

    }

    void UpdateAnchor()
    {
        switch (anchorType)
        {
            case AnchorType.BottomLeft:
                SetAnchor(CameraviewportHandler.Instance.BottomLeft);
                break;
            case AnchorType.BottomCenter:
                SetAnchor(CameraviewportHandler.Instance.BottomCenter);
                break;
            case AnchorType.BottomRight:
                SetAnchor(CameraviewportHandler.Instance.BottomRight);
                break;
            case AnchorType.MiddleLeft:
                SetAnchor(CameraviewportHandler.Instance.MiddleLeft);
                break;
            case AnchorType.MiddleCenter:
                SetAnchor(CameraviewportHandler.Instance.MiddleCenter);
                break;
            case AnchorType.MiddleRight:
                SetAnchor(CameraviewportHandler.Instance.MiddleRight);
                break;
            case AnchorType.TopLeft:
                SetAnchor(CameraviewportHandler.Instance.TopLeft);
                break;
            case AnchorType.TopCenter:
                SetAnchor(CameraviewportHandler.Instance.TopCenter);
                break;
            case AnchorType.TopRight:
                SetAnchor(CameraviewportHandler.Instance.TopRight);
                break;
        }
    }

    void SetAnchor(Vector3 anchor)
    {
        Vector3 newPos = anchor + anchorOffset;
        if (!transform.position.Equals(newPos))
        {
            transform.position = newPos;
        }
    }

#if UNITY_EDITOR
    // Update is called once per frame
    void Update()
    {
        if (updateAnchorRoutine == null && executeInUpdate)
        {
            updateAnchorRoutine = UpdateAnchorAsync();
            StartCoroutine(updateAnchorRoutine);
        }
    }
#endif
}