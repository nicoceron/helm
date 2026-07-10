using System.Collections.Generic;
using SVGImporter.Data;
using SVGImporter.Rendering;
using SVGImporter.Utils;
using UnityEngine;

namespace SVGImporter.Geometry
{
	public class SVGMesh
	{
		public static bool CombineMeshes(SVGLayer[] layers, Mesh mesh, out Shader[] shaders, SVGUseGradients useGradients = SVGUseGradients.Always, SVGAssetFormat format = SVGAssetFormat.Transparent, bool compressDepth = true, bool antialiased = false)
		{
			shaders = new Shader[0];
			bool flag = false;
			bool flag2 = false;
			bool flag3 = useGradients == SVGUseGradients.Always;
			if (layers == null)
			{
				return false;
			}
			int num = layers.Length;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			FILL_BLEND fILL_BLEND = FILL_BLEND.ALPHA_BLENDED;
			if (format == SVGAssetFormat.Opaque)
			{
				if (compressDepth)
				{
					SVGBounds infiniteInverse = SVGBounds.InfiniteInverse;
					for (int i = 0; i < num; i++)
					{
						int num5 = layers[i].shapes.Length;
						for (int j = 0; j < num5; j++)
						{
							SVGShape sVGShape = layers[i].shapes[j];
							if (sVGShape.bounds.size.sqrMagnitude != 0f)
							{
								infiniteInverse.Encapsulate(sVGShape.bounds.center, sVGShape.bounds.size);
							}
						}
					}
					infiniteInverse.size *= 1.2f;
					if (!infiniteInverse.isInfiniteInverse)
					{
						SVGDepthTree sVGDepthTree = new SVGDepthTree(infiniteInverse);
						for (int k = 0; k < num; k++)
						{
							int num6 = layers[k].shapes.Length;
							for (int l = 0; l < num6; l++)
							{
								SVGShape sVGShape = layers[k].shapes[l];
								int[] array = sVGDepthTree.TestDepthAdd(l, new SVGBounds(sVGShape.bounds.center, sVGShape.bounds.size));
								int num7 = 0;
								if (array == null || array.Length == 0)
								{
									sVGShape.depth = 0f;
								}
								else
								{
									num7 = array.Length;
									int num8 = 0;
									int num9 = -1;
									for (int m = 0; m < num7; m++)
									{
										if ((int)layers[k].shapes[array[m]].depth > num8)
										{
											num8 = (int)layers[k].shapes[array[m]].depth;
											num9 = array[m];
										}
									}
									if (layers[k].shapes[l].fill.blend == FILL_BLEND.OPAQUE)
									{
										sVGShape.depth = num8 + 1;
									}
									else if (num9 != -1 && layers[k].shapes[num9].fill.blend == FILL_BLEND.OPAQUE)
									{
										sVGShape.depth = num8 + 1;
									}
									else
									{
										sVGShape.depth = num8;
									}
								}
								layers[k].shapes[l] = sVGShape;
							}
						}
					}
				}
				else
				{
					int num10 = 0;
					for (int n = 0; n < num; n++)
					{
						int num11 = layers[n].shapes.Length;
						for (int num12 = 0; num12 < num11; num12++)
						{
							SVGShape sVGShape = layers[n].shapes[num12];
							SVGFill fill = sVGShape.fill;
							if (fill.blend == FILL_BLEND.OPAQUE || fILL_BLEND == FILL_BLEND.OPAQUE)
							{
								sVGShape.depth = ++num10;
							}
							else
							{
								sVGShape.depth = num10;
							}
							fILL_BLEND = fill.blend;
							layers[n].shapes[num12] = sVGShape;
						}
					}
				}
			}
			int num13 = 0;
			int num14 = 0;
			for (int num15 = 0; num15 < num; num15++)
			{
				int num16 = layers[num15].shapes.Length;
				for (int num17 = 0; num17 < num16; num17++)
				{
					SVGFill fill = layers[num15].shapes[num17].fill;
					if (fill.blend == FILL_BLEND.OPAQUE)
					{
						num3 += layers[num15].shapes[num17].triangles.Length;
						flag = true;
					}
					else if (fill.blend == FILL_BLEND.ALPHA_BLENDED)
					{
						num4 += layers[num15].shapes[num17].triangles.Length;
						flag2 = true;
					}
					if (fill.fillType == FILL_TYPE.GRADIENT)
					{
						flag3 = true;
					}
					int num18 = layers[num15].shapes[num17].vertices.Length;
					num13 += num18;
				}
			}
			num2 = num3 + num4;
			if (useGradients == SVGUseGradients.Never)
			{
				flag3 = false;
			}
			if (format != SVGAssetFormat.Opaque)
			{
				flag = false;
				flag2 = true;
			}
			Vector3[] array2 = new Vector3[num13];
			Color32[] array3 = new Color32[num13];
			Vector2[] array4 = null;
			Vector2[] array5 = null;
			Vector3[] array6 = null;
			int[][] array7 = null;
			List<Shader> list = new List<Shader>();
			if (antialiased)
			{
				array6 = new Vector3[num13];
			}
			if (flag3)
			{
				array4 = new Vector2[num13];
				array5 = new Vector2[num13];
				if (flag)
				{
					list.Add(SVGShader.GradientColorOpaque);
				}
				if (flag2)
				{
					if (antialiased)
					{
						list.Add(SVGShader.GradientColorAlphaBlendedAntialiased);
					}
					else
					{
						list.Add(SVGShader.GradientColorAlphaBlended);
					}
				}
			}
			else
			{
				if (flag)
				{
					list.Add(SVGShader.SolidColorOpaque);
				}
				if (flag2)
				{
					if (antialiased)
					{
						list.Add(SVGShader.SolidColorAlphaBlendedAntialiased);
					}
					else
					{
						list.Add(SVGShader.SolidColorAlphaBlended);
					}
				}
			}
			for (int num19 = 0; num19 < num; num19++)
			{
				int num20 = layers[num19].shapes.Length;
				for (int num21 = 0; num21 < num20; num21++)
				{
					int num18 = layers[num19].shapes[num21].vertices.Length;
					if (layers[num19].shapes[num21].colors != null && layers[num19].shapes[num21].colors.Length == num18)
					{
						Color32 finalColor = layers[num19].shapes[num21].fill.finalColor;
						for (int num22 = 0; num22 < num18; num22++)
						{
							int num23 = num14 + num22;
							array2[num23] = layers[num19].shapes[num21].vertices[num22];
							if (flag)
							{
								array2[num23].z = layers[num19].shapes[num21].depth * (0f - SVGAssetImport.minDepthOffset);
							}
							else
							{
								array2[num23].z = layers[num19].shapes[num21].depth;
							}
							array3[num23].r = (byte)(finalColor.r * layers[num19].shapes[num21].colors[num22].r / 255);
							array3[num23].g = (byte)(finalColor.g * layers[num19].shapes[num21].colors[num22].g / 255);
							array3[num23].b = (byte)(finalColor.b * layers[num19].shapes[num21].colors[num22].b / 255);
							array3[num23].a = (byte)(finalColor.a * layers[num19].shapes[num21].colors[num22].a / 255);
						}
					}
					else
					{
						Color32 finalColor2 = layers[num19].shapes[num21].fill.finalColor;
						for (int num24 = 0; num24 < num18; num24++)
						{
							int num23 = num14 + num24;
							array2[num23] = layers[num19].shapes[num21].vertices[num24];
							if (flag)
							{
								array2[num23].z = layers[num19].shapes[num21].depth * (0f - SVGAssetImport.minDepthOffset);
							}
							else
							{
								array2[num23].z = layers[num19].shapes[num21].depth;
							}
							array3[num23] = finalColor2;
						}
					}
					if (flag3)
					{
						if (layers[num19].shapes[num21].fill.fillType == FILL_TYPE.GRADIENT && layers[num19].shapes[num21].fill.gradientColors != null)
						{
							SVGMatrix transform = layers[num19].shapes[num21].fill.transform;
							Rect viewport = layers[num19].shapes[num21].fill.viewport;
							Vector2 point = Vector2.zero;
							Vector2 vector = new Vector2(layers[num19].shapes[num21].fill.gradientColors.index, (int)layers[num19].shapes[num21].fill.gradientType);
							if (layers[num19].shapes[num21].angles != null && layers[num19].shapes[num21].angles.Length == num18)
							{
								for (int num25 = 0; num25 < num18; num25++)
								{
									int num23 = num14 + num25;
									point.x = array2[num23].x;
									point.y = array2[num23].y;
									point = transform.Transform(point);
									array4[num23].x = (point.x - viewport.x) / viewport.width;
									array4[num23].y = (point.y - viewport.y) / viewport.height;
									array5[num23] = vector;
									array6[num23].x = layers[num19].shapes[num21].angles[num25].x;
									array6[num23].y = layers[num19].shapes[num21].angles[num25].y;
								}
							}
							else
							{
								for (int num26 = 0; num26 < num18; num26++)
								{
									int num23 = num14 + num26;
									point.x = array2[num23].x;
									point.y = array2[num23].y;
									point = transform.Transform(point);
									array4[num23].x = (point.x - viewport.x) / viewport.width;
									array4[num23].y = (point.y - viewport.y) / viewport.height;
									array5[num23] = vector;
								}
							}
						}
						else if (layers[num19].shapes[num21].fill.fillType == FILL_TYPE.TEXTURE)
						{
							SVGMatrix transform2 = layers[num19].shapes[num21].fill.transform;
							Vector2 zero = Vector2.zero;
							if (layers[num19].shapes[num21].angles != null && layers[num19].shapes[num21].angles.Length == num18)
							{
								for (int num27 = 0; num27 < num18; num27++)
								{
									int num23 = num14 + num27;
									zero.x = array2[num23].x;
									zero.y = array2[num23].y;
									array4[num23] = transform2.Transform(zero);
									array6[num23].x = layers[num19].shapes[num21].angles[num27].x;
									array6[num23].y = layers[num19].shapes[num21].angles[num27].y;
								}
							}
							else
							{
								for (int num28 = 0; num28 < num18; num28++)
								{
									int num23 = num14 + num28;
									zero.x = array2[num23].x;
									zero.y = array2[num23].y;
									array4[num23] = transform2.Transform(zero);
								}
							}
						}
						else if (layers[num19].shapes[num21].angles != null && layers[num19].shapes[num21].angles.Length == num18)
						{
							for (int num29 = 0; num29 < num18; num29++)
							{
								int num23 = num14 + num29;
								array6[num23].x = layers[num19].shapes[num21].angles[num29].x;
								array6[num23].y = layers[num19].shapes[num21].angles[num29].y;
							}
						}
					}
					else if (antialiased && layers[num19].shapes[num21].angles != null && layers[num19].shapes[num21].angles.Length == num18)
					{
						for (int num30 = 0; num30 < num18; num30++)
						{
							int num23 = num14 + num30;
							array6[num23] = layers[num19].shapes[num21].angles[num30];
						}
					}
					num14 += num18;
				}
			}
			if (flag && flag2)
			{
				array7 = new int[2][]
				{
					new int[num3],
					new int[num4]
				};
				int num31 = 0;
				int num32 = 0;
				int num33 = 0;
				for (int num34 = 0; num34 < num; num34++)
				{
					int num35 = layers[num34].shapes.Length;
					for (int num36 = 0; num36 < num35; num36++)
					{
						int num37 = layers[num34].shapes[num36].triangles.Length;
						if (layers[num34].shapes[num36].fill.blend == FILL_BLEND.OPAQUE)
						{
							for (int num38 = 0; num38 < num37; num38++)
							{
								array7[0][num32++] = num31 + layers[num34].shapes[num36].triangles[num38];
							}
						}
						else
						{
							for (int num39 = 0; num39 < num37; num39++)
							{
								array7[1][num33++] = num31 + layers[num34].shapes[num36].triangles[num39];
							}
						}
						num31 += layers[num34].shapes[num36].vertices.Length;
					}
				}
			}
			else
			{
				array7 = new int[1][] { new int[num2] };
				int num40 = 0;
				int num41 = 0;
				for (int num42 = 0; num42 < num; num42++)
				{
					int num43 = layers[num42].shapes.Length;
					for (int num44 = 0; num44 < num43; num44++)
					{
						int num45 = layers[num42].shapes[num44].triangles.Length;
						for (int num46 = 0; num46 < num45; num46++)
						{
							array7[0][num41++] = num40 + layers[num42].shapes[num44].triangles[num46];
						}
						num40 += layers[num42].shapes[num44].vertices.Length;
					}
				}
			}
			if (list.Count != 0)
			{
				shaders = list.ToArray();
			}
			mesh.Clear();
			mesh.MarkDynamic();
			if (array2 != null)
			{
				if (array2.Length > 65000)
				{
					Debug.LogError("A mesh may not have more than 65000 vertices. Please try to reduce quality or split SVG file.");
					return false;
				}
				mesh.vertices = array2;
				mesh.colors32 = array3;
				if (array4 != null)
				{
					mesh.uv = array4;
				}
				if (array5 != null)
				{
					mesh.uv2 = array5;
				}
				if (array6 != null)
				{
					mesh.normals = array6;
				}
				if (array7.Length == 1)
				{
					mesh.triangles = array7[0];
				}
				else
				{
					mesh.subMeshCount = array7.Length;
					for (int num47 = 0; num47 < array7.Length; num47++)
					{
						mesh.SetTriangles(array7[num47], num47);
					}
				}
				return true;
			}
			return false;
		}
	}
}
