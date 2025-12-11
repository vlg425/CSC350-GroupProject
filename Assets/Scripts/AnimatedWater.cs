using UnityEngine;

public class AnimatedWater : MonoBehaviour
{
    // Drag your 4 sprite frames here in the Inspector
    public Sprite[] frames;
    
    // Frames per second
    public float frameRate = 10f;

    private SpriteRenderer _renderer;
    private float _timer;
    private int _currentFrame;

    void Start()
    {
        _renderer = GetComponent<Renderer>() as SpriteRenderer;
    }

    void Update()
    {
        if (frames.Length == 0) return;

        _timer += Time.deltaTime;

        // Using a while loop handles cases where a lag spike might skip a frame
        float interval = 1f / frameRate;
        while (_timer >= interval)
        {
            // Subtract interval to keep the "extra" time for the next frame
            // This keeps the animation perfectly smooth
            _timer -= interval;
            
            _currentFrame = (_currentFrame + 1) % frames.Length;
            _renderer.sprite = frames[_currentFrame];
        }
    }
}