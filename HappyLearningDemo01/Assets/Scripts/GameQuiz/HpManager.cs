/*
http://www.cgsoso.com/forum-211-1.html

CG搜搜 Unity3d 每日Unity3d插件免费更新 更有VIP资源！

CGSOSO 主打游戏开发，影视设计等CG资源素材。

插件如若商用，请务必官网购买！

daily assets update for try.

U should buy the asset from home store if u use it in your project!
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// HP & MP Manager
/// </summary>
public class HpManager : MonoBehaviour {
    // HP bar, MP bar
    public Slider hpBar, mpBar;

    // Max State 
    public int hpMax = 100;
    public int mpMax = 100;

    // Current State
    int hp = 100;
    int mp = 100;

    // Init HP State
    public void InitHp()
    {
        SetHp(hpMax);
    }

    // Init MP State
    public void InitMp()
    {
        SetHp(mpMax);
    }

    // Set Damage on HP State
    public void DoDamageHp(int point)
    {
        SetHp(hp - point);
    }

    // Set Recover on HP State
    public void DoSaveHp(int point)
    {
        SetHp(hp + point);
    }

    // Set Recover on MP State
    public void DoSaveMp(int point)
    {
        SetMp(mp + point);
    }

    // Set HP State
    public void SetHp(int point)
    {
        hp = Mathf.Clamp(point, 0, hpMax);
        if (hpBar)
            hpBar.value = (float)hp / (float)hpMax;
    }

    // Set MP State
    public void SetMp(int point)
    {
        mp = Mathf.Clamp(point, 0, mpMax);
        if (mpBar)
            mpBar.value = (float)mp / (float)mpMax;
    }

}
