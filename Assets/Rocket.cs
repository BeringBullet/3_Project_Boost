using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float mainThrust = 100f;
    [SerializeField] float levelLoadingDelay = 1f;

    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip success;
    [SerializeField] AudioClip death;

    [SerializeField] ParticleSystem mainEngineParticle;
    [SerializeField] ParticleSystem successParticle;
    [SerializeField] ParticleSystem deathParticle;

    MyLog myLog;
    Rigidbody rigidBody;
    AudioSource audioSource;
    enum State { Alive, Dying, Trancending }
    State state = State.Alive;

    Vector3 startingLocation;
    Quaternion startingRotation;
    // Start is called before the first frame update
    void Start()
    {
        startingLocation = transform.position;
        startingRotation = transform.rotation;
        myLog = GetComponent<MyLog>();
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        PlayerStats.livesLeft = PlayerStats.MaxLives;
    }

    // Update is called once per frame
    void Update()
    {
        myLog.Clear();
        Debug.Log($"current Level: {PlayerStats.currentLevel}");
        Debug.Log($"Lives Left: {PlayerStats.livesLeft}");
        if (state == State.Alive)
        {
            RespondToThrustInput();
            Rotate();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive) return;

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                // do nothing
                break;
            case "Finish":
                StartSeccessSequence();
                break;
            default:
                StartDeathSequence();
                break;
        }
    }

    private void StartSeccessSequence()
    {
        state = State.Trancending;
        PLayOneSound(success);
        successParticle.Play();
        PlayerStats.currentLevel += 1;
        StartCoroutine(LoadNextLevel((int)PlayerStats.currentLevel, levelLoadingDelay));
    }

    private void StartDeathSequence()
    {
        state = State.Dying;
        PLayOneSound(death);
        deathParticle.Play();
        if (PlayerStats.livesLeft == 0 || PlayerStats.currentLevel == PlayerStats.Levels.one)
        {
            PlayerStats.livesLeft = PlayerStats.MaxLives;
            PlayerStats.currentLevel = PlayerStats.Levels.one;
            StartCoroutine(LoadNextLevel((int)PlayerStats.Levels.one, levelLoadingDelay));
        }
        else
        {
            state = State.Trancending;
            StartCoroutine(Reset(levelLoadingDelay));
        }
    }
    private IEnumerator Reset(float delayTime = 1f)
    {
        yield return new WaitForSeconds(delayTime);
        PlayerStats.livesLeft--;
        audioSource.Stop();
        transform.position = startingLocation;
        transform.rotation = startingRotation;
        state = State.Alive;
    }

    private void PLayOneSound(AudioClip audioClip)
    {
        audioSource.Stop();
        audioSource.PlayOneShot(audioClip);
    }

    private IEnumerator LoadNextLevel(int index = 0, float delayTime = 1f)
    {
        yield return new WaitForSeconds(delayTime);
        SceneManager.LoadScene(index);
        state = State.Alive;
    }

    private void Rotate()
    {
        float rotaionThisFrame = rcsThrust * Time.deltaTime;
        rigidBody.freezeRotation = true;
        Vector3 rotateValue = Vector3.forward * rotaionThisFrame;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(rotateValue);
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(-rotateValue);
        }
        rigidBody.freezeRotation = false;
    }

    private void RespondToThrustInput()
    {
        if (Input.GetKey(KeyCode.Space)) //Can thrust whire rotating
        {
            ApplyThrust();
        }
        else
        {
            audioSource.Stop();
            mainEngineParticle.Stop();
        }
    }

    private void ApplyThrust()
    {
        rigidBody.AddRelativeForce(Vector3.up * mainThrust);
        if (!audioSource.isPlaying)
            audioSource.PlayOneShot(mainEngine);

        mainEngineParticle.Play();
    }
}
