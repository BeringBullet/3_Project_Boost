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

    Rigidbody rigidBody;
    AudioSource audioSource;
    enum State { Alive, Dying, Trancending }
    State state = State.Alive;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
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
        Invoke("LoadNextLevel", levelLoadingDelay);
    }
   
    private void StartDeathSequence()
    {
        state = State.Dying;
        PLayOneSound(death);
        deathParticle.Play();
        Invoke("LoadFirstLevel", levelLoadingDelay);
    }

    private void PLayOneSound(AudioClip audioClip)
    {
        audioSource.Stop();
        audioSource.PlayOneShot(audioClip);
    }



    private void LoadNextLevel()
    {
        SceneManager.LoadScene(1);
        state = State.Alive;
    }
    private void LoadFirstLevel()
    {
        SceneManager.LoadScene(0);
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
