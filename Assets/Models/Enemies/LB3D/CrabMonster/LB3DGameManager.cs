using UnityEngine;
using UnityEngine.UI;

public class LB3DGameManager : MonoBehaviour
{
    public Animator animator;

    public GameObject cam;
    public GameObject product;
    public Slider rotate;
    public float rotationSpeed = 1;
    public Slider zoom;
    public Vector3 originalCamPosition;

    public Dropdown loops;
    public Dropdown actions;
    public Button play;

    // Use this for initialization
    private void Start()
    {
        originalCamPosition = cam.transform.position;
    }

    // Update is called once per frame
    private void Update()
    {
        var startRotation = product.transform.rotation;
        var toRotation = Quaternion.Euler(new Vector3(product.transform.rotation.eulerAngles.x, 360.0f - rotate.value,
            product.transform.rotation.eulerAngles.z));
        product.transform.rotation = Quaternion.Lerp(startRotation, toRotation, Time.deltaTime * rotationSpeed);

        cam.transform.position = originalCamPosition + new Vector3(0, 0, zoom.value) * 5f;

        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }

    public void PlayAction()
    {
        var actionIndex = actions.value;
        var actionString = actions.options[actionIndex].text;
        animator.SetTrigger(actionString);
    }

    public void PlayLoop()
    {
        var loopIndex = loops.value;
        var loopString = loops.options[loopIndex].text;
        animator.SetTrigger(loopString);
    }
}