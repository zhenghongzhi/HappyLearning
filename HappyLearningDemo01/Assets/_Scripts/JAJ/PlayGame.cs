using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[Serializable]//向云端服务器进行远程方法调用 设计为可序列化的类
class GameScore
{
    //定义云端数据库的分数和用户名
    public int score;
    public string playerName;
}

class QueryRankResult
{
    //定义所有游戏分数的列表
    public List<GameScore> results = null;
}

public class PlayGame : MonoBehaviour {

    /*公有变量*/
    // 小人跳跃时，决定远近的一个参数
    public float Factor;
    // 盒子随机最远的距离
    public float MaxDistance = 5;

    // 所在弹簧
    public Transform Spring;
    //单词画布的位置
    // 领袖
    //public Transform Head;

    public Transform WordCanvasPos;
    

    // 第一个盒子物体
    public GameObject Stage;
    //第一个单词
    public GameObject WordCanvas;
    // 粒子效果
    public GameObject Particle;
   // 盒子仓库，可以放上各种盒子的prefab，用于动态生成。
    public GameObject[] BoxTemplates;
    //单词仓库， 可以放上各种单词，用于动态生成。
    public GameObject[] WordTemplates;
    // 保存分数面板
    public GameObject SaveScorePanel;
    // 排行数据的姓名
    public GameObject RankName;
    // 排行数据的分数
    public GameObject RankScore;
    // 排行榜面板
    public GameObject RankPanel;


    // 左上角总分的UI组件
    public Text TotalScoreText;
    // 飘分效果的单个UI组件
    public Text SingleScoreText;
    //单词显示的UI
    public Text WordText;
    //
    // 名字输入框
    public InputField NameField;
    // 保存按钮
    public Button SaveButton;
    // 重新开始按钮
    public Button RestartButton;

    //LeanCloudApp密钥
    public string LeanCloudAppId;
    public string LeanCloudAppKey;



    /*私有变量*/
    //按下鼠标的起始时间
    private float _startTime;
    //飘分UI的起始时间
    private float _scoreAnimationStartTime;

    //玩家每次游戏的得分
    private int _score;
    //玩家的附加分数
    private int _lastReward = 1;

    //使玩家具有物理属性的刚体
    private Rigidbody _rigidbody;

    //玩家实时着落的当前台子
    private GameObject _currentStage;
    //实时显示的单词
    private GameObject _currentWord;

    //相机与玩家的相对位置
    private Vector3 _cameraRelativePosition;
    //玩家向前（X轴）移动一个单位的向量
    private Vector3 _direction = new Vector3(1, 0, 0);   

    //检查是否显示飘分的效果
    private bool _isUpdateScoreAnimation;
    //检查是否点击鼠标的状态
    private bool _enableInput = true;

    //封装好的软件开发工具包类
    private LeanCloudRestAPI _leanCloud;

    // Use this for initialization
    void Start () {
        //给刚体变量赋值 获取刚体组件
        _rigidbody = GetComponent<Rigidbody>();
        //改变玩家的质量中心位置
        _rigidbody.centerOfMass = new Vector3(0, 0, 0);

        //将初始台子赋值给实时台子
        _currentStage = Stage;
        //将初始单词赋给实时单词
        _currentWord = WordCanvas;

        //调用生成台子的方法
        SpawnStage();

        //调用生成单词的方法
        ShowWords();

        //给相机与小人的相对位置赋值
        _cameraRelativePosition = Camera.main.transform.position - transform.position;

        //给上传分数的UI链接相应的方法
        SaveButton.onClick.AddListener(OnClickSaveButton);
        //给重新开始的UI链接场景管理的方法
        RestartButton.onClick.AddListener(() => { SceneManager.LoadScene(0); });

        //获取外部的LeaCloud密钥 
        _leanCloud = new LeanCloudRestAPI(LeanCloudAppId, LeanCloudAppKey);
    }

    private void ShowWords()
    {

        //定义一个单词的预制体用来存放原始台子集合中的其中一个
        GameObject wordprefab;

        //判断单词集合的数量大于0则执行
        if (WordTemplates.Length > 0)
        {
            // 从单词库中随机取出一个单词赋值给前面的预制体以便进行动态生成
            wordprefab = WordTemplates[Random.Range(0, BoxTemplates.Length)];
            //调用动态生成的方法生成单词 并用一个变量存取便于后续操作
            var word = Instantiate(wordprefab,WordCanvasPos);
            //调整生成的单词的位置 随机位置生成
            word.transform.position = _currentStage.transform.position + _direction * Random.Range(1.1f, MaxDistance) + new Vector3(0, 0.3f, 0);

            //将克隆的单词赋实时单词
            _currentWord = word;
        }
        //若存放单词模型的集合内没有就把当前小人所在的单词赋值给预制体
        else
        {
            wordprefab = WordCanvas;
            //调用动态生成的方法生成单词 并用一个变量存取便于后续操作
            var word = Instantiate(wordprefab);
            //调整生成的单词的位置 随机位置生成
            word.transform.position = _currentStage.transform.position + _direction * Random.Range(1.5f, MaxDistance) + new Vector3(0, 0.3f, 0);

            //将克隆的单词赋实时单词
            _currentWord = word;
           

        }
       

        
        //定义一个可以随机缩放的随机数
        //var randomScale = Random.Range(1, 1.2f);
        //随机调整单词的大小
        //word.transform.localScale = new Vector3(randomScale, 1, randomScale);

        //重载函数 或 重载方法 随机调整台子的颜色
        //word.GetComponent<Renderer>().material.color =
          //  new Color(Random.Range(0f, 1), Random.Range(0f, 1), Random.Range(0f, 1));
    }

    //处理点击上传分数按钮
    private void OnClickSaveButton()
    {
        //将名字输入框下的text组件赋值给字符串类型的变量
        var nickname = NameField.text;

        //检测到用户未输入任何字符串则结束本次方法的调用 需要用户填写后再次点击
        if (nickname.Length == 0)
            return;

        //创建一个GameScore分数对象 初始化
        GameScore gameScore = new GameScore
        {
            //将每次游戏的得分和每次的昵称赋值给云端数据库类的分数变量和用户名变量
            score = _score,
            playerName = nickname
        };

        //通过封装好的SDK进行异步保存（不用等用户所有的操作都完成 就可以相应用户的请求）
        StartCoroutine(_leanCloud.Create("GameScore", JsonUtility.ToJson(gameScore, false), ShowRankPanel));
        //上传后关闭这个进行上传用户名和分数的面板
        SaveScorePanel.SetActive(false);
    }

    //获取GameScore数据对象，降序排列取前10个数据进行显示
    private void ShowRankPanel()
    {        
        //初始化 Dictionary为键类型所使用的比较容器
        var param = new Dictionary<string, object>();
        //将指定的键添加到Dictionary中
        param.Add("order", "-score");
        param.Add("limit", 10);

        //通过协程依次获取数据并在比较容器Dictionary进行查找所有保存的结果 并按一定顺序排序 ？=>
        StartCoroutine(_leanCloud.Query("GameScore", param, 
        t =>
        {
            //从云端调取所有的结果
            var results = JsonUtility.FromJson<QueryRankResult>(t);
            //初始化比较容器中的列表
            var scores = new List<KeyValuePair<string, string>>();

            //依次查找云端所有的数据并将数据转化为字符串添加到列表scores中
            foreach (var result in results.results)
            {
                scores.Add(
                    new KeyValuePair<string, string>(result.playerName, result.score.ToString())
                          );
            }
            //依次查找scores列表中的结果进行分类显示
            foreach (var score in scores)
            {
                //生成排名用户的昵称
                var item = Instantiate(RankName);
                //进行状态显示
                item.SetActive(true);
                //获取昵称下的text组件进行赋值
                item.GetComponent<Text>().text = score.Key;
                //将用户昵称设置在父物体下
                item.transform.SetParent(RankName.transform.parent);

                //同理设置对应昵称的分数
                item = Instantiate(RankScore);
                item.SetActive(true);
                item.GetComponent<Text>().text = score.Value;
                item.transform.SetParent(RankScore.transform.parent);
            }
            //显示排名面板
            RankPanel.SetActive(true);
        }));
    }

    //生成盒子的方法
    private void SpawnStage()
    {
        //定义一个台子的预制体用来存放原始台子集合中的其中一个
        GameObject prefab;

        //判断台子集合的数量大于0则执行
        if (BoxTemplates.Length > 0)
        {
            // 从盒子库中随机取出一个盒子赋值给前面的预制体以便进行动态生成
            prefab = BoxTemplates[Random.Range(0, BoxTemplates.Length)];
        }
        //若存放台子模型的集合内没有就把当前小人所在的台子赋值给预制体
        else
        {
            prefab = Stage;
        }
        //调用动态生成的方法并用一个变量存取便于后续操作
        var stage = Instantiate(prefab);
        //调整生成的台子的位置 随机位置生成
        stage.transform.position = _currentStage.transform.position + _direction * Random.Range(1.5f, MaxDistance);

        //定义一个可以随机缩放的随机数
        var randomScale = Random.Range(1.5f, 2f);
        //随机调整台子的大小
        stage.transform.localScale = new Vector3(randomScale, 0.5f, randomScale);

        // 重载函数 或 重载方法 随机调整台子的颜色
        stage.GetComponent<Renderer>().material.color =
            new Color(Random.Range(0f, 1), Random.Range(0f, 1), Random.Range(0f, 1));
    }

    // Update is called once per frame
    void Update () {


        //当检查到按下鼠标时执行
        if (_enableInput)
        {
            //检测按下鼠标
            if (Input.GetMouseButtonDown(0))
            {
                //将此时的每帧渲染的时间赋值给按下鼠标的起始时间
                _startTime = Time.time;
                //打开粒子效果
                Particle.SetActive(true);
            }

            //检测抬起鼠标
            if (Input.GetMouseButtonUp(0))
            {
                // 计算总共按下鼠标的时长
                var elapse = Time.time - _startTime;
                //执行跳跃的方法
                OnJump(elapse);
                //关闭粒子效果
                Particle.SetActive(false);

                //还原玩家的形状
                //Spring.transform.DOScale(0.1f, 0.2f);
                //Monkey.transform.DOLocalMoveY(0.29f, 0.2f);

                //还原盒子的形状
                _currentStage.transform.DOLocalMoveY(-0.25f, 0.2f);
                _currentStage.transform.DOScaleY(0.5f, 0.2f);

                //让之前的单词消失
                DestroyWord();
               


                //将是否按下鼠标的状态设置为否
                _enableInput = false;
            }

            // 处理整个按下鼠标时小人和盒子的动画效果
            if (Input.GetMouseButton(0))
            {
                //添加限定，盒子达到一定的缩放程度执行
                if (_currentStage.transform.localScale.y > 0.3)
                {
                    //弹簧沿着Y轴进行比例变换
                    //Spring.transform.localScale += new Vector3(1, -1, 1) * 0.05f * Time.deltaTime;
                    //小猴子的位置进行变换
                    //Monkey.transform.localPosition += new Vector3(0, -1, 0) * 0.1f * Time.deltaTime;

                    //当前台子进行比例和位置变换
                    _currentStage.transform.localScale += new Vector3(0, -1, 0) * 0.15f * Time.deltaTime;
                    _currentStage.transform.localPosition += new Vector3(0, -1, 0) * 0.15f * Time.deltaTime;
                }
            }
        }


        //在没有显示飘分效果时执行更新飘分动画的方法
        if (_isUpdateScoreAnimation)
            UpdateScoreAnimation();
    }

    private void DestroyWord()
    {
        _currentWord.transform.DOLocalMoveZ(40, 0.2f);
    }

    //更新飘分效果
    private void UpdateScoreAnimation()
    {
        //控制最短飘分时间为1秒执行关闭更新飘分动画的状态
        if (Time.time - _scoreAnimationStartTime > 1)
            _isUpdateScoreAnimation = false;

        //实现飘分效果
        //将小人的世界坐标转换成屏幕坐标并获取
        var playerScreenPos =
            RectTransformUtility.WorldToScreenPoint(Camera.main, transform.position);
        //控制飘分内容的位置移动以及移动的时间
        SingleScoreText.transform.position = playerScreenPos + 
                                             Vector2.Lerp(Vector2.zero, new Vector2(0, 200),
                                             Time.time - _scoreAnimationStartTime);
        //控制飘分内容的颜色的渐变由黑色到透明
        SingleScoreText.color = Color.Lerp(Color.black, new Color(0, 0, 0, 0), Time.time - _scoreAnimationStartTime);
    }

    //跳跃
    private void OnJump(float elapse)
    {
        //给玩家施加一个前上方的力
        _rigidbody.AddForce(new Vector3(0, 5f, 0) + (_direction) * elapse * Factor, ForceMode.Impulse);
        //给玩家添加旋转的效果
        transform.DOLocalRotate(new Vector3(0, 0, -360), 0.6f, RotateMode.LocalAxisAdd); 
    }

    
    //碰撞进入检测
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Ground")
        {
            OnGameOver();
        }
        else
        {
            if (_currentStage != collision.gameObject)//条件 当前的小人所在的盒子不是碰撞后的游戏对象 表示碰到的不是原盒子
            {
                var contacts = collision.contacts;//var定义数组变量contact 将被碰撞后的物体的接触点集（集合）赋值给这个数组变量

                //检测玩家是否成功平稳地落在接触物上
                if (contacts.Length == 4 && contacts[0].normal == Vector3.up)//数组的长度就是这个接触集合是否为1（是否落到台子上），数组的法线是否是向上的（小人直立状态）
                {
                    _currentStage = collision.gameObject;//将当前的游戏对象赋值给当前的盒子                                                                         
                    AddScore(contacts);
                    RandomDirection();
                    SpawnStage();
                    ShowWords();
                    //_currentWord.transform.DOLocalMoveY(-0.3f, 0.2f);//还原单词的形状
                    MoveCamera();

                    _enableInput = true;//可以继续进行点击鼠标
                }
                else // 虽然小人跳出去了，但可能由于跳偏了，或者小人的身子不是直立状态
                {
                    OnGameOver();
                }
            }
            else //玩家仍在当前的盒子上跳
            {
                var contacts = collision.contacts;

                //检测玩家是否成功平稳地落在接触物上
                if (contacts.Length == 4 && contacts[0].normal == Vector3.up)
                {
                    _enableInput = true;//还可以继续进行点击，也就是说允许在原地跳的
                }
                else // 出现意外
                {
                    OnGameOver();
                }
            }
        }
    }

    
    //游戏结束的方法
    private void OnGameOver()
    {
        if (_score > 0)
        {
            //本局游戏结束，如果得分大于0，显示上传分数panel
            SaveScorePanel.SetActive(true);
            _enableInput = false;
        }
        else
        {
            //否则直接显示排行榜
            ShowRankPanel();
            _enableInput = false;
        }
    }

    //加分的方法
    private void AddScore(ContactPoint[] contacts)
    {
        if (contacts.Length > 0)
        {
            //获取小人与台子的接触点位置 
            var hitPoint = contacts[0].point;
            //将Y值设为0 使位置变成同一平面
            hitPoint.y = 0;

            //获取台子的位置 同理设置为同一平面
            var stagePos = _currentStage.transform.position;
            stagePos.y = 0;

            //求接触点和台子位置的相差距离
            var precision = Vector3.Distance(hitPoint, stagePos);
            //相对距离值小于0.1也就是根据玩家跳跃的精准度进行设置加分码值的大小
            if (precision < 0.1)
                _lastReward *= 2;
            else
                _lastReward = 1;

            //给分数赋值
            _score += _lastReward;
            //将分数转化成字符串赋值给UI的text组件
            TotalScoreText.text = _score.ToString();
            //显示飘分动画
            ShowScoreAnimation();
        }
    }

    //随机方向设定
    void RandomDirection()
    {
        //定义一个随机种子包含0，1
        var seed = Random.Range(0, 2);
        //给控制方向的变量进行随机赋值
        _direction = seed == 0 ? new Vector3(1, 0, 0) : new Vector3(0, 0, 1);
        //让玩家始终朝着正确的方向
        transform.right = _direction;
    }

    //相机移动方法
    void MoveCamera()
    {
        Camera.main.transform.DOMove(transform.position + _cameraRelativePosition, 1);
    }

   //显示飘分动画的方法
    private void ShowScoreAnimation()
    {
        //改变更新动画的状态
        _isUpdateScoreAnimation = true;
        //设置更新动画的开始时间
        _scoreAnimationStartTime = Time.time;
        //给飘分内容进行复制
        SingleScoreText.text = "+" + _lastReward;
    }


}
