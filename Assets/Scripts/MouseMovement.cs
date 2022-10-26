using System.Threading;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;    // console testing purposes
using Color = UnityEngine.Color;

public class MouseMovement : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color originalColor;
    private Vector3 mousePosition;
    private Rigidbody2D rb;
    private List<Color> colorStorage;           // storing color values of cubes in collision with cursor
    private List<GameObject> collisionObjStorage;   //storing gameObjects in collision with cursor
    public List<GameObject> cubes;              // reference to all cubes in scene
    //timer code
    [SerializeField] public float timer = 3f;   // public timer for adjusting the color change delay
    private bool collisionTrigger = false;      // flag to check for collisions to start countdown
    private float tempTimer;                    // actual timer var to use in code

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        collisionObjStorage = new List<GameObject>();
        colorStorage = new List<Color>();

        tempTimer = timer;
        originalColor = sr.color;
    }

    // Update is called once per frame
    void Update()
    {
        rb.WakeUp();    // constantly keep cursor awake for timer countdown purposes

        //setting up cursor object to follow mouse
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z += Camera.main.nearClipPlane;
        transform.position = mousePosition;
    }

    void OnTriggerEnter2D(Collider2D obj)
    {
        // since I'm assured I'll only be dealing with BoxCollider obj, no need to check tags
        // will assume this for all Trigger functions
        Debug.Log("Collision encountered.");

        collisionTrigger = true;
        collisionObjStorage.Add(obj.gameObject);

        colorStorage.Add(obj.gameObject.GetComponent<SpriteRenderer>().color);
        sr.color = (colorStorage.Count == 1) ? colorStorage[0] : colorBlender(colorStorage);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (collisionTrigger)
        {
            //starting countdown
            tempTimer -= Time.deltaTime;
            if (tempTimer < 0)
            {
                tempTimer = 0f; // resetting timer

                // In retrospect, should've used a manager following the Singleton programming pattern to keep check of...
                // color changes in the cursor, however I've decided to carry on with utilizing the lists I produced earlier
                // SOLUTION 1: ONE UNIQUE NEW COLOR FOR THE PREVIOUS OLD COLOR
                HashSet<Color> unique_colors = new HashSet<Color>();
                foreach (Color c in colorStorage)
                {
                    unique_colors.Add(c);
                }

                // mapping old colors to a newly produced color using a Dictionary
                Dictionary<Color, Color> colorMatcher = new Dictionary<Color, Color>();
                foreach (Color c in unique_colors)
                {
                    Color randMatch = randColorGenerator();
                    colorMatcher.Add(c, randMatch);
                }

                // replace color of the colliding GameObjects, resetting colorStorage list to keep Cursor color consistent
                colorStorage.Clear();
                foreach (GameObject i in collisionObjStorage)
                {
                    Color cubeColor = i.GetComponent<SpriteRenderer>().color;
                    if (colorMatcher.ContainsKey(cubeColor))
                    {
                        Color temp = new Color();
                        colorMatcher.TryGetValue(cubeColor, out temp);
                        i.GetComponent<SpriteRenderer>().color = temp;
                        colorStorage.Add(temp);
                    }
                }

                // replace color of all other referred GameObjects in the scene
                foreach (GameObject i in cubes)
                {
                    Color cubeColor = i.GetComponent<SpriteRenderer>().color;
                    if (colorMatcher.ContainsKey(cubeColor))
                    {
                        Color temp = new Color();
                        colorMatcher.TryGetValue(cubeColor, out temp);
                        i.GetComponent<SpriteRenderer>().color = temp;
                    }
                }

                // // SOLUTION 2: RANDOM NEW COLORS FOR ONE PARTICULAR COLOR
                // HashSet<Color> unique_colors = new HashSet<Color>();
                // foreach (Color c in colorStorage)
                // {
                //     unique_colors.Add(c);
                // }

                // colorStorage.Clear();
                // foreach (GameObject i in collisionObjStorage)
                // {
                //     if (unique_colors.Contains(i.GetComponent<SpriteRenderer>().color))
                //     {
                //         i.GetComponent<SpriteRenderer>().color = randColorGenerator();
                //         colorStorage.Add(i.GetComponent<SpriteRenderer>().color);
                //     }
                // }

                // foreach (GameObject i in cubes)
                // {
                //     if (unique_colors.Contains(i.GetComponent<SpriteRenderer>().color))
                //     {
                //         i.GetComponent<SpriteRenderer>().color = randColorGenerator();
                //     }
                // }
                collisionTrigger = false;   // to terminate the color production process
            }
        }
    }

    void OnTriggerExit2D(Collider2D obj)
    {
        // removing all references of GameObject collision with cursor
        collisionObjStorage.Remove(obj.gameObject);
        colorStorage.Remove(obj.gameObject.GetComponent<SpriteRenderer>().color);

        // resetting variables
        sr.color = (colorStorage.Count > 0) ? colorBlender(colorStorage) : originalColor;
        tempTimer = timer;
        collisionTrigger = false;
        Debug.Log("Collision annihilated.");
    }

    // Helper functions
    private Color colorBlender(List<Color> arr)
    {
        Color res = new Color(0, 0, 0, 0);

        foreach (Color c in arr) { res += c; }
        res /= arr.Count;
        res.a = 1f; // setting alpha to full opacity

        return res;
    }

    private Color randColorGenerator()
    {
        Color res = new Color(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            1f
        );
        return res;
    }
}