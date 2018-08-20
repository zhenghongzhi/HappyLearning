using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LeanCloud;
using UniRx;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour {
    // 小人跳跃时，决定远近的一个参数
    public float Factor;
    private Rigidbody _rigidbody;
    private float _startTime;

    // 盒子随机最远的距离
    public float MaxDistance=2.5f;

    // 第一个盒子物体
    public GameObject Stage;
    // 盒子仓库，可以放上各种盒子的prefab，用于动态生成。
    public GameObject[] BoxTemplates;
    private GameObject _currentStage;
    private Collider _lastCollionCollider;
    public Transform Camera;
    private Vector3 _cameraRelativePosition;
    public Text ScoreText;
    private int _score;
    // 粒子效果
    public GameObject Particle;
    public Transform Head;
    public Transform Body;
    // 飘分的UI组件
    public Text SingleScoreText;
    private bool _isUpdateScoreAnimation;
    private float _scoreAnimationStartTime;
    // 保存分数面板
    public GameObject SaveScorePanel;
    // 名字输入框
    public InputField NameField;
    // 保存按钮
    public Button SaveButton;
    // 排行榜面板
    public GameObject RankPanel;
    public GameObject RankItem;
    // 重新开始按钮
    public Button RestartButton;
    Vector3 _direction = new Vector3(1, 0, 0);
    // 左上角总分的UI组件
    public Text TotalScoreText;
    // 排行数据的姓名
    //public GameObject RankName;
    // 排行数据的分数
   // public GameObject RankScore;
    private int _lastReward = 1;
    private bool _enableInput = true;

    // Use this for initialization
    void Start () {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.centerOfMass = Vector3.zero;

        _currentStage = Stage;
        SpawnStage();
        //_lastCollionCollider = _currentStage.GetComponent<Collider>();
        _cameraRelativePosition = Camera.position - transform.position;

        SaveButton.onClick.AddListener(OnClickSaveButton);
        MainThreadDispatcher.Initialize();
        RestartButton.onClick.AddListener(()=>
        {
            SceneManager.LoadScene(0);
        });
    
    }

    // Update is called once per frame
    void Update() {
        if (_enableInput)
        {
            if (Input.GetMouseButtonDown(1))
            {
                _startTime = Time.time;
                Particle.SetActive(true);
            }

            if (Input.GetMouseButtonUp(1))
            {
                // 计算总共按下鼠标的时长
                var elapse = Time.time - _startTime;
                OnJump(elapse);
                Particle.SetActive(false);

                //还原小人的形状
                Body.transform.DOScale(0.1f, 0.2f);
                Head.transform.DOLocalMoveY(0.29f, 0.2f);

                //还原盒子的形状
                _currentStage.transform.DOLocalMoveY(0.25f, 0.2f);
                _currentStage.transform.DOScaleY(0.5f, 0.2f);

                _enableInput = false;
            }

            // 处理按下空格时小人和盒子的动画
            if (Input.GetMouseButton(1))
            {
                //添加限定，盒子最多缩放一半
                if (_currentStage.transform.localScale.y > 0.3)
                {
                    Body.transform.localScale += new Vector3(1, -1, 1) * 0.05f * Time.deltaTime;
                    Head.transform.localPosition += new Vector3(0, -1, 0) * 0.1f * Time.deltaTime;

                    _currentStage.transform.localScale += new Vector3(0, -1, 0) * 0.15f * Time.deltaTime;
                    _currentStage.transform.localPosition += new Vector3(0, -1, 0) * 0.15f * Time.deltaTime;
                }
            }
        }

        // 是否显示飘分效果
        if (_isUpdateScoreAnimation)
            UpdateScoreAnimation();

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Home))
        {
            Application.Quit();
        }
    }
    void OnJump(float elapse)
    {
       
        _rigidbody.AddForce(new Vector3(0, 5f, 0) + (_direction) * elapse * Factor, ForceMode.Impulse);
        transform.DOLocalRotate(new Vector3(0, 0, -360), 0.6f, RotateMode.LocalAxisAdd);
    }
    void SpawnStage()
    {
        GameObject prefab;
        if (BoxTemplates.Length > 0)
        {
            // 从盒子库中随机取盒子进行动态生成
            prefab = BoxTemplates[Random.Range(0, BoxTemplates.Length)];
        }
        else
        {
            prefab = Stage;
        }

        var stage = Instantiate(prefab);
        stage.transform.position = _currentStage.transform.position + _direction * Random.Range(1.1f, MaxDistance);

        var randomScale = Random.Range(0.5f, 1);
        stage.transform.localScale = new Vector3(randomScale, 0.5f, randomScale);

        // 重载函数 或 重载方法
        stage.GetComponent<Renderer>().material.color =
            new Color(Random.Range(0f, 1), Random.Range(0f, 1), Random.Range(0f, 1));

    }
    void OnCollisionExit(Collision collision)
    {
        //_enableInput = false;
    } 

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Ground")
        {
            OnGameOver();
        }
        else
        {
            if (_currentStage != collision.gameObject)//条件 当前的小人所在的盒子不是碰撞后的游戏对象
            {
                var contacts = collision.contacts;//var定义数组变量contact 将被碰撞后的物体的接触点集（集合）赋值给这个数组变量

                //check if player's feet on the stage
                if (contacts.Length == 4 && contacts[0].normal == Vector3.up)//数组的长度就是这个接触集合是否为4（是否落到台子上），数组的法线是否是向上的（小人直立状态）
                {
                    _currentStage = collision.gameObject;//将当前的游戏对象赋值给当前的盒子
                    AddScore(contacts);
                    RandomDirection();
                    SpawnStage();
                    MoveCamera();

                    _enableInput = true;//可以继续进行点击鼠标
                }
                else // 虽然小人跳出去了，但可能由于跳偏了，或者小人的身子不是直立状态
                {
                    OnGameOver();
                }
            }
            else //still on the same box 仍在当前的盒子上跳
            {
                var contacts = collision.contacts;

                //check if player's feet on the stage
                if (contacts.Length == 4 && contacts[0].normal == Vector3.up)
                {
                    _enableInput = true;//还可以继续进行点击，也就是说允许在原地跳的
                }
                else // body just collides with this box
                {
                    OnGameOver();
                }
            }
        }
    }
    private void OnGameOver()
    {
        if (_score > 0)
        {
            //本局游戏结束，如果得分大于0，显示上传分数panel
            SaveScorePanel.SetActive(true);
        }
        else
        {
            //否则直接显示排行榜
            ShowRankPanel();
        }
    }
    private void AddScore(ContactPoint[] contacts)
    {
        if (contacts.Length > 0)
        {
            var hitPoint = contacts[0].point;
            hitPoint.y = 0;

            var stagePos = _currentStage.transform.position;
            stagePos.y = 0;

            var precision = Vector3.Distance(hitPoint, stagePos);
            if (precision < 0.1)
                _lastReward *= 2;
            else
                _lastReward = 1;

            _score += _lastReward;
            TotalScoreText.text = _score.ToString();
            ShowScoreAnimation();
        }
    }
    private void ShowScoreAnimation()
    {
        _isUpdateScoreAnimation = true;
        _scoreAnimationStartTime = Time.time;
        SingleScoreText.text = "+" + _lastReward;
    }
    void UpdateScoreAnimation()
    {
        if (Time.time - _scoreAnimationStartTime > 1)
            _isUpdateScoreAnimation = false;

        var playerScorePos=RectTransformUtility.WorldToScreenPoint(Camera.GetComponent<Camera>(), transform.position);
        SingleScoreText.transform.position = playerScorePos + Vector2.Lerp(Vector2.zero, new Vector2(0, 200), Time.time - _scoreAnimationStartTime);
        SingleScoreText.color = Color.Lerp(Color.black, new Color(0, 0, 0, 0), Time.time - _scoreAnimationStartTime);
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Home))
        {
            Application.Quit();
        }

    }
   
    void RandomDirection()
    {
        var seed = Random.Range(0, 2);
        if (seed == 0)
        {
            _direction = new Vector3(1, 0, 0);
        }
        else
        {
            _direction = new Vector3(0, 0, 1);
        }
        transform.right = _direction;
    }
    
    void MoveCamera()
    {
        Camera.DOMove(transform.position + _cameraRelativePosition, 1);
        //Camera.position = transform.position + _cameraRelativePosition;
    }
    void OnClickSaveButton()
    {
        var nickname = NameField.text;
        AVObject gameScore = new AVObject("GameScore");
        gameScore["score"] = _score;
        gameScore["playerName"] = nickname;
        gameScore.SaveAsync().ContinueWith(_=>
        {
            ShowRankPanel();
         });
        SaveScorePanel.SetActive(false);
    }
    void ShowRankPanel()
    {
        AVQuery<AVObject> query = new AVQuery<AVObject>("GameScore").OrderByDescending("score").Limit(10);
        query.FindAsync().ContinueWith(t => 
        {
            var results = t.Result;
            var scores = new List<string>();
            foreach(var result in results)
            {
                var score = result["playerName"] + ":" + result["score"];
                scores.Add(score);
             }
            MainThreadDispatcher.Send(_=>
            {
                foreach(var score in scores)
               {
                    var item = Instantiate(RankItem);
                    item.SetActive(true);
                    item.GetComponent<Text>().text = score;
                    item.transform.SetParent(RankItem.transform.parent);


               }
               RankPanel.SetActive(true);
            },null);
            
         }
            );

    }
}
