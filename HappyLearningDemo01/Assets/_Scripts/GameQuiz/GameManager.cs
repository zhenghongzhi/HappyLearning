using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

/// <summary>
/// Game Manager Main Class.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Effect Prefab
    public GameObject goodEffect, badEffect, soulEffect, happyEffect;
    // Actor Animator Component
    public Animator friendAnimator, enemyAnimator;
    // Actor HP Manager Component
    public HpManager friendHpMan, enemyHpMan;

    // Save Start Position
    Vector3 friendPos, enemyPos, friendHpPos, enemyHpPos, shieldPos;
    Transform friendHpGroup, enemyHpGroup, shieldGroup;

    // Save Question & Answer Display Position
    Transform questionTf;
    Transform[] answerTfs;
    Text questionLabel;
    Text[] answerLabels;

    // Quiz List Array
    List<QuizData> quizList;
    int quizTotal;
    int quizIndex = 0;

    [HideInInspector]
    public int quizLength = 0;

    // Quiz Condition
    bool quizOn = true;

    void Awake()
    {
        // Set mobile display res.
        Screen.SetResolution(640, 960, false); 
    }
    void Start()
    {
        InitGame();
        HideGame();
        //StartGame();
    }

    // Hide game interface for next quiz
    void HideGame()
    {
        ClearQuiz();
        Vector3 pos = friendPos;
        friendAnimator.transform.localPosition = new Vector3(pos.x * 3f, pos.y, pos.z);
        pos = enemyPos;

        enemyAnimator.transform.localPosition = new Vector3(pos.x * 3f, pos.y, pos.z);
        pos = friendHpPos;

        friendHpGroup.localPosition = new Vector3(pos.x * 5f, pos.y, pos.z);
        pos = enemyHpPos;

        enemyHpGroup.localPosition = new Vector3(pos.x * 5f, pos.y, pos.z);
        shieldGroup.localScale = new Vector3(2f, 2f, 1f);

        pos = shieldPos;
        shieldGroup.localPosition = new Vector3(pos.x, 0f, pos.z);
    }

    // Start game & draw next quiz
    public void StartGame()
    {
        IntroGame();
        DrawQuiz();
    }

    // Draw Quiz
    void DrawQuiz()
    {
        HideQuiz();
        StartCoroutine(DelayActoin(1f, () =>
        {
            SetQuiz();
            ShowQuiz();
        }));
    }

    // Init Quiz List
    void QuizInit()
    {
        quizList = new List<QuizData>();
        List<string> champs = new List<string>();
        string[,] dic = LolSkillData.dic;
        for (int i = 0; i < dic.GetLength(0); i++)
        {
            string champ = dic[i,2];
            if (!champs.Contains(champ)) champs.Add(champ);
        }
        for (int i = 0; i < dic.GetLength(0); i++)
        {
            string idx = dic[i, 0];
            string skill = dic[i, 1];
            string champ = dic[i, 2];
            QuizData quiz = new QuizData();

            int t = champs.IndexOf(champ);
            List<int> ansIdList = new List<int>();
            Hashtable ansValList = new Hashtable();
            ansValList[0] = champs[t];
            ansValList[1] = champs[(t+1)%champs.Count];
            ansValList[2] = champs[(t+2)%champs.Count];
            ansValList[3] = champs[(t+3)%champs.Count];
            for (int j = 0; j < 4; j++) ansIdList.Add(j);
            ansIdList.Shuffle();
            for (int j = 0; j < 4; j++)
                if (ansIdList[j] == 0) quiz.correct = j;
            quiz.answer1 = "1. " + ansValList[ansIdList[0]] as string;
            quiz.answer2 = "2. " + ansValList[ansIdList[1]] as string;
            quiz.answer3 = "3. " + ansValList[ansIdList[2]] as string;
            quiz.answer4 = "4. " + ansValList[ansIdList[3]] as string;
            quiz.question = skill + "?";
            quiz.id = int.Parse(idx);
            quizList.Add(quiz);
        }
        quizTotal = quizList.Count;
    }

    // Init Quiz Game
    void InitGame()
    {
        friendHpMan.InitHp();
        enemyHpMan.InitHp();
        questionTf = GameObject.Find("Question").transform;
        questionLabel = questionTf.GetComponentInChildren<Text>();
        answerLabels = new Text[4];
        answerTfs = new Transform[4];
        int i = 0;
        foreach (Transform tf in GameObject.Find("Answers").transform)
        {
            answerTfs[i] = tf;
            answerLabels[i] = tf.GetComponentInChildren<Text>();
            i++;
        }
        QuizInit();

        shieldGroup = GameObject.Find("ShieldGroup").transform;
        shieldPos = shieldGroup.localPosition;
        friendPos = friendAnimator.transform.localPosition;
        enemyPos = enemyAnimator.transform.localPosition;
        friendHpGroup = friendHpMan.hpBar.transform.parent;
        enemyHpGroup = enemyHpMan.hpBar.transform.parent;
        friendHpPos = friendHpGroup.localPosition;
        enemyHpPos = enemyHpGroup.localPosition;
    }

    // Draw quiz intro motion
    void IntroGame()
    {
        friendAnimator.CrossFade("Walk", 0.2f);
        enemyAnimator.CrossFade("Walk", 0.2f);

        Vector3 pos = friendPos;
        friendAnimator.transform.localPosition = new Vector3(pos.x * 3f, pos.y, pos.z);
        TweenParms parms = new TweenParms().Prop("localPosition", friendPos).Ease(EaseType.Linear).OnComplete(OnFriendStop);
        HOTween.To(friendAnimator.transform, 2f, parms);
        
        pos = enemyPos;
        enemyAnimator.transform.localPosition = new Vector3(pos.x * 3f, pos.y, pos.z);
        parms = new TweenParms().Prop("localPosition", pos).Ease(EaseType.Linear).OnComplete(OnEnemyStop);
        HOTween.To(enemyAnimator.transform, 2f, parms);

        pos = shieldPos;
        shieldGroup.localPosition = new Vector3(pos.x, 0f, pos.z);
        parms = new TweenParms().Prop("localPosition", pos).Delay(1f);
        HOTween.To(shieldGroup, 1f, parms);

        shieldGroup.localScale = new Vector3(2f, 2f, 1f);
        parms = new TweenParms().Prop("localScale", new Vector3(0.8f, 0.8f, 1f));
        HOTween.To(shieldGroup, 1f, parms);

        pos = friendHpPos;
        friendHpGroup.localPosition = new Vector3(pos.x * 5f, pos.y, pos.z);
        parms = new TweenParms().Prop("localPosition", pos).Delay(0.5f);
        HOTween.To(friendHpGroup, 1f, parms);

        pos = enemyHpPos;
        enemyHpGroup.localPosition = new Vector3(pos.x * 5f, pos.y, pos.z);
        parms = new TweenParms().Prop("localPosition", pos).Delay(0.5f);
        HOTween.To(enemyHpGroup, 1f, parms);
    }

    // Stop Friend Actor Animation.
    void OnFriendStop()
    {
        friendAnimator.CrossFade("Idle", 0.2f);
    }

    // Stop Enemy Actor Animation.
    void OnEnemyStop()
    {
        enemyAnimator.CrossFade("Idle", 0.2f);
    }

    // Clear Quiz Display
    void ClearQuiz()
    {
        questionTf.localScale = new Vector3(0f, 1f, 1f);
        int i = -1;
        foreach (Transform tf in answerTfs)
        {
            tf.localPosition = new Vector3(1000f * i, tf.localPosition.y, tf.localPosition.z);
            i *= -1;
        }
    }

    // Hide quiz motion 
    void HideQuiz()
    {
        TweenParms parms = new TweenParms().Prop("localScale", new Vector3(0f, 1f, 1f));
        HOTween.To(questionTf, 0.5f, parms);
        int i = -1;
        foreach (Transform tf in answerTfs)
        {
            parms = new TweenParms().Prop("localPosition", new Vector3(1000f * i, tf.localPosition.y, tf.localPosition.z));
            HOTween.To(tf, 0.5f, parms);
            i *= -1;
        }
    }

    // Display Quiz Question like typewriter
    void TypeQuiz()
    {
        questionLabel.text = quizList[quizIndex].question.Substring(0, quizLength);
    }

    // Show Quiz Display Motion
    void ShowQuiz()
    {
        TweenParms parms = new TweenParms().Prop("localScale", new Vector3(1f, 1f, 1f));
        HOTween.To(questionTf, 0.5f, parms);
        int i = 1;
        foreach (Transform tf in answerTfs)
        {
            parms = new TweenParms().Prop("localPosition", new Vector3(0f, tf.localPosition.y, tf.localPosition.z)).Delay(0.3f * i++);
            HOTween.To(tf, 0.5f, parms);
        }
        quizOn = true;

        quizLength = 0;
        parms = new TweenParms().Prop("quizLength", quizList[quizIndex].question.Length).Ease(EaseType.Linear).OnUpdate(TypeQuiz);
        HOTween.To(this, 1f, parms);
    }

    // Make String Max Length
    string QuizMakeString(string str) 
    {
        return (str.Length > 41) ? str.Substring(0, 40) : str;
    }

    // set Quiz Answer & Question variables
    void SetQuiz()
    {
        quizIndex = Random.Range(0, quizTotal) % quizTotal;
        QuizData item = quizList[quizIndex];
        answerLabels[0].text = QuizMakeString(item.answer1);
        answerLabels[1].text = QuizMakeString(item.answer2);
        answerLabels[2].text = QuizMakeString(item.answer3);
        answerLabels[3].text = QuizMakeString(item.answer4);
        questionLabel.text = item.question;
    }
    
	void Update () {
        // Quit Application
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        if (Input.GetButton("Fire1"))
        {
            DrawQuiz();
        }
	}

    void ClickAnswer(int no)
    {
        if (!quizOn) return;
        quizOn = false;
        QuizData item = quizList[quizIndex];

        // Is answer correct?
        if (item.correct == no) 
        {
            // Display good Effect
            Instantiate(goodEffect);
            // Display soul trail effect
            GameObject go = Instantiate(soulEffect) as GameObject;
            go.GetComponent<SoulEffect>().posX = -1f;
            // Display Happy Effect
            StartCoroutine(DelayActoin(0.6f, () =>
            {
                go = Instantiate(happyEffect, new Vector3(-0.7f, 1f, 0f), Quaternion.identity) as GameObject;
                enemyHpMan.DoDamageHp(10);
            }));
            // Display Actor's motion
            friendAnimator.CrossFade("Good", 0.2f);
            enemyAnimator.CrossFade("Bad", 0.2f);
        }
        else
        {
            // Display Bad Effect
            Instantiate(badEffect);
            // Display soul trail effect
            GameObject go = Instantiate(soulEffect) as GameObject;
            go.GetComponent<SoulEffect>().posX = 1f;
            // Display Happy Effect
            StartCoroutine(DelayActoin(0.6f, () =>
            {
                go = Instantiate(happyEffect, new Vector3(0.7f, 1f, 0f), Quaternion.identity) as GameObject;
                friendHpMan.DoDamageHp(10);
            }));
            // Display Actor's motion
            friendAnimator.CrossFade("Bad", 0.2f);
            enemyAnimator.CrossFade("Good", 0.2f);
        }

        StartCoroutine(DelayActoin(3f, () =>
        {
            DrawQuiz();
        }));
    }

    public void OnClickAnswer1()
    {
        ClickAnswer(0);
    }
    public void OnClickAnswer2()
    {
        ClickAnswer(1);
    }
    public void OnClickAnswer3()
    {
        ClickAnswer(2);
    }
    public void OnClickAnswer4()
    {
        ClickAnswer(3);
    }

    // time delay action
    public IEnumerator DelayActoin(float dtime, System.Action callback)
    {
        yield return new WaitForSeconds(dtime);
        callback();
    }
}
