﻿// The MIT License (MIT)

// Copyright 2015 Siney/Pangweiwei siney@yeah.net
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace SLua
{
	using UnityEngine;
	using System.Collections;
	using LuaInterface;
	using SLua;
	using System;

	public class LuaCoroutine : LuaObject
	{

		static MonoBehaviour mb;

		static public void reg(IntPtr l, MonoBehaviour m)
		{
			mb = m;
			reg(l, Yield, "UnityEngine");
		}

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		static public int Yield(IntPtr l)
		{
			try
			{
				if (LuaDLL.lua_pushthread(l) == 1)
				{
					LuaDLL.luaL_error(l, "should put Yield call into lua coroutine.");
					return 0;
				}
				object y = checkObj(l, 1);

				Action act = () =>
				{
#if LUA_5_3
					if(LuaDLL.lua_resume(l,IntPtr.Zero,0) > (int) LuaThreadStatus.LUA_YIELD )
#else
					if (LuaDLL.lua_resume(l, 0) > (int) LuaThreadStatus.LUA_YIELD )
#endif
                    {
						LuaObject.pushTry(l);
						LuaDLL.lua_pushvalue(l, -2);
						LuaDLL.lua_call(l, 1, 0);
						LuaDLL.lua_pop(l, 1);
                    }
				};

				mb.StartCoroutine(yieldReturn(y, act));
#if LUA_5_3
				return LuaDLL.luaS_yield(l, 0);
#else
				return LuaDLL.lua_yield(l, 0);
#endif
			}
			catch (Exception e)
			{
				LuaDLL.luaL_error(l, e.ToString());
				return 0;
			}
		}

		static public IEnumerator yieldReturn(object y, Action act)
		{
			if (y is IEnumerator)
				yield return mb.StartCoroutine((IEnumerator)y);
			else
				yield return y;
			act();
		}

	}
}
