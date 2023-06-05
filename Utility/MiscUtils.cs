/*urada 2023/5/29*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// 一些迭代器拓展方法可能和system.linq中的重复
public static class MiscUtils
{
	public static bool InLayer(this GameObject go, string layer)
		=> InLayer(go, LayerMask.GetMask(layer));
	public static bool InLayer(this GameObject go, LayerMask layermask)
		=> layermask == (layermask | (1 << go.layer));

	public static void RequiredComponent<ComT>(Component obj, out ComT com)
		where ComT : Component
	{
		if (!obj.TryGetComponent(out com))
			com = obj.gameObject.AddComponent<ComT>();
	}
	public static ComT RequiredComponent<ComT>(Component obj)
		where ComT : Component
	{
		if (!obj.TryGetComponent(out ComT com))
			com = obj.gameObject.AddComponent<ComT>();
		return com;
	}

	public static Color InvertColor(Color c)
	{
		return new Color(1.0f - c.r, 1.0f - c.g, 1.0f - c.b);
	}

	public static bool RandomBool()
	{
		return Random.value > 0.5f;
	}

	public static int AddDistinct<T>(ref List<T> dst, IEnumerable<T> src)
	{
		int count = dst.Count;
		foreach (var v in src)
		{
			if (dst.IndexOf(v) < 0)
				dst.Add(v);
		}
		return dst.Count - count;
	}

	public static int ConverAndAppendList<T1, T2>(ref List<T1> dst, IEnumerable<T2> src, bool allowRepeat = false)
	{
		int count = dst.Count;
		foreach (var v in src)
		{
			if (v is T1 tv)
			{
				if (allowRepeat == false || dst.IndexOf(tv) < 0)
					dst.Add(tv);
			}
		}
		return dst.Count - count;
	}

	public static IEnumerable<T1> TryCast<T1, T2>(this IEnumerable<T2> src, T1 def = default)
	{
		foreach (var i in src)
		{
			if (i is T1 tv)
				yield return tv;
			else
				yield return def;
		}
	}

	public static IEnumerable<Vector2> Cast(this IEnumerable<Vector3> src)
	{
		foreach (var i in src)
			yield return new Vector2(i.x, i.y);
	}
	public static IEnumerable<Vector3> Cast(this IEnumerable<Vector2> src, float z = 0)
	{
		foreach (var i in src)
			yield return new Vector3(i.x, i.y, z);
	}

	public static IEnumerable<T1> Export<T1, T2>(this IEnumerable<T2> src, System.Func<T2, T1> export)
	{
		foreach (var i in src)
			yield return export.Invoke(i);
	}

	public static string Connect(string space, params string[] args)
	{
		if (args.Length == 0) return "";
		if (args.Length == 1) return args[0];

		string result = "";
		for (int i = 0; i < args.Length - 1; i++)
		{
			if (!string.IsNullOrEmpty(args[i]))
				result += args[i] + space;
		}
		result += args[args.Length - 1];

		return result;
	}

	public static Texture2D CreateTexture(int w, int h, Color? color, bool temporary = true)
	{
		color = color ?? Color.clear;

		var tex = new Texture2D(w, h);

		var colors = new Color[w * h];
		for (int i = 0; i < colors.Length; i++)
			colors[i] = color.Value;

		if (temporary) tex.hideFlags = HideFlags.HideAndDontSave;

		tex.SetPixels(colors);
		tex.Apply();

		return tex;
	}

	public static Texture2D CreateColorTexture(Color color)
		=> CreateTexture(4, 4, color, true); // 将纹理大小控制为2的幂, 虽然不是必要的

	public static void GenCircleLine(LineRenderer line, float radius, Vector3? centerOffset = null, int sample = 32)
	{
		var offset = centerOffset ?? Vector3.zero;

		line.positionCount = sample;
		var points = new Vector3[line.positionCount];

		for (int i = 0; i < sample; i++)
		{
			var r = ((float)i / sample - 1) * Mathf.PI * 2.0f;
			var point = offset + (Vector3)MathUtility.Circle(radius, r);

			points[i] = point;
		}
		line.SetPositions(points);
	}

	// 待修复 rect.position指示左上角而不是中间
	public static Rect GetCameraViewport(Camera camera)
	{
		var y = camera.orthographicSize * 2;
		var x = y * camera.aspect;

		var pos = (Vector2)camera.transform.position;

		return new Rect(position: pos, size: new Vector2(x, y));
	}

	public static Vector2 MousePosition()
		=> Camera.main.ScreenToWorldPoint(Input.mousePosition);

	// 在保持与目标距离的同时以forward一面朝向目标
	public static Vector3 Alignment(Vector3 self, Vector3 target, Vector3 forward)
	{
		var dist = (self - target).magnitude;
		var dir = forward;

		return target - dir * dist;
	}

	public static bool BitMaskAnd(int mask, int v)
	{
		return (v & mask) == v;
	}
	public static bool BitMaskOr(int mask, int v)
	{
		return (v & mask) != 0;
	}

	public static IEnumerable<T2> Process<T1, T2>(IEnumerable<T1> src, System.Func<T1, T2> handle)
	{
		foreach (var i in src)
			yield return handle.Invoke(i);
	}

	public static IEnumerable<Vector3> ExportPosition(IEnumerable<Transform> src, bool local = false)
	{
		foreach (var tr in src)
			yield return local ? tr.localPosition : tr.position;
	}
	public static IEnumerable<Vector3> ExportPositionFromRoot(Transform root)
	{
		foreach (Transform tr in root)
			yield return tr.position;
	}

	public static string FormatContainer<T>(IEnumerable<T> container, System.Func<T, string> toString = null)
	{
		if (toString == null) toString = (T t) => t.ToString();

		bool first = false;

		string str = "{";
		foreach (T child in container)
		{
			if (first)
			{
				str += toString.Invoke(child);
				first = false;
			}
			else
			{
				str += ", " + toString.Invoke(child);
			}
		}
		str += "}";
		return str;
	}

	public static IEnumerable<float> SampleValue(int sample, float v)
	{
		sample = Mathf.Max(sample, 1);
		for (int i = 0; i < sample; i++)
		{
			float r = i / (sample - 1);
			yield return v * r;
		}
	}

	// 过渡到正交视图
	public static IEnumerator CameraToOrthViewProcess(Camera camera, float duration, float focusDist, float fovTarget = 1)
	{
		// 记录原始的fov值
		var fovStart = camera.fieldOfView;

		// 计算固定的视角大小
		/* 视口锥台的高度与底边宽度
			Camera
				/+\ angle=fov
				-+- near
			   / + \
			  /  +  \
			 /   +   \
			---Target== far
			/    |    \
		整个锥体的角度为fov
		由+标注的即为dist, 由=标注的即为size
		在计算过程中将其视为一个直角三角形, 由三角函数计算另外2对参数的值
		*/
		var halfFovAngle = fovStart * 0.5f * Mathf.Deg2Rad;
		var size = Mathf.Tan(halfFovAngle) * focusDist;

		var camTr = camera.transform;
		var focusPoint = camTr.position + camTr.forward * focusDist;

		for (float t = 0; t < duration; t += Time.deltaTime)
		{
			float r = Mathf.Clamp01(t / duration);
			camera.fieldOfView = Mathf.Lerp(fovStart, fovTarget, r);

			// 根据fov来更新目标到摄像机的距离
			// 随着fov越来越小, dist将越来越大来保持size不变
			var dist = MathUtility.Cot(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * size;

			// 更新摄像机的位置
			var pos = focusPoint - camTr.forward * dist;
			camTr.position = pos;

			yield return null;
		}

		// 在过渡动画完成后将视角切换为正交
		camera.orthographic = true;
		camera.orthographicSize = size;
	}

	// 过渡到透视视图
	public static IEnumerator CameraToPersViewProcess(Camera camera, float duration, float focusDist, float fovTarget = 60)
	{
		// 记录原始的fov值
		var fovStart = camera.fieldOfView;

		// 计算固定的视角大小
		/* 视口锥台的高度与底边宽度
			Camera
				/+\ angle=fov
				-+- near
			   / + \
			  /  +  \
			 /   +   \
			---Target== far
			/    |    \
		整个锥体的角度为fov
		由+标注的即为dist, 由=标注的即为size
		在计算过程中将其视为一个直角三角形, 由三角函数计算另外2对参数的值
		*/
		var halfFovAngle = fovStart * 0.5f * Mathf.Deg2Rad;
		var size = camera.orthographicSize;

		Debug.Assert(Mathf.Abs(halfFovAngle - 1.0f) < 10);

		var camTr = camera.transform;
		var focusPoint = camTr.position + camTr.forward * focusDist;

		// 初始化, 将相机移动到足够远的距离后设置为正交视角, 再逐渐拉近到正常位置
		var startDist = MathUtility.Cot(halfFovAngle) * size;

		var startPos = focusPoint - camTr.forward * startDist;
		camTr.position = startPos;

		// 设置回正交视角
		camera.orthographic = false;

		// 逐渐拉近
		for (float t = 0; t < duration; t += Time.deltaTime)
		{
			float r = Mathf.Clamp01(t / duration);
			camera.fieldOfView = Mathf.Lerp(fovStart, fovTarget, r);

			// 根据fov来更新目标到摄像机的距离
			// 随着fov越来越大, dist将越来越小来保持size不变
			var dist = MathUtility.Cot(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * size;

			// 更新摄像机的位置
			var pos = focusPoint - camTr.forward * dist;
			camTr.position = pos;

			yield return null;
		}
	}

#if UNITY_EDITOR
	// 按顺序获取被选中的GameObject
	public static IEnumerable<GameObject> GetSelectedGameObjectsByOrder(bool inScene = true, System.Func<GameObject, bool> filter = null)
	{
		var selected = Selection.objects;
		foreach (var obj in selected)
		{
			if (obj is GameObject go)
			{
				// 检查是否是场景中的对象且通过过滤器
				if (inScene && string.IsNullOrEmpty(go.scene.name))
					continue;
				if (filter != null && filter.Invoke(go))
					continue;

				yield return go;
			}
		}
	}

	// 按顺序遍历被选中对象且尝试获取目标组件并返回
	public static IEnumerable<T> GetSelectedComponentsByOrder<T>()
		where T : Component
	{
		var selected = Selection.objects;
		foreach (var obj in selected)
		{
			if (obj is GameObject go && go.TryGetComponent(out T com))
				yield return com;
		}
	}

	// 按顺序获取被选中的任意对象, 必须继承自UnityEngine.Object
	// 使用过滤器进行过滤
	public static IEnumerable<T> GetSelectedByOrder<T>(System.Func<T, bool> filter = null)
		where T : UnityEngine.Object
	{
		var selected = Selection.objects;
		foreach (var obj in selected)
		{
			if (obj is T to && (filter == null || filter.Invoke(to)))
				yield return to;
		}
	}

	// 获取场景中被选中的GameObject
	public static IEnumerable<GameObject> GetSelectedGameObjectInScene()
	{
		var selected = Selection.objects;
		foreach (var obj in selected)
		{
			// 只处理场景中的对象
			if (obj is GameObject go && !string.IsNullOrEmpty(go.scene.name))
				yield return go;
		}
	}

	public static float GizmoScale(Vector3 position, Camera camera = null)
	{
		camera = camera ?? Camera.current;
		position = Gizmos.matrix.MultiplyPoint(position);

		if (camera)
		{
			Transform transform = camera.transform;
			Vector3 position2 = transform.position;
			float z = Vector3.Dot(position - position2, transform.TransformDirection(new Vector3(0f, 0f, 1f)));
			Vector3 a = camera.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(0f, 0f, z)));
			Vector3 b = camera.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(1f, 0f, z)));
			float magnitude = (a - b).magnitude;
			return 80f / Mathf.Max(magnitude, 0.0001f);
		}

		return 20f;
	}

	// 编辑器快速操作
	// 不可用, 待修复
	// 对当前选中的在编辑器中的GameObject的子对象进行反向排序
	// [MenuItem("MiscUtils/Reverse Selected GameObjects Child")]
	private static void ReverseSelectedChild()
	{
		var itor = MiscUtils.GetSelectedComponentsByOrder<Transform>();
		var result = new List<Transform>(itor);

		// 储存位置方便撤销
		List<Transform> childs = new List<Transform>();
		foreach (var tr in result)
		{
			foreach (Transform child in tr)
				childs.Add(child);
		}
		Undo.RecordObjects(childs.ToArray(), "Spacing Spacing YAxis");

		foreach (var tr in result)
		{
			for (int i = 0; i < tr.childCount; i++)
				tr.GetChild(0).SetAsLastSibling();
		}
	}

	// 编辑器快速操作
	// 首先在选中的所有对象中计算ymin和ymax, 再进行均匀分布
	[MenuItem("MiscUtils/Spacing YAxis")]
	public static void SpacingYAxis()
	{
		var itor = MiscUtils.GetSelectedComponentsByOrder<Transform>();
		var result = new List<Transform>(itor);

		if (result.Count <= 1) return;

		// 储存位置方便撤销
		Undo.RecordObjects(result.ToArray(), "Spacing Spacing YAxis");

		float yMax = float.MinValue;
		float yMin = float.MaxValue;

		foreach (var i in result)
		{
			var pos = i.transform.localPosition;
			yMax = Mathf.Max(pos.y, yMax);
			yMin = Mathf.Min(pos.y, yMin);
		}

		var yDelta = (yMax - yMin) / (result.Count - 1);

		for (int i = 0; i < result.Count; i++)
		{
			var pos = result[i].transform.localPosition;
			pos.y = yDelta * i + yMin;

			result[i].transform.localPosition = pos;
		}
	}
#endif
}

//public static int GetSelected<T>(ref List<T> results)
//	where T : UnityEngine.Object
//{
//	int count = results.Count;
//	foreach (var obj in UnityEditor.Selection.objects)
//	{
//		if (obj is T go)
//			results.Add(go);
//	}
//	return results.Count - count;
//}
//public static T GetSelected<T>()
//	where T : UnityEngine.Object
//{
//	List<T> results = new List<T>();
//	int count = GetSelected(ref results);

//	if (count == 0) return null;
//	else return results[results.Count - 1];
//}
