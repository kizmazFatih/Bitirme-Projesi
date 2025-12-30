using UnityEngine;

public class CarWaypointFollower : MonoBehaviour
{
    [Header("Path")]
    public Transform[] waypoints;
    public bool loop = true;

    [Header("Movement")]
    public float speed = 8f;
    public float turnSpeed = 6f;
    public float arriveDistance = 1.5f;

    [Header("Optional")]
    public bool startOnPlay = true;
    public float startDelay = 0f;

	[Header("Model Forward Fix")]
	public float yawOffset = -90f; // 90 veya -90 deneyeceğiz


    int _index = 0;
    bool _running = false;
    float _timer = 0f;

    void Start()
    {
        _running = startOnPlay;
    }

    void Update()
    {
        if (!_running)
        {
            if (startOnPlay)
            {
                _timer += Time.deltaTime;
                if (_timer >= startDelay) _running = true;
            }
            else return;
        }

        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[_index];
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        // move
        Vector3 dir = toTarget.normalized;
        transform.position += dir * speed * Time.deltaTime;

        // rotate smoothly
        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
			lookRot *= Quaternion.Euler(0f, yawOffset, 0f);
			transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, turnSpeed * Time.deltaTime);
 }

        // next waypoint
        if (toTarget.magnitude <= arriveDistance)
        {
            _index++;
            if (_index >= waypoints.Length)
            {
                if (loop) _index = 0;
                else _running = false;
            }
        }
    }

    // Timeline veya kodla kontrol için:
    public void SetRunning(bool value) => _running = value;
    public void SetSpeed(float value) => speed = value;
}