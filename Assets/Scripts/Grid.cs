﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class Grid : MonoBehaviour
{
    public InterpolationType type;

    public List<Pivot> pivot;
    public ColorGradient colorGradient;

    private int width = 300;
    private int height = 300;

    private float size = 60;

    private void Update()
    {
        Draw();
    }

    private void Draw()
    {
        var texture = new Texture2D(width, height);
        switch (type)
        {
            case InterpolationType.Nearest:
                Nearest(texture);
                break;

            case InterpolationType.Bilinear:
                Bilinear(texture);
                break;

            case InterpolationType.Bicubic:
                Bicubic(texture);
                break;
        }
        texture.Apply();
        GetComponent<RawImage>().texture = texture;
    }

    private void Nearest(Texture2D texture)
    {
        for (int i = 0; i < width; i++)
        {
            for (int k = 0; k < height; k++)
            {
                texture.SetPixel(i, k, colorGradient.GetColor(pivot[Mathf.FloorToInt(k / size)][Mathf.FloorToInt(i / size)]));
            }
        }
    }

    private void Bilinear(Texture2D texture)
    {
        for (int i = 0; i < width; i++)
        {
            for (int k = 0; k < height; k++)
            {
                var dx = Mathf.Repeat(i / size, 1);
                var dy = Mathf.Repeat(k / size, 1);

                var x = Mathf.FloorToInt(i / size);
                var y = Mathf.FloorToInt(k / size);

                var rx = Mathf.RoundToInt(i / size);
                var ry = Mathf.RoundToInt(k / size);

                #region Edge Draw
                if(rx == 0 || rx == 5)
                {
                    if(ry == 0 || ry == 5)
                    {
                        texture.SetPixel(i, k, colorGradient.GetColor(pivot[y][x]));
                    }
                    else
                    {
                        var l = Mathf.Lerp(pivot[ry - 1][x], pivot[ry][x], Mathf.InverseLerp(30 + 60 * (ry - 1), 30 + 60 * (ry), k));
                        texture.SetPixel(i, k, colorGradient.GetColor(l));
                    }
                    continue;
                }

                if(ry == 0 || ry == 5)
                {
                    if (rx != 0 && rx != 5)
                    {
                        var l = Mathf.Lerp(pivot[y][rx - 1], pivot[y][rx], Mathf.InverseLerp(30 + 60 * (rx - 1), 30 + 60 * (rx), i));
                        texture.SetPixel(i, k, colorGradient.GetColor(l));
                    }
                    continue;
                }
                #endregion
                
                var l0 = Mathf.Lerp(pivot[ry - 1][rx - 1], pivot[ry - 1][rx], Mathf.InverseLerp(30 + 60 * (rx - 1), 30 + 60 * (rx), i));
                var l1 = Mathf.Lerp(pivot[ry][rx - 1], pivot[ry][rx], Mathf.InverseLerp(30 + 60 * (rx - 1), 30 + 60 * (rx), i));
                var l2 = Mathf.Lerp(l0, l1, Mathf.InverseLerp(30 + 60 * (ry - 1), 30 + 60 * (ry), k));
                texture.SetPixel(i, k, colorGradient.GetColor(l2));
            }
        }
    }

    private void Bicubic(Texture2D texture)
    {
        for (int i = 0; i < width; i++)
        {
            for (int k = 0; k < height; k++)
            {
                var dx = Mathf.Repeat(i / size, 1);
                var dy = Mathf.Repeat(k / size, 1);

                var x = Mathf.FloorToInt(i / size);
                var y = Mathf.FloorToInt(k / size);


                float l0, l1, l2, l3;
                l0 = 0;
                l1 = 0;
                l2 = 0;
                l3 = 0;


                try
                {
                    if(x == 0)
                    {
                        l0 = CubicInterpolation(pivot[y][x], pivot[y + 1][x], pivot[y + 2][x], pivot[y + 3][x], dy);
                        l1 = l0;
                    }
                    l0 = CubicInterpolation(pivot[y][x], pivot[y + 1][x], pivot[y + 2][x], pivot[y + 3][x], dy);
                    l1 = CubicInterpolation(pivot[y][x + 1], pivot[y + 1][x + 1], pivot[y + 2][x + 1], pivot[y + 3][x + 1], dy);
                    l2 = CubicInterpolation(pivot[y][x + 2], pivot[y + 1][x + 2], pivot[y + 2][x + 2], pivot[y + 3][x + 2], dy);
                    l3 = CubicInterpolation(pivot[y][x + 3], pivot[y + 1][x + 3], pivot[y + 2][x + 3], pivot[y + 3][x + 3], dy);
                }
                catch
                {

                }
                    var t = CubicInterpolation(l0, l1, l2, l3, dx);
                    texture.SetPixel(i, k, colorGradient.GetColor(t));
            }
        }
    }

    private float CubicInterpolation(float p0, float p1, float p2, float p3, float t)
    {
        float a0 = p3 - p2 - p0 + p1;
        float a1 = p0 - p1 - a0;
        float a2 = p2 - p0;
        float a3 = p1;
        return a0 * t * t * t + a1 * t * t + a2 * t + a3;
    }
}