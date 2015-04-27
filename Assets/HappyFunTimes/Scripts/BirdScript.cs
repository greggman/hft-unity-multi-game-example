using HappyFunTimes;
using UnityEngine;
using System.Collections;

public class BirdScript : MonoBehaviour {

    public float maxSpeed = 10;
    public Transform groundCheck;
    public LayerMask whatIsGround;
    public float jumpForce = 700f;
    public Transform nameTransform;

    private Color m_color;
    private float m_direction = 0.0f;
    private bool m_jumpPressed = false;      // true if currently held down
    private bool m_jumpJustPressed = false;  // true if pressed just now
    private float m_groundRadius = 0.2f;
    private bool m_grounded = false;
    private bool m_facingRight = true;
    private Animator m_animator;
    private Rigidbody2D m_rigidbody2d;
    private GUIStyle m_guiStyle = new GUIStyle();
    private GUIContent m_guiName = new GUIContent("");
    private Rect m_nameRect = new Rect(0,0,0,0);
    private string m_playerName;

    // Manages the connection between this object and the phone.
    private NetPlayer m_netPlayer;

    // Message when player changes their name.
    private class MessageSetName : MessageCmdData
    {
        public MessageSetName() {  // needed for deserialization
        }
        public MessageSetName(string _name) {
            name = _name;
        }
        public string name = "";
    }

    // Message when player presses or release jump button
    private class MessageJump : MessageCmdData
    {
        public bool jump = false;
    }

    // Message when player pressed left or right
    private class MessageMove : MessageCmdData
    {
        public int dir = 0;  // will be -1, 0, or +1
    }

    // Message when player starts or stops editing their name.
    private class MessageBusy : MessageCmdData
    {
        public bool busy = false;
    }

    // Message to send when sending a player to another game
    private class MessageSwitchGame : MessageCmdData
    {
        public Vector2 pos;
        public Vector2 vel;
        public string name;
        public Color color;
        public float dir;
    }

    // Use this for initialization
    void Start ()
    {
        Init();
        SetColor(new Color(1f, 0.5f, 0.8f, 1f));
    }

    void Init() {
        if (m_animator == null) {
            m_animator = GetComponent<Animator>();
            m_rigidbody2d = GetComponent<Rigidbody2D>();
        }
    }

    // Called when player connects with their phone
    void InitializeNetPlayer(SpawnInfo spawnInfo)
    {
        Init();

        m_netPlayer = spawnInfo.netPlayer;
        m_netPlayer.OnDisconnect += Remove;

        // Setup events for the different messages.
        m_netPlayer.RegisterCmdHandler<MessageSetName>("setName", OnSetName);
        m_netPlayer.RegisterCmdHandler<MessageBusy>("busy", OnBusy);
//        m_netPlayer.RegisterCmdHandler<MessageColor>(OnColor);
        m_netPlayer.RegisterCmdHandler<MessageMove>("move", OnMove);
        m_netPlayer.RegisterCmdHandler<MessageJump>("jump", OnJump);

        // We always get a `data` so check if it has one key from our expected message
        if (spawnInfo.data != null && spawnInfo.data.ContainsKey("dir")) {
            // This player was transferred from another game.

            // Turn the data back into our structure
            DeJson.Deserializer deserializer = new DeJson.Deserializer();
            MessageSwitchGame data = deserializer.Deserialize<MessageSwitchGame>(spawnInfo.data);

            // Choose a starting position based on the old position
            float x = (data.pos.x < LevelSettings.settings.leftEdgeOfLevel.position.x)
                ? LevelSettings.settings.rightEdgeOfLevel.position.x - 1 : LevelSettings.settings.leftEdgeOfLevel.position.x + 1;
            transform.localPosition = new Vector3(x, data.pos.y, 0f);

            // Set the initial velocity
            m_rigidbody2d.velocity = data.vel;

            // And the direction
            m_direction = data.dir;

            SetName(data.name);
            SetColor(data.color);
        } else {
            // This player just joined.
            MoveToRandomSpawnPoint();
            SetName(spawnInfo.name);
        }
    }

    void Update()
    {
        // If we're on the ground AND we pressed jump (or space)
        if (m_grounded && (m_jumpJustPressed || Input.GetKeyDown("space")))
        {
            m_grounded = false;
            m_animator.SetBool("Ground", m_grounded);
            m_rigidbody2d.AddForce(new Vector2(0, jumpForce));
        }
        m_jumpJustPressed = false;
    }

    void MoveToRandomSpawnPoint()
    {
        // Pick a random spawn point
        int ndx = Random.Range(0, LevelSettings.settings.spawnPoints.Length - 1);
        transform.localPosition = LevelSettings.settings.spawnPoints[ndx].localPosition;
    }

    void SetName(string name)
    {
        m_playerName = name;
        gameObject.name = "Player-" + m_playerName;
        m_guiName = new GUIContent(m_playerName);
        Vector2 size = m_guiStyle.CalcSize(m_guiName);
        m_nameRect.width  = size.x + 12;
        m_nameRect.height = size.y + 5;
    }

    void SetColor(Color color) {
        m_color = color;
        Color[] pix = new Color[1];
        pix[0] = color;
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixels(pix);
        tex.Apply();
        m_guiStyle.normal.background = tex;
    }

    void Remove(object sender, System.EventArgs e)
    {
        Destroy(gameObject);
    }

    // Update is called once per frame
    void FixedUpdate () {
        // Check if the center under us is touching the ground and
        // pass that info to the Animator
        m_grounded = Physics2D.OverlapCircle(groundCheck.position, m_groundRadius, whatIsGround);
        m_animator.SetBool("Ground", m_grounded);

        // Pass our vertical speed to the animator
        m_animator.SetFloat("vSpeed", m_rigidbody2d.velocity.y);

        // Get left/right input (get both phone and local input)
        float move = m_direction + Input.GetAxis("Horizontal");

        // Pass that to the animator
        m_animator.SetFloat("Speed", Mathf.Abs(move));

        // and move us
        m_rigidbody2d.velocity = new Vector2(move * maxSpeed, m_rigidbody2d.velocity.y);
        if (move > 0 && !m_facingRight) {
            Flip();
        } else if (move < 0 && m_facingRight) {
            Flip();
        }

        if (transform.position.x < LevelSettings.settings.leftEdgeOfLevel.position.x) {
            SwitchGame(-1);
        } else if (transform.position.x > LevelSettings.settings.rightEdgeOfLevel.position.x) {
            SwitchGame(1);
        }
        if (transform.position.y < LevelSettings.settings.bottomOfLevel.position.y) {
            MoveToRandomSpawnPoint();
        }
    }

    void Flip()
    {
        m_facingRight = !m_facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    void SwitchGame(int dir)
    {
        MessageSwitchGame data = new MessageSwitchGame();
        data.pos = transform.position;
        data.vel = m_rigidbody2d.velocity;
        data.name = m_playerName;
        data.color = m_color;
        data.dir = m_direction;

        // Look up the id of the game where in. Something like "level3"
        string gameId = LevelSettings.settings.playerSpawner.server.Id;
        // Extract the number part
        int numNdx = gameId.IndexOfAny("0123456789".ToCharArray());
        int gameNum = System.Convert.ToInt32(gameId.Substring(numNdx));
        // Increment or Decrement the number wraping at the ends
        gameNum = (gameNum + LevelSettings.settings.numGames + dir) % LevelSettings.settings.numGames;

        // Build the ID of the game we want to switch to
        gameId = gameId.Substring(0, numNdx) + gameNum;

        // Send this player to that game.
        // Note: No more messages will be received or sent to this player.
        m_netPlayer.SwitchGame(gameId, data);
    }

    void OnGUI()
    {
        Vector2 size = m_guiStyle.CalcSize(m_guiName);
        Vector3 coords = Camera.main.WorldToScreenPoint(nameTransform.position);
        m_nameRect.x = coords.x - size.x * 0.5f - 5f;
        m_nameRect.y = Screen.height - coords.y;
        m_guiStyle.normal.textColor = Color.black;
        m_guiStyle.contentOffset = new Vector2(4, 2);
        GUI.Box(m_nameRect, m_playerName, m_guiStyle);
    }

    void OnSetName(MessageSetName data)
    {
        if (data.name.Length == 0)
        {
            m_netPlayer.SendCmd(new MessageSetName(m_playerName));
        }
        else
        {
            SetName(data.name);
        }
    }

    void OnBusy(MessageBusy data)
    {
        // We ignore this message
    }

    void OnMove(MessageMove data)
    {
        m_direction = data.dir;
    }

    void OnJump(MessageJump data)
    {
        m_jumpJustPressed = data.jump && !m_jumpPressed;
        m_jumpPressed = data.jump;
    }
}
