using UnityEngine;
using UnityEngine.UI;

public class RadialLayoutGroup : LayoutGroup
{
    // 设置参数
    public float radius = 100f; // 元素到中心点的半径
    public float angleRange = 360f;    // 角度范围
    public float angleOffset = 0f;     // 旋转角度的偏移量
    public float spacingAngle = 30f;   // 元素之间的角度间距

    // 选项：是否参考 LayoutElement 中的数据
    public bool usePreferredWidth = true; // 是否参考 preferredWidth
    public bool usePreferredHeight = true; // 是否参考 preferredHeight
    public bool useFlexibleWidth = false; // 是否参考 flexibleWidth
    public bool useFlexibleHeight = false; // 是否参考 flexibleHeight

    private Vector2[] cachedPositions; // 缓存位置
    private Quaternion[] cachedRotations; // 缓存旋转

    // 更新布局
    public override void CalculateLayoutInputHorizontal() { }
    public override void CalculateLayoutInputVertical() { }

    // 设置布局
    public override void SetLayoutHorizontal() { SetRadialLayout(); }
    public override void SetLayoutVertical() { SetRadialLayout(); }

    private void SetRadialLayout()
    {
        int childCount = rectTransform.childCount;
        if (childCount == 0) return;

        // 初始化缓存的位置信息
        cachedPositions = new Vector2[childCount];
        cachedRotations = new Quaternion[childCount];

        // 计算每个元素的位置和旋转
        for (int i = 0; i < childCount; i++)
        {
            RectTransform childRect = rectTransform.GetChild(i) as RectTransform;
            if (childRect == null) continue;

            // 获取 LayoutElement 数据
            LayoutElement layoutElement = childRect.GetComponent<LayoutElement>();
            float preferredWidth = layoutElement ? layoutElement.preferredWidth : 0f;
            float preferredHeight = layoutElement ? layoutElement.preferredHeight : 0f;
            float flexibleWidth = layoutElement ? layoutElement.flexibleWidth : 0f;
            float flexibleHeight = layoutElement ? layoutElement.flexibleHeight : 0f;

            // 计算当前子物体的角度
            float angle = angleOffset + i * spacingAngle;
            float angleInRadians = Mathf.Deg2Rad * angle;

            // 计算元素在圆上应该的位置
            cachedPositions[i] = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians)) * radius;

            // 设置旋转，使得子物体朝向圆心
            cachedRotations[i] = Quaternion.Euler(0f, 0f, angle); // 旋转90度是为了让子物体朝向圆心

            // 根据 LayoutElement 设置偏移
            if (usePreferredWidth && preferredWidth > 0)
            {
                float extraRadius = preferredWidth * 0.5f;
                cachedPositions[i] += cachedPositions[i].normalized * extraRadius;
            }

            if (usePreferredHeight && preferredHeight > 0)
            {
                float extraRadius = preferredHeight * 0.5f;
                cachedPositions[i] += cachedPositions[i].normalized * extraRadius;
            }

            if (useFlexibleWidth && flexibleWidth > 0)
            {
                // 如果 flexibleWidth 有效，考虑它来动态调整半径
                float extraRadius = flexibleWidth * 10f; // 适当的调整因子
                cachedPositions[i] += cachedPositions[i].normalized * extraRadius;
            }

            if (useFlexibleHeight && flexibleHeight > 0)
            {
                // 如果 flexibleHeight 有效，考虑它来动态调整半径
                float extraRadius = flexibleHeight * 10f; // 适当的调整因子
                cachedPositions[i] += cachedPositions[i].normalized * extraRadius;
            }
        }

        // 应用缓存的位置信息
        for (int i = 0; i < childCount; i++)
        {
            RectTransform childRect = rectTransform.GetChild(i) as RectTransform;
            if (childRect != null)
            {
                // 只有在位置或旋转发生变化时才进行修改
                if (childRect.anchoredPosition != cachedPositions[i])
                {
                    childRect.anchoredPosition = cachedPositions[i];
                }

                if (childRect.rotation != cachedRotations[i])
                {
                    childRect.rotation = cachedRotations[i];
                }
            }
        }
    }
}
