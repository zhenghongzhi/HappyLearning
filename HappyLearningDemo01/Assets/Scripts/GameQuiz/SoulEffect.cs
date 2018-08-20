/*
http://www.cgsoso.com/forum-211-1.html

CG搜搜 Unity3d 每日Unity3d插件免费更新 更有VIP资源！

CGSOSO 主打游戏开发，影视设计等CG资源素材。

插件如若商用，请务必官网购买！

daily assets update for try.

U should buy the asset from home store if u use it in your project!
*/

using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

/// <summary>
/// Soul Trail Effect
/// </summary>
public class SoulEffect : MonoBehaviour {
    Transform tr;
    Vector3 startPos;
    public float posX;

	void Start () {
        tr = transform;
        startPos = tr.localPosition;
        //startPos.z = -1f;
        //posX = ((UnityEngine.Random.Range(0, 2) % 2) * 2 - 1)*1f;
        DoSkillEffect();
    }

    // HOTween OnComplete Method
    void OnDoneEffect()
    {
        Destroy(gameObject, 1f);
    }

    // HOTween Motion Effect
    public void DoSkillEffect()
    {
        Vector3[] path = new Vector3[] { startPos, new Vector3(posX, 2f, startPos.z), new Vector3(posX * -2f, 6f, startPos.z) };
        tr.localPosition = startPos;
        HOTween.To(tr, 1f, new TweenParms().Prop("localPosition", new PlugVector3Path(path, EaseType.Linear, true)).OnComplete(OnDoneEffect));
    }
}
