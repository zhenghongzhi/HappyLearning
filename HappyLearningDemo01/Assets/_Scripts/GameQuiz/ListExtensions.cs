/*
http://www.cgsoso.com/forum-211-1.html

CG搜搜 Unity3d 每日Unity3d插件免费更新 更有VIP资源！

CGSOSO 主打游戏开发，影视设计等CG资源素材。

插件如若商用，请务必官网购买！

daily assets update for try.

U should buy the asset from home store if u use it in your project!
*/

using System;
using System.Collections.Generic;

/// <summary>
/// Array Shuffle
/// </summary>
public static class ListExtensions {
	public static void Shuffle<T>(this IList<T> list) {
		var randomNumber = new Random(DateTime.Now.Millisecond);
		var n = list.Count;
		while (n > 1) {
			n--;
			var k = randomNumber.Next(n + 1);
			var value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}
}
