using UnityEngine;
using UnityEngine.UI;

public class Oxygen : MonoBehaviour
{
    public static bool NoSprint;

    private float _oxygenMeter = 100.0f;
    [SerializeField] public float _depletionSpeed = 1.0f;

    [SerializeField] private Color _goodState;
    [SerializeField] private Color _warning;
    [SerializeField] private Color _low;

    private RectTransform _rect;
    private Image _image;

    // thresholds for color transitions
    [SerializeField] private float warningThreshold = 50.0f;
    [SerializeField] private float lowThreshold = 20.0f;

    void Start()
    {
        _rect = gameObject.GetComponent<RectTransform>();
        _image = GetComponent<Image>();
    }

    void Update()
    {
        if(!NoSprint)
        {
            _oxygenMeter -= _depletionSpeed * Time.deltaTime;

            _oxygenMeter = Mathf.Clamp(_oxygenMeter, 0f, 100f);

            _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, _oxygenMeter);
            _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x, -534.0f + ((_oxygenMeter * 2.221f))); // Set your desired position

            NoSprint = _oxygenMeter <= 0;

            UpdateColor();
        }
    }

    void UpdateColor()
    {
        if(_oxygenMeter > warningThreshold)
        {
            _image.color = _goodState;
        }
        else if(_oxygenMeter > lowThreshold)
        {
            _image.color = _warning;
        }
        else
        {
            _image.color = _low;
        }
    }
}
