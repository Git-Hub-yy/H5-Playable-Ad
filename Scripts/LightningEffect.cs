using System.Collections;
using UnityEngine;

public class LightningEffect : MonoBehaviour
{
    [Header("闪电数量")]
    public int lightningCount = 6;

    [Header("持续时间")]
    public float duration = 0.45f;

    [Header("闪电宽度")]
    public float startWidth = 0.14f;

    public float endWidth = 0.04f;

    [Header("闪电抖动")]
    public float maxOffset = 0.45f;

    [Header("闪电颜色")]
    public Color lightningColor =
        new Color(0.4f, 0.9f, 1f, 1f);


    public void Play(Vector3 target)
    {
        StartCoroutine(CreateLightning(target));
    }


    private IEnumerator CreateLightning(Vector3 target)
    {
        LineRenderer[] lines =
            new LineRenderer[lightningCount];


        for (int i = 0; i < lightningCount; i++)
        {
            GameObject obj =
                new GameObject("Lightning");

            obj.transform.SetParent(transform);

            LineRenderer line =
                obj.AddComponent<LineRenderer>();


            // 增加节点，让闪电有明显折线
            line.positionCount = 9;

            line.startWidth =
                startWidth;

            line.endWidth =
                endWidth;


            line.useWorldSpace = true;

            line.numCapVertices = 4;

            line.numCornerVertices = 4;


            // 使用透明材质
            Material material =
                new Material(
                    Shader.Find("Sprites/Default")
                );

            material.color =
                lightningColor;

            line.material = material;


            // 提高渲染顺序
            line.sortingOrder = 100;


            lines[i] = line;


            CreateLightningLine(
                line,
                transform.position,
                target
            );
        }


        // 闪电持续
        yield return
            new WaitForSeconds(duration);


        for (int i = 0;
             i < lines.Length;
             i++)
        {
            if (lines[i] != null)
            {
                Destroy(
                    lines[i].gameObject
                );
            }
        }
    }


    private void CreateLightningLine(
        LineRenderer line,
        Vector3 start,
        Vector3 end)
    {
        for (int i = 0; i < 9; i++)
        {
            float t =
                i / 8f;

            Vector3 position =
                Vector3.Lerp(
                    start,
                    end,
                    t
                );


            // 首尾不要乱动
            if (i != 0 && i != 8)
            {
                position +=
                    new Vector3(
                        Random.Range(
                            -maxOffset,
                            maxOffset
                        ),

                        Random.Range(
                            -maxOffset,
                            maxOffset
                        ),

                        Random.Range(
                            -maxOffset,
                            maxOffset
                        )
                    );
            }


            line.SetPosition(
                i,
                position
            );
        }
    }
}