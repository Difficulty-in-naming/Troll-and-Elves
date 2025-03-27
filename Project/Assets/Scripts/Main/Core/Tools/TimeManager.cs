using System;
using UnityEngine;
using System.Collections.Generic;
using LitMotion;
using Panthea.Common;
using Panthea.Utils;
using R3;
using Sirenix.OdinInspector;

namespace EdgeStudio
{


	/// <summary>
	/// 改变当前分数的可用方法列表
	/// </summary>
	public enum MMTimeScaleMethods
	{
		For,
		Reset,
		Unfreeze
	}

	/// <summary>
	/// 插值时间尺度的可能模式。Speed是一种遗留模式，如果你想插值时间尺度，推荐使用Duration模式，它提供了最多的选项和精确度
	/// </summary>
	public enum MMTimeScaleLerpModes
	{
		Speed,
		Duration,
		NoInterpolation
	}

	/// <summary>
	/// 时间尺度事件中可以调整的不同设置
	/// </summary>
	public struct TimeScaleProperties
	{
		public float TimeScale;
		public float Duration;
		public bool TimeScaleLerp;
		public float LerpSpeed;
		public bool Infinite;
		public MMTimeScaleLerpModes TimeScaleLerpMode;
		public Ease TimeScaleLerpCurve;
		public float TimeScaleLerpDuration;
		public bool TimeScaleLerpOnReset;
		public Ease TimeScaleLerpCurveOnReset;
		public float TimeScaleLerpDurationOnReset;
		public override string ToString() => $"REQUESTED ts={TimeScale} time={Duration} lerp={TimeScaleLerp} speed={LerpSpeed} keep={Infinite}";
	}

	public struct TimeScaleEvent
	{
		private static readonly Subject<TimeScaleEvent> Subject = new Subject<TimeScaleEvent>();

		public MMTimeScaleMethods TimeScaleMethod;
		public float TimeScale;
		public float Duration;
		public bool Lerp;
		public float LerpSpeed;
		public bool Infinite;
		public MMTimeScaleLerpModes TimeScaleLerpMode;
		public Ease TimeScaleLerpCurve;
		public float TimeScaleLerpDuration;
		public bool TimeScaleLerpOnReset;
		public Ease TimeScaleLerpCurveOnReset;
		public float TimeScaleLerpDurationOnReset;

		public static IDisposable Register(Action<TimeScaleEvent> callback)
		{
			return Subject.Subscribe(callback);
		}

		public static void Trigger(MMTimeScaleMethods timeScaleMethod, float timeScale, float duration, bool lerp, float lerpSpeed, bool infinite,
			MMTimeScaleLerpModes timeScaleLerpMode = MMTimeScaleLerpModes.Speed, Ease timeScaleLerpCurve = Ease.Linear, float timeScaleLerpDuration = 0.2f,
			bool timeScaleLerpOnReset = false, Ease timeScaleLerpCurveOnReset = Ease.Linear, float timeScaleLerpDurationOnReset = 0.2f)
		{
			Subject.OnNext(new TimeScaleEvent
			{
				TimeScaleMethod = timeScaleMethod,
				TimeScale = timeScale,
				Duration = duration,
				Lerp = lerp,
				LerpSpeed = lerpSpeed,
				Infinite = infinite,
				TimeScaleLerpMode = timeScaleLerpMode,
				TimeScaleLerpCurve = timeScaleLerpCurve,
				TimeScaleLerpDuration = timeScaleLerpDuration,
				TimeScaleLerpOnReset = timeScaleLerpOnReset,
				TimeScaleLerpCurveOnReset = timeScaleLerpCurveOnReset,
				TimeScaleLerpDurationOnReset = timeScaleLerpDurationOnReset
			});
		}

		public static void Unfreeze() => Trigger(MMTimeScaleMethods.Unfreeze, 0f, 0f, false, 0f, false);

		public static void Reset() => Trigger(MMTimeScaleMethods.Reset, 0f, 0f, false, 0f, false);
	}

	public struct FreezeFrameEvent
	{
		private static readonly Subject<float> Subject = new Subject<float>();

		public static IDisposable Register(Action<float> callback) => Subject.Subscribe(callback);

		public static void Trigger(float duration) => Subject.OnNext(duration);
	}

	/// <summary>
	/// 将此组件放在场景中，它会捕获MMFreezeFrameEvents和MMTimeScaleEvents，允许你控制时间流动。
	/// </summary>
	public sealed class TimeManager : Singleton<TimeManager>
	{
		public float NormalTimeScale = 1f;

		public bool UpdateTimescale = true;

		public bool UpdateFixedDeltaTime = true;

		public bool UpdateMaximumDeltaTime = true;

		public float CurrentTimeScale = 1f;

		public float TargetTimeScale = 1f;
		
		private Stack<TimeScaleProperties> mTimeScaleProperties;
		private TimeScaleProperties mCurrentProperty;
		private TimeScaleProperties mResetProperty;
		private float mInitialFixedDeltaTime = 0f;
		private float mInitialMaximumDeltaTime = 0f;
		private float mStartedAt;
		private bool mLerpingBackToNormal = false;
		private float mTimeScaleLastTime = float.NegativeInfinity;
		
		[Button]
		private void TestButtonToSlowDownTime() => TimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0.5f, 3f, true, 1f, false);

		public override void OnCreate()
		{
			base.OnCreate();
			PreInitialization();
			Initialization();
			Observable.EveryUpdate().Subscribe(Update);
		}

		/// <summary>
		/// 我们初始化我们的堆栈
		/// </summary>
		public void PreInitialization() => mTimeScaleProperties = new Stack<TimeScaleProperties>();

		/// <summary>
		/// 在初始化时我们存储初始时间尺度并应用正常时间尺度
		/// </summary>
		public void Initialization()
		{
			TargetTimeScale = NormalTimeScale;
			mInitialFixedDeltaTime = Time.fixedDeltaTime;
			mInitialMaximumDeltaTime = Time.maximumDeltaTime;
			ApplyTimeScale(NormalTimeScale);
		}

		/// <summary>
		/// 在Update中，应用时间尺度并在需要时重置它
		/// </summary>
		private void Update(Unit unit)
		{
			// 如果我们的堆栈中有内容，我们处理它们，否则重置为正常时间尺度
			while (mTimeScaleProperties.Count > 0)
			{
				mCurrentProperty = mTimeScaleProperties.Peek();
				TargetTimeScale = mCurrentProperty.TimeScale;
				mCurrentProperty.Duration -= Time.unscaledDeltaTime;

				mTimeScaleProperties.Pop();
				mTimeScaleProperties.Push(mCurrentProperty);

				if (mCurrentProperty.Duration > 0f || mCurrentProperty.Infinite)
				{
					break; // 保持当前属性值
				}
				else
				{
					Unfreeze(); // 弹出当前属性
				}
			}

			if (mTimeScaleProperties.Count == 0)
			{
				TargetTimeScale = NormalTimeScale;
			}

			// 我们应用我们的时间尺度
			if (mCurrentProperty.TimeScaleLerp)
			{
				if (mCurrentProperty.TimeScaleLerpMode == MMTimeScaleLerpModes.Speed)
				{
					if (mCurrentProperty.LerpSpeed <= 0)
					{
						mCurrentProperty.LerpSpeed = 1;
					}

					ApplyTimeScale(Mathf.Lerp(Time.timeScale, TargetTimeScale, Time.unscaledDeltaTime * mCurrentProperty.LerpSpeed));
				}
				else if (mCurrentProperty.TimeScaleLerpMode == MMTimeScaleLerpModes.Duration)
				{
					float timeSinceStart = Time.unscaledTime - mStartedAt;
					float progress = MathUtils.Remap(timeSinceStart, 0f, mCurrentProperty.TimeScaleLerpDuration, 0f, 1f);
					float delta = EaseUtility.Evaluate(progress, mCurrentProperty.TimeScaleLerpCurve);
					ApplyTimeScale(Mathf.Lerp(Time.timeScale, TargetTimeScale, delta));
					if (timeSinceStart > mCurrentProperty.TimeScaleLerpDuration)
					{
						ApplyTimeScale(TargetTimeScale);
						if (mLerpingBackToNormal)
						{
							mLerpingBackToNormal = false;
							mTimeScaleProperties.Pop();
						}
					}
				}
			}
			else
			{
				ApplyTimeScale(TargetTimeScale);
			}
		}

		/// <summary>
		/// 修改时间尺度和时间属性以匹配新的时间尺度
		/// </summary>
		/// <param name="newValue"></param>
		private void ApplyTimeScale(float newValue)
		{
			// 如果新的时间尺度与上次相同，我们不必更新它
			if (Mathf.Approximately(newValue, mTimeScaleLastTime))
			{
				return;
			}

			if (UpdateTimescale)
			{
				Time.timeScale = newValue;
			}

			if (UpdateFixedDeltaTime && (newValue != 0))
			{
				Time.fixedDeltaTime = mInitialFixedDeltaTime * newValue;
			}

			if (UpdateMaximumDeltaTime)
			{
				Time.maximumDeltaTime = mInitialMaximumDeltaTime * newValue;
			}

			CurrentTimeScale = Time.timeScale;
			mTimeScaleLastTime = CurrentTimeScale;
		}

		/// <summary>
		/// 重置所有堆叠的时间尺度更改并简单地设置时间尺度，直到进一步更改
		/// </summary>
		/// <param name="newTimeScale">新时间尺度。</param>
		private void SetTimeScale(float newTimeScale)
		{
			mTimeScaleProperties.Clear();
			ApplyTimeScale(newTimeScale);
		}

		/// <summary>
		/// 为指定的属性设置时间尺度（持续时间、时间尺度、是否插值以及插值速度）
		/// </summary>
		/// <param name="timeScaleProperties">时间尺度属性。</param>
		private void SetTimeScale(TimeScaleProperties timeScaleProperties)
		{
			if (timeScaleProperties.TimeScaleLerp &&
			    timeScaleProperties.TimeScaleLerpMode == MMTimeScaleLerpModes.Duration)
			{
				timeScaleProperties.Duration = Mathf.Max(timeScaleProperties.Duration, timeScaleProperties.TimeScaleLerpDuration);
				timeScaleProperties.Duration = Mathf.Max(timeScaleProperties.Duration, timeScaleProperties.TimeScaleLerpDurationOnReset);
			}

			mStartedAt = Time.unscaledTime;
			mTimeScaleProperties.Push(timeScaleProperties);
		}

		/// <summary>
		/// 将时间尺度重置为存储的正常时间尺度
		/// </summary>
		public void ResetTimeScale() => SetTimeScale(NormalTimeScale);

		/// <summary>
		/// 将时间尺度重置为最后保存的时间尺度。
		/// </summary>
		public void Unfreeze()
		{
			if (mTimeScaleProperties.Count > 0)
			{
				mResetProperty = mTimeScaleProperties.Peek();
				mTimeScaleProperties.Pop();
			}

			if (mTimeScaleProperties.Count == 0)
			{
				if (mResetProperty is { TimeScaleLerp: true, TimeScaleLerpMode: MMTimeScaleLerpModes.Duration, TimeScaleLerpOnReset: true })
				{
					mLerpingBackToNormal = true;
					TimeScaleEvent.Trigger(MMTimeScaleMethods.For, NormalTimeScale, mResetProperty.TimeScaleLerpDuration, mResetProperty.TimeScaleLerp,
						mResetProperty.LerpSpeed, true, MMTimeScaleLerpModes.Duration, mResetProperty.TimeScaleLerpCurveOnReset,
						mResetProperty.TimeScaleLerpDurationOnReset);
				}
				else
				{
					ResetTimeScale();
				}
			}
		}

		/// <summary>
		/// 立即将时间尺度设置为指定值
		/// </summary>
		/// <param name="newNormalTimeScale">新的正常时间尺度。</param>
		public void SetTimeScaleTo(float newNormalTimeScale)
		{
			TimeScaleEvent.Trigger(MMTimeScaleMethods.For, newNormalTimeScale, 0f, false, 0f, true);
		}

		/// <summary>
		/// 捕获TimeScaleEvents并对其采取行动
		/// </summary>
		/// <param name="timeScaleEvent">MMTimeScaleEvent事件。</param>
		public void OnTimeScaleEvent(TimeScaleEvent timeScaleEvent)
		{
			TimeScaleProperties timeScaleProperty = new TimeScaleProperties();
			timeScaleProperty.TimeScale = timeScaleEvent.TimeScale;
			timeScaleProperty.Duration = timeScaleEvent.Duration;
			timeScaleProperty.TimeScaleLerp = timeScaleEvent.Lerp;
			timeScaleProperty.LerpSpeed = timeScaleEvent.LerpSpeed;
			timeScaleProperty.Infinite = timeScaleEvent.Infinite;
			timeScaleProperty.TimeScaleLerpOnReset = timeScaleEvent.TimeScaleLerpOnReset;
			timeScaleProperty.TimeScaleLerpCurveOnReset = timeScaleEvent.TimeScaleLerpCurveOnReset;
			timeScaleProperty.TimeScaleLerpDurationOnReset = timeScaleEvent.TimeScaleLerpDurationOnReset;
			timeScaleProperty.TimeScaleLerpMode = timeScaleEvent.TimeScaleLerpMode;
			timeScaleProperty.TimeScaleLerpCurve = timeScaleEvent.TimeScaleLerpCurve;
			timeScaleProperty.TimeScaleLerpDuration = timeScaleEvent.TimeScaleLerpDuration;

			switch (timeScaleEvent.TimeScaleMethod)
			{
				case MMTimeScaleMethods.Reset:
					ResetTimeScale();
					break;

				case MMTimeScaleMethods.For:
					SetTimeScale(timeScaleProperty);
					break;

				case MMTimeScaleMethods.Unfreeze:
					Unfreeze();
					break;
			}
		}

		/// <summary>
		/// 当获取冻结帧事件时，我们停止时间
		/// </summary>
		/// <param name="freezeFrameEvent">冻结帧事件。</param>
		public void OnFreezeFrameEvent(float duration)
		{
			TimeScaleProperties properties = new TimeScaleProperties();
			properties.Duration = duration;
			properties.TimeScaleLerp = false;
			properties.LerpSpeed = 0f;
			properties.TimeScale = 0f;
			SetTimeScale(properties);
		}

		private IDisposable FreezeFrameEventDispose;
		private IDisposable TimeScaleEventDispose;

		/// <summary>
		/// 启用时，开始监听FreezeFrame事件
		/// </summary>
		void OnEnable()
		{
			FreezeFrameEventDispose = FreezeFrameEvent.Register(OnFreezeFrameEvent);
			TimeScaleEventDispose = TimeScaleEvent.Register(OnTimeScaleEvent);
		}

		/// <summary>
		/// 禁用时，停止监听FreezeFrame事件
		/// </summary>
		void OnDisable()
		{
			FreezeFrameEventDispose?.Dispose();
			TimeScaleEventDispose?.Dispose();
		}
	}
}