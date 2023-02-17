using System.Collections;
using UnityEngine;

public class DayAndNightCycler : MonoBehaviour
{
    public GameGlobalParameters gameParameters;
    public Transform starsTransform;

    private float _starsRefreshRate;
    private float _rotationAngleStep;
    private Vector3 _rotationAxis;

    private void Start()
    {        
        starsTransform.rotation = Quaternion.Euler(
            gameParameters.dayInitialRatio * 360f,
            -30f,
            0f
        );
        _starsRefreshRate = 0.1f;
        _rotationAxis = starsTransform.right;
        _rotationAngleStep = 360f * _starsRefreshRate / gameParameters.dayLengthInSeconds;
        
        StartCoroutine("_UpdateStars");
    }

    private IEnumerator _UpdateStars()
    {
        while (true)
        {
            starsTransform.Rotate(_rotationAxis, _rotationAngleStep, Space.World);
            yield return new WaitForSeconds(_starsRefreshRate);
        }
    }
}
