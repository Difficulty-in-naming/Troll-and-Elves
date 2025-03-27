using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EdgeStudio.Tools;
using EdgeStudio.UI;
using EdgeStudio.UI.Common;
using Panthea.Asset;
using Panthea.Common;
using UnityEngine;
using Object = UnityEngine.Object;
namespace EdgeStudio.Manager
{
    namespace Panthea.UI
    {
        public class UIKit : Singleton<UIKit>
        {
            private Dictionary<Type, UIWidget> CacheWidget { get; } = new(50);
            private List<UIBase> Stack { get; } = new(20);
            private Dictionary<Type, UIBase> Dictionary { get; } = new(20);
            private Dictionary<Type, List<UIBase>> RepeatUI { get; } = new(20);
            private ConcurrentDictionary<Type,UniTaskCompletionSource> LoadingUI { get; } = new();

            /// 我们存储UI的Dictionary的Keys类型,以便再后面Update中使用它
            private readonly List<Type> DictKeys = new List<Type>(50);

            /// 同上,这个是用于Update中使用的映射
            private readonly List<Type> RepeatKeys = new List<Type>(50);

            public Canvas RootCanvas { get; private set; }
            public RectTransform RootTransform { get; set; }
            public Vector2 CanvasSize { get; set; }
            public float height => CanvasSize.y;
            public float width => CanvasSize.x;
            private GameObject mPoolGo;
            private Transform mPoolTransform;

            public override void OnCreate()
            {
                base.OnCreate();
                RootCanvas = GameObject.FindWithTag("UI").GetComponent<Canvas>();
                RootTransform = (RectTransform)RootCanvas.transform;
                CanvasSize = RootTransform.sizeDelta;
                // mPoolGo = new GameObject("Pool");
                // mPoolGo.AddComponent<DontDestroyOnLoadAgent>();
                // mPoolTransform = mPoolGo.transform;
            }

            public UIBase GetLastUI()
            {
                if (Stack.Count > 0)
                {
                    return Stack[^1];
                }

                return null;
            }

            public List<UIBase> GetAllStack()
            {
                return Stack;
            }

            public async UniTask<T> Create<T>(UIWidget? uiConfig = null,string subFolder = "") where T : UIBase
            {
                return (T)await Create(typeof(T), uiConfig,subFolder);
            }

            public async UniTask<UIBase> Create(Type type, UIWidget? uiConfig = null,string subFolder = "")
            {
                var ui = await Create(type, MissingArgs.Instance, uiConfig,subFolder);
                return ui;
            }

            public async UniTask<T1> Create<T1, T2>(T2 args, UIWidget? uiConfig = null,string subFolder = "") where T1 : UIBase
            {
                var t = typeof(T1);
                var ui = (T1)await Create(t, args, uiConfig,subFolder);
                return ui;
            }

            private void AddNormalUi(Type t, UIBase uiBase)
            {
                Dictionary[t] = uiBase;

                if (!DictKeys.Contains(t))
                {
                    DictKeys.Add(t);
                }
            }

            private void RemoveNormalUi(UIBase uiBase)
            {
                var t = uiBase.Type;
                Dictionary.Remove(t);
                DictKeys.Remove(t);
            }

            private void AddRepeatUi(Type t, UIBase uiBase)
            {
                if (!RepeatUI.ContainsKey(t))
                {
                    RepeatUI.Add(t, new List<UIBase>(8){uiBase});
                }
                else
                {
                    if (!RepeatUI[t].Contains(uiBase))
                    {
                        RepeatUI[t].Add(uiBase);
                    }
                }

                if (!RepeatKeys.Contains(t))
                {
                    RepeatKeys.Add(t);
                }
            }

            private void RemoveRepeatUi(UIBase uiBase)
            {
                var t = uiBase.Type;
                if (RepeatUI.ContainsKey(t))
                {
                    var list = RepeatUI[t];
                    list.Remove(uiBase);
                    if (list.Count == 0)
                    {
                        RepeatUI.Remove(t);
                        RepeatKeys.Remove(t);
                    }
                }
            }

            public async UniTask<UIBase> Create<T>(Type t, T args, UIWidget? uiConfig = null,string subFolder = "")
            {
                if (LoadingUI.TryGetValue(t, out var uTask))
                {
                    await uTask.Task;
                }
                UIBase uiBase = null;
                try
                {
                    if (RepeatUI.TryGetValue(t, out var repeatList))
                    {
                        var count = repeatList.Count;
                        for (var i = count - 1; i >= 0; i--)
                        {
                            var value = repeatList[i];
                            var com = value.CachedGameObject;
                            if (com)
                            {
                                if (com.activeSelf == false)
                                {
                                    uiBase = value;
                                    break;
                                }
                            }
                            else
                                repeatList.RemoveAt(i);
                        }
                    }

                    UIWidget widget;
                    if (uiBase || Dictionary.TryGetValue(t, out uiBase))
                    {
                        widget = uiBase.Widget;
                        uiBase.visible = true;
                        InitUIWidget(uiConfig ?? widget, uiBase, t);
                        if (!widget.GetControl)
                        {
                            Stack.Remove(uiBase);
                            Stack.Add(uiBase);
                        }
                    }
                    else
                    {
                        //首先查找类型是否重写了UIConfig属性
                        if (!CacheWidget.TryGetValue(t, out widget))
                        {
                            var attribute = (UIWidgetAttribute[])t.GetCustomAttributes(typeof(UIWidgetAttribute), true);
                            widget = attribute.Length > 0 ? attribute[0] : UIWidget.Default;
                            CacheWidget[t] = widget;
                        }

                        if (!widget.Dynamic)
                        {
                            AssetObject ao;
                            var utcs = new UniTaskCompletionSource();
                            try
                            {
                                if (string.IsNullOrEmpty(subFolder))
                                {
                                    var task = AssetsKit.Inst.Load<AssetObject>("prefabs/gui/" + t.Name.ToLower());
                                    LoadingUI.TryAdd(t, utcs);
                                    ao = await task;
                                }
                                else
                                {
                                    var task = AssetsKit.Inst.Load<AssetObject>("prefabs/gui/" + subFolder + "/" + t.Name.ToLower());
                                    LoadingUI.TryAdd(t, utcs);
                                    ao = await task;
                                }
                            }
                            finally
                            {
                                utcs.TrySetResult();
                                LoadingUI.TryRemove(t, out _);
                            }
 
#if HAS_PANTHEA_ASSET
                            var go = ao.Instantiate(RootTransform);
                            uiBase = go.GetComponent<UIBase>();
#else
                            var go = Object.Instantiate(ao.GetGameObject(), RootTransform);
                            uiBase = go.GetComponent<UIBase>();
#endif
                        }
                        else
                        {
                            var go = new GameObject(t.Name);
                            uiBase = (UIBase)go.AddComponent(t);
                            uiBase.Reset();
                        }

                        uiBase.Widget = widget;
                        uiBase.Type = t;
                        if (!widget.GetControl)
                        {
                            Stack.Remove(uiBase);
                            Stack.Add(uiBase);
                        }

                        // await uiBase.PrepareAsset(args);
                        InitUIWidget(uiConfig ?? widget, uiBase, t);
                        uiBase.BaseStart(args);
                    }

                    if (uiBase.IsDisposed) //有可能在Start的时候就销毁了这个界面.我们这里判断一下.如果销毁了这里就要及时阻止往下执行
                    {
                        return null;
                    }

                    if (uiBase.CachedTransform.parent != RootTransform)
                        uiBase.CachedTransform.parent = RootTransform;

                    uiBase.Animate.DoShowAnimation();
                    uiBase.BaseEnable(args);
                    uiBase.OnFocus();
                    if (uiBase.Active && widget.Background)
                        SetBackground(uiBase);

                    GetLastUI()?.OnLostFocus();
                    return uiBase;
                }
                catch (Exception e)
                {
                    Log.Error($"加载类型为{t}的界面错误\n{e}");
                    return null;
                }
            }

            private void InitUIWidget(UIWidget widget, UIBase uiBase, Type t)
            {
                if (!widget.Repeat)
                {
                    RemoveRepeatUi(uiBase);
                    AddNormalUi(t, uiBase);
                }
                else
                {
                    RemoveNormalUi(uiBase);
                    AddRepeatUi(t, uiBase);
                }

                if (widget.SortingOrder != 0)
                {
                    uiBase.Canvas.overrideSorting = true;
                    uiBase.Canvas.sortingOrder = (int)widget.SortingOrder;
                }

                uiBase.Animate ??= new AnimateUI(uiBase);
                uiBase.Animate.EnterAnim = widget.Enter;
                uiBase.Animate.ExitAnim = widget.Exit;
            }

            private bool SetBackground(UIBase ui)
            {
                if (ui.Widget.Background)
                {
                    var background = Create<BackgroundUI>().GetAwaiter().GetResult();
                    background.visible = true;
                    var index = ui.CachedTransform.GetSiblingIndex();
                    var x = background.CachedTransform.GetSiblingIndex();
                    if (background.CachedTransform.GetSiblingIndex() < index)
                    {
                        index--;
                    }
                    background.CachedTransform.SetSiblingIndex(index);
                    return true;
                }

                return false;
            }

            public void SetBackground()
            {
                bool setBackground = false;
                for (int i = Stack.Count - 1; i >= 0; i--)
                {
                    setBackground = SetBackground(Stack[i]);
                    if (setBackground)
                        break;
                }

                if (setBackground == false)
                {
                    //不能走Destroy避免堆栈溢出(递归调用)
                    var black = Find<BackgroundUI>();
                    black?.CloseMySelf();
                }
            }

            /// 这个方法会移除当前界面的上一个界面.
            public void RemoveParent()
            {
                var index = Stack.Count - 2;
                if (index < 0)
                    return;
                var ui = Stack[index];
                Destroy(ui);
            }

            /// 这个方法会移除最顶部的UI.
            public void RemoveActive()
            {
                var index = Stack.Count - 1;
                if (index < 0)
                    return;
                var ui = Stack[index];
                Destroy(ui);
            }

            /// 这个方法会移除所有非GetControl的界面.可以快速的回到主界面窗口
            public void RemoveAll()
            {
                for (int i = Stack.Count - 1; i >= 0; i--)
                {
                    var ui = Stack[i];
                    Destroy(ui);
                    //有可能被销毁的UI中会触发销毁别的UI导致数组错误.我们这里重新矫正一下
                    i = Stack.Count - 1;
                }
            }

            public T Find<T>(bool needActive = true) where T : UIBase
            {
                var type = typeof(T);
                return (T)Find(type, needActive);
            }

            public UIBase Find(Type type, bool needActive = true)
            {
                if (Dictionary.TryGetValue(type, out var result))
                {
                    if (result.Active || !needActive)
                    {
                        return result;
                    }
                }

                return null;
            }

            public UIBase Find(string name, bool needActive = true)
            {
                foreach (var node in Dictionary)
                {
                    var value = node.Value;
                    if (value.Name == name)
                    {
                        if (value.Active || !needActive)
                            return value;
                    }
                }

                return null;
            }

            public List<UIBase> FindAll<T>() where T : UIBase
            {
                var type = typeof(T);
                return FindAll(type);
            }

            public List<UIBase> FindAll(Type type)
            {
                if (RepeatUI.TryGetValue(type, out var list))
                {
                    return list;
                }

                return null;
            }

            ///     Window得销毁不在这里调用而在OnRemoveWindow中触发.这里仅触发Window得动画和Window得池处理
            internal void Destroy(UIBase @base, bool force = false)
            {
                Internal_Destroy(@base, force);
            }

            private void Internal_Destroy(UIBase @base, bool force)
            {
                try
                {
                    if (!@base)
                    {
                        // Log.Warning("销毁的UI界面不能为空,检查是否出现逻辑错误！！！");
                        return;
                    }

                    if (@base.IsDisposed)
                    {
                        /*Stack.Remove(@base);
                        RemoveNormalUi(@base);
                        RemoveRepeatUi(@base);*/
                        return;
                    }

                    if (@base.Widget.Pool && force == false)
                    {
                        var com = @base.CachedTransform;
                        Stack.Remove(@base);
                        @base.BaseDisable();
                        @base.OnLostFocus();
                        GetLastUI()?.OnFocus();
                        @base.visible = false;
                        // com.parent = mPoolTransform;
                        SetBackground();
                    }
                    else
                    {
                        //检测是否是重复类型的如果是则从另一个字典中进入
                        if (@base.Widget.Repeat)
                        {
                            RemoveRepeatUi(@base);
                        }
                        else
                        {
                            if (Dictionary.ContainsKey(@base.Type))
                            {
                                RemoveNormalUi(@base);
                            }
                        }

                        Stack.Remove(@base);
                        @base.BaseDisable();
                        @base.OnLostFocus();
                        @base.BaseDestroy();
                        GetLastUI()?.OnFocus();
                        Object.Destroy(@base.CachedGameObject);
                        SetBackground();
                    }
                }
                catch (Exception e)
                {
                    Log.Error("卸载UI发生错误,如果你是关闭Unity发生得问题可以无视,如果不是请截图给我\n" + e);
                }
            }

            public void Destroy<T>() where T : UIBase
            {
                var type = typeof(T);
                Destroy(type);
            }

            public void Destroy(Type type)
            {
                Dictionary.TryGetValue(type, out var @base);
                @base?.Animate.DoHideAnimation();
            }

            public void DestroyAll<T>() where T : UIBase
            {
                var type = typeof(T);
                DestroyAll(type);
            }

            public void DestroyAll(Type type)
            {
                if (Dictionary.TryGetValue(type, out var ui))
                {
                    ui.Animate.DoHideAnimation();
                }

                if (RepeatUI.TryGetValue(type, out var uiList))
                {
                    int count = uiList.Count;
                    for (int i = count - 1; i >= 0; i--)
                    {
                        uiList[i].DoHideAnimation();
                    }
                }
            }

            /// 切换场景的时候调用的函数.受到DontDestroyOnLoad影响
            public void UnLoadAllUI(bool affectDontDestroy = true)
            {
                int pass = 0;
                for (var index = DictKeys.Count - 1; index >= 0; index--)
                {
                    Type node = DictKeys[index];
                    UIBase value = Dictionary[node];
                    if (!value.Widget.DontDestroyOnLoad || affectDontDestroy == false)
                    {
                        Destroy(value, true);
                    }
                    else
                    {
                        pass++;
                    }

                    //因为有的UI会自己移除他的附加UI.有可能会导致数组越界.
                    index = DictKeys.Count - pass;
                }

                for (var index = RepeatKeys.Count - 1; index >= 0; index--)
                {
                    var node = RepeatKeys[index];
                    var list = RepeatUI[node];
                    int count = list.Count;
                    for (int i = count - 1; i >= 0; i--)
                    {
                        var value = list[i];
                        if (!value.Widget.DontDestroyOnLoad || affectDontDestroy == false)
                        {
                            Destroy(value, true);
                        }
                    }
                }
            }
        }
    }
}