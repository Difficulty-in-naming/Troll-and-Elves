using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Panthea.Utils
{
	public static class MathUtils
	{
		/// <summary>
		/// 将一个值从一个范围重新映射到另一个范围，并确保结果在目标范围内。
		/// </summary>
		/// <param name="value">需要重新映射的输入值。</param>
		/// <param name="from1">输入范围的下限。</param>
		/// <param name="to1">输入范围的上限。</param>
		/// <param name="from2">目标范围的下限。</param>
		/// <param name="to2">目标范围的上限。</param>
		/// <returns>重新映射后的值，确保在目标范围 [from2, to2] 内。</returns>
		public static float RemapClamped(float value, float from1, float to1, float from2, float to2)
		{
			// 计算输入值在输入范围内的归一化位置（比例）。
			float normalizedValue = (value - from1) / (to1 - from1);

			// 将归一化值映射到目标范围。
			float remappedValue = normalizedValue * (to2 - from2) + from2;

			// 返回重新映射后的值，确保其在目标范围内。
			return remappedValue;
		}

		/// <summary>
		/// 将一个值从一个范围重新映射到另一个范围，并确保结果在目标范围内（适用于 double 类型）。
		/// </summary>
		/// <param name="value">需要重新映射的输入值。</param>
		/// <param name="from1">输入范围的下限。</param>
		/// <param name="to1">输入范围的上限。</param>
		/// <param name="from2">目标范围的下限。</param>
		/// <param name="to2">目标范围的上限。</param>
		/// <returns>重新映射后的值，确保在目标范围 [from2, to2] 内。</returns>
		public static double RemapClamped(double value, double from1, double to1, double from2, double to2)
		{
			// 计算输入值在输入范围内的归一化位置（比例）。
			double normalizedValue = (value - from1) / (to1 - from1);

			// 将归一化值映射到目标范围。
			double remappedValue = normalizedValue * (to2 - from2) + from2;

			// 返回重新映射后的值，确保其在目标范围内。
			return remappedValue;
		}

		public static bool IsInRange(float value, float min, float max)
		{
			return value >= min && value <= max;
		}

		public static bool IsInRange(double value, double min, double max)
		{
			return value >= min && value <= max;
		}

		/// <summary>
		/// 权重算法
		/// </summary>
		/// <param name="list">需要计算权重的列表</param>
		/// <param name="outInt">根据哪个变量计算权重</param>
		/// <param name="nums">返回权重数量</param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static List<T> WeightMath<T>(List<T> list, Func<T, int> outInt, int nums, uint? seed = null)
		{
			BetterRandom Random = new BetterRandom();
			if (seed.HasValue)
			{
				Random = new BetterRandom(seed.Value);
			}

			if (list == null || list.Count <= 0 || nums == 0)
			{
				return new List<T>();
			}

			nums = Mathf.Min(list.Count, nums);

			var totalWeight = 0;
			var cumulativeWeights = new List<int>(list.Count);
			foreach (var item in list)
			{
				totalWeight += outInt(item);
				cumulativeWeights.Add(totalWeight);
			}

			if (totalWeight <= 0)
			{
				throw new ArgumentException("Total weight must be greater than 0.");
			}

			var result = new List<T>(nums);
			for (var i = 0; i < nums; i++)
			{
				var randomValue = seed.HasValue ? Random.NextInt(1, totalWeight) : UnityEngine.Random.Range(1, totalWeight);
				var index = BinarySearch(cumulativeWeights, randomValue);
				result.Add(list[index]);
			}

			return result;
		}

		private static int BinarySearch(IReadOnlyList<int> cumulativeWeights, int value)
		{
			var min = 0;
			var max = cumulativeWeights.Count - 1;

			while (min <= max)
			{
				var mid = (min + max) / 2;
				if (cumulativeWeights[mid] == value)
				{
					return mid;
				}

				if (cumulativeWeights[mid] < value)
				{
					min = mid + 1;
				}
				else
				{
					max = mid - 1;
				}
			}

			return min;
		}

		/// <summary>
		/// 权重算法
		/// </summary>
		/// <param name="list">需要计算权重的列表</param>
		/// <param name="outInt">根据哪个变量计算权重</param>
		/// <param name="nums">返回权重数量</param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T WeightMath<T>(List<T> list, Func<T, int> outInt, uint? seed = null)
		{
			BetterRandom Random = new BetterRandom();
			if (seed.HasValue)
			{
				Random = new BetterRandom(seed.Value);
			}

			if (list is not { Count: > 0 })
			{
				return default;
			}

			int listCount = list.Count;
			int totalWeight = 0;

			for (int i = 0; i < listCount; i++)
			{
				totalWeight += outInt(list[i]);
			}

			if (totalWeight <= 0)
			{
				throw new ArgumentException("Total weight must be greater than 0.");
			}

			var randomValue = seed.HasValue ? Random.NextInt(1, totalWeight) : UnityEngine.Random.Range(1, totalWeight);
			int currentWeight = 0;
			foreach (var item in list)
			{
				currentWeight += outInt(item);
				if (randomValue < currentWeight)
				{
					return item;
				}
			}

			return list[^1];
		}

		/// <summary>
		/// 权重算法
		/// </summary>
		/// <param name="list">需要计算权重的列表</param>
		/// <param name="outInt">根据哪个变量计算权重</param>
		/// <param name="seed">随机种子</param>
		/// <typeparam name="T"></typeparam>
		/// <returns>返回权重数量</returns>
		public static int WeightIndex<T>(List<T> list, Func<T, int> outInt, uint? seed = null)
		{
			BetterRandom Random = new BetterRandom();
			if (seed.HasValue)
			{
				Random = new BetterRandom(seed.Value);
			}

			if (list == null || list.Count == 0)
			{
				throw new ArgumentException("List cannot be null or empty.", nameof(list));
			}

			int listCount = list.Count;
			int totalWeight = 0;

			for (int i = 0; i < listCount; i++)
			{
				totalWeight += outInt(list[i]);
			}

			if (totalWeight <= 0)
			{
				throw new ArgumentException("Total weight must be greater than 0.");
			}

			var randomValue = seed.HasValue ? Random.NextInt(1, totalWeight) : UnityEngine.Random.Range(1, totalWeight);
			int currentWeight = 0;

			for (int i = 0; i < listCount; i++)
			{
				currentWeight += outInt(list[i]);
				if (randomValue <= currentWeight)
				{
					return i;
				}
			}

			throw new InvalidOperationException("Failed to select a random index.");
		}

		/// <summary>
		/// 权重算法
		/// </summary>
		/// <param name="array">需要计算权重的数组</param>
		/// <param name="outInt">根据哪个变量计算权重</param>
		/// <param name="nums">返回权重数量</param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T WeightMath<T>(T[] array, Func<T, int> outInt, out int index, uint? seed = null)
		{
			BetterRandom Random = new BetterRandom();
			if (seed.HasValue)
			{
				Random = new BetterRandom(seed.Value);
			}

			if (array is not { Length: > 0 })
			{
				index = -1;
				return default;
			}

			int arrayLength = array.Length;
			int totalWeight = 0;

			for (int i = 0; i < arrayLength; i++)
			{
				totalWeight += outInt(array[i]);
			}

			if (totalWeight <= 0)
			{
				throw new ArgumentException("Total weight must be greater than 0.");
			}


			var randomValue = seed.HasValue ? Random.NextInt(1, totalWeight) : UnityEngine.Random.Range(1, totalWeight);
			int currentWeight = 0;

			for (int i = 0; i < arrayLength; i++)
			{
				currentWeight += outInt(array[i]);
				if (randomValue < currentWeight)
				{
					index = i;
					return array[i];
				}
			}

			index = array.Length - 1;
			return array[^1];
		}

		/// <summary>
		/// 权重算法
		/// </summary>
		/// <param name="array">需要计算权重的数组</param>
		/// <param name="outInt">根据哪个变量计算权重</param>
		/// <param name="nums">返回权重数量</param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T WeightMath<T>(T[] array, Func<T, int> outInt, uint? seed = null) => WeightMath(array, outInt, out _, seed);

		public static Vector3 Parabola(Vector3 start, Vector3 end, float t)
		{
			Vector3 height = -Vector3.up * ((end - start).magnitude * 0.3f);
			Vector3 minPoint = (start + end) * 0.5f + height;

			var P0 = start;
			var P1 = minPoint;
			var P2 = end;
			var u = 1 - t;
			var tt = t * t;
			var uu = u * u;
			var p = uu * P0;
			p += 2 * u * t * P1;
			p += tt * P2;
			return p;
		}

		public static Vector3 Parabola(Vector3 start, Vector3 end, Vector3 height, Vector3 minPoint, float t)
		{
			var P0 = start;
			var P1 = minPoint;
			var P2 = end;
			var u = 1 - t;
			var tt = t * t;
			var uu = u * u;
			var p = uu * P0;
			p += 2 * u * t * P1;
			p += tt * P2;
			return p;
		}

		private static int[] ShuffleArray(int[] array)
		{
			var n = array.Length;
			while (n > 1)
			{
				n--;
				var k = Random.Range(0, n);
				(array[k], array[n]) = (array[n], array[k]);
			}

			return array;
		}

		/// <summary>
		/// 内部方法，用于计算弹簧效应的瞬时速度
		/// </summary>
		/// <param name="currentValue">当前值</param>
		/// <param name="targetValue">目标值</param>
		/// <param name="velocity">当前速度</param>
		/// <param name="damping">阻尼系数（0.01-1，值越大弹性越小）</param>
		/// <param name="frequency">频率（Hz，每秒震动周期数）</param>
		/// <param name="deltaTime">时间增量（通常使用Time.deltaTime）</param>
		/// <returns>计算后的新速度</returns>
		private static float SpringVelocity(float currentValue, float targetValue, float velocity, float damping, float frequency, float deltaTime)
		{
			frequency = frequency * 2f * Mathf.PI;
			float f2 = frequency * frequency;
			float d2 = 2.0f * damping * frequency;
			float x = currentValue - targetValue;
			float acceleration = -f2 * x - d2 * velocity;
			velocity += deltaTime * acceleration;
			return velocity;
		}


		/// <summary>
		/// 将浮点数弹性过渡至目标值（支持引用参数原地修改）
		/// </summary>
		/// <param name="currentValue">当前待弹性处理的数值（引用参数）</param>
		/// <param name="targetValue">目标值</param>
		/// <param name="velocity">速度参考值（引用参数）</param>
		/// <param name="damping">阻尼系数（0.01-1）</param>
		/// <param name="frequency">震动频率（Hz）</param>
		/// <param name="deltaTime">时间增量</param>
		public static void Spring(ref float currentValue, float targetValue, ref float velocity, float damping, float frequency, float deltaTime)
		{
			float fixedDeltaTime = 1.0f / 60.0f;
			float accumulator = deltaTime;
			while (accumulator > 0f)
			{
				float step = Mathf.Min(accumulator, fixedDeltaTime);
				velocity = SpringVelocity(currentValue, targetValue, velocity, damping, frequency, step);
				currentValue += step * velocity;
				accumulator -= step;
			}
		}

		/// <summary>
		/// 将浮点数弹性过渡至目标值（支持引用参数原地修改）
		/// </summary>
		/// <param name="currentValue">当前待弹性处理的数值（引用参数）</param>
		/// <param name="targetValue">目标值</param>
		/// <param name="velocity">速度参考值（引用参数）</param>
		/// <param name="damping">阻尼系数（0.01-1）</param>
		/// <param name="frequency">震动频率（Hz）</param>
		/// <param name="deltaTime">时间增量</param>
		public static void Spring(ref Vector2 currentValue, Vector2 targetValue, ref Vector2 velocity, float damping, float frequency, float deltaTime)
		{
			float fixedDeltaTime = 1.0f / 60.0f;
			float accumulator = deltaTime;
			while (accumulator > 0f)
			{
				float step = Mathf.Min(accumulator, fixedDeltaTime);
				velocity.x = SpringVelocity(currentValue.x, targetValue.x, velocity.x, damping, frequency, step);
				velocity.y = SpringVelocity(currentValue.y, targetValue.y, velocity.y, damping, frequency, step);
				currentValue += step * velocity;
				accumulator -= step;
			}
		}

		/// <summary>
		/// 将浮点数弹性过渡至目标值（支持引用参数原地修改）
		/// </summary>
		/// <param name="currentValue">当前待弹性处理的数值（引用参数）</param>
		/// <param name="targetValue">目标值</param>
		/// <param name="velocity">速度参考值（引用参数）</param>
		/// <param name="damping">阻尼系数（0.01-1）</param>
		/// <param name="frequency">震动频率（Hz）</param>
		/// <param name="deltaTime">时间增量</param>
		public static void Spring(ref Vector3 currentValue, Vector3 targetValue, ref Vector3 velocity, float damping, float frequency, float deltaTime)
		{
			float fixedDeltaTime = 1.0f / 60.0f;
			float accumulator = deltaTime;
			while (accumulator > 0f)
			{
				float step = Mathf.Min(accumulator, fixedDeltaTime);
				velocity.x = SpringVelocity(currentValue.x, targetValue.x, velocity.x, damping, frequency, step);
				velocity.y = SpringVelocity(currentValue.y, targetValue.y, velocity.y, damping, frequency, step);
				velocity.z = SpringVelocity(currentValue.z, targetValue.z, velocity.z, damping, frequency, step);
				currentValue += step * velocity;
				accumulator -= step;
			}
		}

		/// <summary>
		/// 将浮点数弹性过渡至目标值（支持引用参数原地修改）
		/// </summary>
		/// <param name="currentValue">当前待弹性处理的数值（引用参数）</param>
		/// <param name="targetValue">目标值</param>
		/// <param name="velocity">速度参考值（引用参数）</param>
		/// <param name="damping">阻尼系数（0.01-1）</param>
		/// <param name="frequency">震动频率（Hz）</param>
		/// <param name="deltaTime">时间增量</param>
		public static void Spring(ref Vector4 currentValue, Vector4 targetValue, ref Vector4 velocity, float damping, float frequency, float deltaTime)
		{
			float fixedDeltaTime = 1.0f / 60.0f;
			float accumulator = deltaTime;
			while (accumulator > 0f)
			{
				float step = Mathf.Min(accumulator, fixedDeltaTime);
				velocity.x = SpringVelocity(currentValue.x, targetValue.x, velocity.x, damping, frequency, step);
				velocity.y = SpringVelocity(currentValue.y, targetValue.y, velocity.y, damping, frequency, step);
				velocity.z = SpringVelocity(currentValue.z, targetValue.z, velocity.z, damping, frequency, step);
				velocity.w = SpringVelocity(currentValue.w, targetValue.w, velocity.w, damping, frequency, step);
				currentValue += step * velocity;
				accumulator -= step;
			}
		}

		/// <summary>
		/// 计算插值速率的核心方法
		/// </summary>
		/// <param name="rate">插值速率（0-1）</param>
		/// <returns>基于帧率修正后的实际插值系数</returns>
		private static float LerpRate(float rate, float deltaTime)
		{
			rate = Mathf.Clamp01(rate);
			float invRate = -Mathf.Log(1.0f - rate, 2.0f) * 60f;
			return Mathf.Pow(2.0f, -invRate * deltaTime);
		}

		/// <summary>
		/// 将浮点数向目标值以指定速率进行平滑插值
		/// </summary>
		/// <param name="value">当前插值基准值</param>
		/// <param name="target">目标终点值</param>
		/// <param name="rate">插值速率（0-1，1表示瞬时完成）</param>
		/// <param name="deltaTime">时间增量（通常使用 Time.deltaTime）</param>
		/// <returns>经过当前帧率修正后的插值结果</returns>
		public static float Lerp(float value, float target, float rate, float deltaTime)
		{
			if (deltaTime == 0f)
			{
				return value;
			}

			return Mathf.Lerp(target, value, LerpRate(rate, deltaTime));
		}

		/// <summary>
		/// 将浮点数向目标值以指定速率进行平滑插值
		/// </summary>
		/// <param name="value">当前插值基准值</param>
		/// <param name="target">目标终点值</param>
		/// <param name="rate">插值速率（0-1，1表示瞬时完成）</param>
		/// <param name="deltaTime">时间增量（通常使用 Time.deltaTime）</param>
		/// <returns>经过当前帧率修正后的插值结果</returns>
		public static Vector2 Lerp(Vector2 value, Vector2 target, float rate, float deltaTime)
		{
			if (deltaTime == 0f)
			{
				return value;
			}

			return Vector2.Lerp(target, value, LerpRate(rate, deltaTime));
		}

		/// <summary>
		/// 将浮点数向目标值以指定速率进行平滑插值
		/// </summary>
		/// <param name="value">当前插值基准值</param>
		/// <param name="target">目标终点值</param>
		/// <param name="rate">插值速率（0-1，1表示瞬时完成）</param>
		/// <param name="deltaTime">时间增量（通常使用 Time.deltaTime）</param>
		/// <returns>经过当前帧率修正后的插值结果</returns>
		public static Vector3 Lerp(Vector3 value, Vector3 target, float rate, float deltaTime)
		{
			if (deltaTime == 0f)
			{
				return value;
			}

			return Vector3.Lerp(target, value, LerpRate(rate, deltaTime));
		}

		/// <summary>
		/// 将浮点数向目标值以指定速率进行平滑插值
		/// </summary>
		/// <param name="value">当前插值基准值</param>
		/// <param name="target">目标终点值</param>
		/// <param name="rate">插值速率（0-1，1表示瞬时完成）</param>
		/// <param name="deltaTime">时间增量（通常使用 Time.deltaTime）</param>
		/// <returns>经过当前帧率修正后的插值结果</returns>
		public static Vector4 Lerp(Vector4 value, Vector4 target, float rate, float deltaTime)
		{
			if (deltaTime == 0f)
			{
				return value;
			}

			return Vector4.Lerp(target, value, LerpRate(rate, deltaTime));
		}

		/// <summary>
		/// 将浮点数向目标值以指定速率进行平滑插值
		/// </summary>
		/// <param name="value">当前插值基准值</param>
		/// <param name="target">目标终点值</param>
		/// <param name="rate">插值速率（0-1，1表示瞬时完成）</param>
		/// <param name="deltaTime">时间增量（通常使用 Time.deltaTime）</param>
		/// <returns>经过当前帧率修正后的插值结果</returns>
		public static Quaternion Lerp(Quaternion value, Quaternion target, float rate, float deltaTime)
		{
			if (deltaTime == 0f)
			{
				return value;
			}

			return Quaternion.Lerp(target, value, LerpRate(rate, deltaTime));
		}

		/// <summary>
		/// 将浮点数向目标值以指定速率进行平滑插值
		/// </summary>
		/// <param name="value">当前插值基准值</param>
		/// <param name="target">目标终点值</param>
		/// <param name="rate">插值速率（0-1，1表示瞬时完成）</param>
		/// <param name="deltaTime">时间增量（通常使用 Time.deltaTime）</param>
		/// <returns>经过当前帧率修正后的插值结果</returns>
		public static Color Lerp(Color value, Color target, float rate, float deltaTime)
		{
			if (deltaTime == 0f)
			{
				return value;
			}

			return Color.Lerp(target, value, LerpRate(rate, deltaTime));
		}

		/// <summary>
		/// 将浮点数向目标值以指定速率进行平滑插值
		/// </summary>
		/// <param name="value">当前插值基准值</param>
		/// <param name="target">目标终点值</param>
		/// <param name="rate">插值速率（0-1，1表示瞬时完成）</param>
		/// <param name="deltaTime">时间增量（通常使用 Time.deltaTime）</param>
		/// <returns>经过当前帧率修正后的插值结果</returns>
		public static Color32 Lerp(Color32 value, Color32 target, float rate, float deltaTime)
		{
			if (deltaTime == 0f)
			{
				return value;
			}

			return Color32.Lerp(target, value, LerpRate(rate, deltaTime));
		}

		/// <summary>
		/// 将浮点数限定在最小最大值范围内，边界控制通过clampMin/clampMax布尔值分别启用
		/// </summary>
		/// <param name="value">待处理的原始值</param>
		/// <param name="min">最小边界值</param>
		/// <param name="max">最大边界值</param>
		/// <param name="clampMin">是否启用最小值限制</param>
		/// <param name="clampMax">是否启用最大值限制</param>
		/// <returns>经过范围限定处理的浮点数值</returns>
		public static float Clamp(float value, float min, float max, bool clampMin, bool clampMax)
		{
			float returnValue = value;
			if (clampMin && (returnValue < min))
			{
				returnValue = min;
			}

			if (clampMax && (returnValue > max))
			{
				returnValue = max;
			}

			return returnValue;
		}

		/// <summary>
		/// 将浮点数四舍五入到最近的半值：1, 1.5, 2, 2.5 等
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static float RoundToNearestHalf(float a)
		{
			return a = a - (a % 0.5f);
		}

		/// <summary>
		/// 根据给定的二维方向返回一个四元数
		/// </summary>
		/// <param name="direction"></param>
		/// <returns></returns>
		public static Quaternion LookAt2D(Vector2 direction)
		{
			var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
			return Quaternion.AngleAxis(angle, Vector3.forward);
		}

		/// <summary>
		/// 将 Vector3 转换为 Vector2
		/// </summary>
		/// <returns>转换后的 Vector2。</returns>
		/// <param name="target">要转换的 Vector3。</param>
		public static Vector2 Vector3ToVector2(Vector3 target)
		{
			return new Vector2(target.x, target.y);
		}

		/// <summary>
		/// 将 Vector2 转换为 Vector3，z 值为 0
		/// </summary>
		/// <returns>转换后的 Vector3。</returns>
		/// <param name="target">要转换的 Vector2。</param>
		public static Vector3 Vector2ToVector3(Vector2 target)
		{
			return new Vector3(target.x, target.y, 0);
		}

		/// <summary>
		/// 将 Vector2 转换为 Vector3，并指定 z 值
		/// </summary>
		/// <returns>转换后的 Vector3。</returns>
		/// <param name="target">要转换的 Vector2。</param>
		/// <param name="newZValue">新的 Z 值。</param>
		public static Vector3 Vector2ToVector3(Vector2 target, float newZValue)
		{
			return new Vector3(target.x, target.y, newZValue);
		}

		/// <summary>
		/// 对 Vector3 的所有分量进行四舍五入
		/// </summary>
		/// <returns>四舍五入后的 Vector3。</returns>
		/// <param name="vector">要四舍五入的 Vector3。</param>
		public static Vector3 RoundVector3(Vector3 vector)
		{
			return new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));
		}

		/// <summary>
		/// 返回两个定义的 Vector2 之间的随机 Vector2
		/// </summary>
		/// <returns>随机的 Vector2。</returns>
		/// <param name="min">最小值。</param>
		/// <param name="max">最大值。</param>
		public static Vector2 RandomVector2(Vector2 minimum, Vector2 maximum)
		{
			return new Vector2(UnityEngine.Random.Range(minimum.x, maximum.x),
				UnityEngine.Random.Range(minimum.y, maximum.y));
		}

		/// <summary>
		/// 返回两个定义的 Vector3 之间的随机 Vector3
		/// </summary>
		/// <returns>随机的 Vector3。</returns>
		/// <param name="min">最小值。</param>
		/// <param name="max">最大值。</param>
		public static Vector3 RandomVector3(Vector3 minimum, Vector3 maximum)
		{
			return new Vector3(UnityEngine.Random.Range(minimum.x, maximum.x),
				UnityEngine.Random.Range(minimum.y, maximum.y),
				UnityEngine.Random.Range(minimum.z, maximum.z));
		}

		/// <summary>
		/// 返回指定半径的圆上的随机点
		/// </summary>
		/// <param name="circleRadius">圆的半径。</param>
		/// <returns>圆上的随机点。</returns>
		public static Vector2 RandomPointOnCircle(float circleRadius)
		{
			return UnityEngine.Random.insideUnitCircle.normalized * circleRadius;
		}

		/// <summary>
		/// 返回指定半径的球体上的随机点
		/// </summary>
		/// <param name="sphereRadius">球体的半径。</param>
		/// <returns>球体上的随机点。</returns>
		public static Vector3 RandomPointOnSphere(float sphereRadius)
		{
			return UnityEngine.Random.onUnitSphere * sphereRadius;
		}

		/// <summary>
		/// 围绕给定的枢轴点旋转一个点
		/// </summary>
		/// <returns>旋转后的点位置。</returns>
		/// <param name="point">要旋转的点。</param>
		/// <param name="pivot">枢轴点的位置。</param>
		/// <param name="angle">要旋转的角度。</param>
		public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, float angle)
		{
			angle = angle * (Mathf.PI / 180f);
			float rotatedX = Mathf.Cos(angle) * (point.x - pivot.x) - Mathf.Sin(angle) * (point.y - pivot.y) + pivot.x;
			float rotatedY = Mathf.Sin(angle) * (point.x - pivot.x) + Mathf.Cos(angle) * (point.y - pivot.y) + pivot.y;
			return new Vector3(rotatedX, rotatedY, 0);
		}

		/// <summary>
		/// 围绕给定的枢轴点旋转一个点
		/// </summary>
		/// <returns>旋转后的点位置。</returns>
		/// <param name="point">要旋转的点。</param>
		/// <param name="pivot">枢轴点的位置。</param>
		/// <param name="angles">旋转角度（Vector3）。</param>
		public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angle)
		{
			// 获取从点到枢轴的方向
			Vector3 direction = point - pivot;
			// 旋转方向
			direction = Quaternion.Euler(angle) * direction;
			// 确定旋转后的点位置
			point = direction + pivot;
			return point;
		}

		/// <summary>
		/// 围绕给定的枢轴点旋转一个点
		/// </summary>
		/// <returns>旋转后的点位置。</returns>
		/// <param name="point">要旋转的点。</param>
		/// <param name="pivot">枢轴点的位置。</param>
		/// <param name="quaternion">旋转四元数。</param>
		public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion quaternion)
		{
			// 获取从点到枢轴的方向
			Vector3 direction = point - pivot;
			// 旋转方向
			direction = quaternion * direction;
			// 确定旋转后的点位置
			point = direction + pivot;
			return point;
		}

		/// <summary>
		/// 将 Vector2 旋转指定角度（以度为单位）并返回
		/// </summary>
		/// <returns>旋转后的 Vector2。</returns>
		/// <param name="vector">要旋转的向量。</param>
		/// <param name="angle">角度（度）。</param>
		public static Vector2 RotateVector2(Vector2 vector, float angle)
		{
			if (angle == 0)
			{
				return vector;
			}

			float sinus = Mathf.Sin(angle * Mathf.Deg2Rad);
			float cosinus = Mathf.Cos(angle * Mathf.Deg2Rad);

			float oldX = vector.x;
			float oldY = vector.y;
			vector.x = (cosinus * oldX) - (sinus * oldY);
			vector.y = (sinus * oldX) + (cosinus * oldY);
			return vector;
		}

		/// <summary>
		/// 计算并返回两个向量之间的角度，范围为 360°
		/// </summary>
		/// <returns>角度。</returns>
		/// <param name="vectorA">向量 A。</param>
		/// <param name="vectorB">向量 B。</param>
		public static float AngleBetween(Vector2 vectorA, Vector2 vectorB)
		{
			float angle = Vector2.Angle(vectorA, vectorB);
			Vector3 cross = Vector3.Cross(vectorA, vectorB);

			if (cross.z > 0)
			{
				angle = 360 - angle;
			}

			return angle;
		}

		/// <summary>
		/// 计算并返回两个 Vector3 之间的方向，用于检查一个向量是否指向另一个向量的左侧或右侧
		/// </summary>
		/// <returns>方向。</returns>
		/// <param name="vectorA">向量 A。</param>
		/// <param name="vectorB">向量 B。</param>
		/// <param name="up">上方向。</param>
		public static float AngleDirection(Vector3 vectorA, Vector3 vectorB, Vector3 up)
		{
			Vector3 cross = Vector3.Cross(vectorA, vectorB);
			float direction = Vector3.Dot(cross, up);

			return direction;
		}

		/// <summary>
		/// 返回点与直线之间的距离
		/// </summary>
		/// <returns>点与直线之间的距离。</returns>
		/// <param name="point">点。</param>
		/// <param name="lineStart">直线起点。</param>
		/// <param name="lineEnd">直线终点。</param>
		public static float DistanceBetweenPointAndLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
		{
			return Vector3.Magnitude(ProjectPointOnLine(point, lineStart, lineEnd) - point);
		}

		/// <summary>
		/// 将点投影到直线上（垂直投影）并返回投影点
		/// </summary>
		/// <returns>投影点。</returns>
		/// <param name="point">点。</param>
		/// <param name="lineStart">直线起点。</param>
		/// <param name="lineEnd">直线终点。</param>
		public static Vector3 ProjectPointOnLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
		{
			Vector3 rhs = point - lineStart;
			Vector3 vector2 = lineEnd - lineStart;
			float magnitude = vector2.magnitude;
			Vector3 lhs = vector2;
			if (magnitude > 1E-06f)
			{
				lhs = (Vector3)(lhs / magnitude);
			}

			float num2 = Mathf.Clamp(Vector3.Dot(lhs, rhs), 0f, magnitude);
			return (lineStart + ((Vector3)(lhs * num2)));
		}

		/// <summary>
		/// 返回所有传入的整数的和
		/// </summary>
		/// <param name="thingsToAdd">要相加的整数。</param>
		public static int Sum(params int[] thingsToAdd)
		{
			int result = 0;
			for (int i = 0; i < thingsToAdd.Length; i++)
			{
				result += thingsToAdd[i];
			}

			return result;
		}

		/// <summary>
		/// 返回掷骰子的结果，骰子的面数为指定的面数
		/// </summary>
		/// <returns>骰子的结果。</returns>
		/// <param name="numberOfSides">骰子的面数。</param>
		public static int RollADice(int numberOfSides)
		{
			return (UnityEngine.Random.Range(1, numberOfSides + 1));
		}

		/// <summary>
		/// 根据指定的百分比返回随机成功的结果。
		/// 例如：我有 20% 的几率做某事，Chance(20) > true，成功！
		/// </summary>
		/// <param name="percent">成功的百分比。</param>
		public static bool Chance(int percent)
		{
			return (UnityEngine.Random.Range(0, 100) <= percent);
		}

		/// <summary>
		/// 从 "from" 移动到 "to"，移动指定的量并返回相应的值
		/// </summary>
		/// <param name="from">起始值。</param>
		/// <param name="to">目标值。</param>
		/// <param name="amount">移动的量。</param>
		public static float Approach(float from, float to, float amount)
		{
			if (Mathf.Approximately(from, to))
			{
				return from;
			}

			if (from < to)
			{
				from += amount;
				if (from > to)
				{
					return to;
				}
			}

			if (from > to)
			{
				from -= amount;
				if (from < to)
				{
					return to;
				}
			}

			return from;
		}

		/// <summary>
		/// 将值 x 从区间 [A,B] 重新映射到区间 [C,D] 中的比例值
		/// </summary>
		/// <param name="x">要重新映射的值。</param>
		/// <param name="A">包含 x 值的区间 [A,B] 的最小边界。</param>
		/// <param name="B">包含 x 值的区间 [A,B] 的最大边界。</param>
		/// <param name="C">目标区间 [C,D] 的最小边界。</param>
		/// <param name="D">目标区间 [C,D] 的最大边界。</param>
		public static float Remap(float x, float A, float B, float C, float D)
		{
			float remappedValue = C + (x - A) / (B - A) * (D - C);
			return remappedValue;
		}

		/// <summary>
		/// 将角度限制在最小和最大角度之间（所有角度以度为单位）
		/// </summary>
		/// <param name="angle">要限制的角度。</param>
		/// <param name="minimumAngle">最小角度。</param>
		/// <param name="maximumAngle">最大角度。</param>
		/// <returns>限制后的角度。</returns>
		public static float ClampAngle(float angle, float minimumAngle, float maximumAngle)
		{
			if (angle < -360)
			{
				angle += 360;
			}

			if (angle > 360)
			{
				angle -= 360;
			}

			return Mathf.Clamp(angle, minimumAngle, maximumAngle);
		}

		/// <summary>
		/// 将值四舍五入到指定的小数位数
		/// </summary>
		/// <param name="value">要四舍五入的值。</param>
		/// <param name="numberOfDecimals">小数位数。</param>
		/// <returns>四舍五入后的值。</returns>
		public static float RoundToDecimal(float value, int numberOfDecimals)
		{
			if (numberOfDecimals <= 0)
			{
				return Mathf.Round(value);
			}
			else
			{
				return Mathf.Round(value * 10f * numberOfDecimals) / (10f * numberOfDecimals);
			}
		}

		/// <summary>
		/// 将传入的值四舍五入到参数数组中最接近的值
		/// </summary>
		/// <param name="value">要四舍五入的值。</param>
		/// <param name="possibleValues">可能的值数组。</param>
		/// <param name="pickSmallestDistance">是否选择最小距离。</param>
		/// <returns>四舍五入后的值。</returns>
		public static float RoundToClosest(float value, float[] possibleValues, bool pickSmallestDistance = false)
		{
			if (possibleValues.Length == 0)
			{
				return 0f;
			}

			float closestValue = possibleValues[0];

			foreach (float possibleValue in possibleValues)
			{
				float closestDistance = Mathf.Abs(closestValue - value);
				float possibleDistance = Mathf.Abs(possibleValue - value);

				if (closestDistance > possibleDistance)
				{
					closestValue = possibleValue;
				}
				else if (closestDistance == possibleDistance)
				{
					if ((pickSmallestDistance && closestValue > possibleValue) || (!pickSmallestDistance && closestValue < possibleValue))
					{
						closestValue = (value < 0) ? closestValue : possibleValue;
					}
				}
			}

			return closestValue;
		}

		/// <summary>
		/// 根据传入的角度返回一个 Vector3
		/// </summary>
		/// <param name="angle">角度。</param>
		/// <param name="additionalAngle">附加角度。</param>
		/// <returns>方向向量。</returns>
		public static Vector3 DirectionFromAngle(float angle, float additionalAngle)
		{
			angle += additionalAngle;

			Vector3 direction = Vector3.zero;
			direction.x = Mathf.Sin(angle * Mathf.Deg2Rad);
			direction.y = 0f;
			direction.z = Mathf.Cos(angle * Mathf.Deg2Rad);
			return direction;
		}

		/// <summary>
		/// 根据传入的角度返回一个 Vector3（二维）
		/// </summary>
		/// <param name="angle">角度。</param>
		/// <param name="additionalAngle">附加角度。</param>
		/// <returns>方向向量。</returns>
		public static Vector3 DirectionFromAngle2D(float angle, float additionalAngle)
		{
			angle += additionalAngle;

			Vector3 direction = Vector3.zero;
			direction.x = Mathf.Cos(angle * Mathf.Deg2Rad);
			direction.y = Mathf.Sin(angle * Mathf.Deg2Rad);
			direction.z = 0f;
			return direction;
		}
	}
}