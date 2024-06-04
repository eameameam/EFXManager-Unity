using UnityEngine;

public class TrailMover
{
    private GameObject _previewObject;
    private float _speed;
    private float _radius;
    private float _angle;

    public TrailMover(GameObject previewObject, float speed, float radius)
    {
        _previewObject = previewObject;
        _speed = speed;
        _radius = radius;
        _angle = 0f;
    }

    public void Update()
    {
        if (_previewObject == null) return;

        _angle += _speed * Time.deltaTime;
        if (_angle > 360f) _angle -= 360f;

        float x = Mathf.Cos(_angle) * _radius;
        float z = Mathf.Sin(_angle) * _radius;

        _previewObject.transform.position = new Vector3(x, _previewObject.transform.position.y, z);
    }

    public void ResetPosition()
    {
        if (_previewObject != null)
        {
            _previewObject.transform.position = Vector3.zero;
        }
        _angle = 0f;
    }

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }

    public void SetRadius(float radius)
    {
        _radius = radius;
    }
}
