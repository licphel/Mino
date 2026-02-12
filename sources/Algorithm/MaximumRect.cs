using System.Runtime.InteropServices;
using Mino.Mathematics;

namespace Mino.Algorithm;

/// <summary>
///     Implement the famous "Maximum rect" UV-packing algorithm.
/// </summary>
public static class MaximumRect {
	public static bool Find(List<RectI> freeRects, int width, int height, out RectI result, int padding = 1) {
        int best = -1;
        int bestScore = int.MaxValue;
        
        for (int i = 0; i < freeRects.Count; i++) {
            ref RectI fr = ref CollectionsMarshal.AsSpan(freeRects)[i];
            if (fr.Width < width + padding || fr.Height < height + padding) {
                continue;
            }
 
            int score = Math.Min(
                fr.Width - (width + padding), 
                fr.Height - (height + padding)
            );
            
            if (score < bestScore) {
                bestScore = score;
                best = i;
            }
        }
        
        if (best == -1) {
            result = new RectI();
            return false;
        }

        RectI used = freeRects[best];
        int dx = used.X;
        int dy = used.Y;

        int remainW = used.Width - (width + padding);
        int remainH = used.Height - (height + padding);

        RectI right1 = new RectI(used.X + width + padding, used.Y, remainW, height + padding);
        RectI top1 = new RectI(used.X, used.Y + height + padding, used.Width, remainH);
        RectI top2 = new RectI(used.X, used.Y + height + padding, width + padding, remainH);
        RectI right2 = new RectI(used.X + width + padding, used.Y, remainW, used.Height);
        
        freeRects.RemoveAt(best);
        
        if (remainW > 0 && remainH > 0) {
            int waste1 = Math.Abs(right1.Width * right1.Height - top1.Width * top1.Height);
            int waste2 = Math.Abs(right2.Width * right2.Height - top2.Width * top2.Height);
            
            if (waste1 <= waste2) {
                append(freeRects, right1);
                append(freeRects, top1);
            } else {
                append(freeRects, right2);
                append(freeRects, top2);
            }
        } else if (remainW > 0) {
            append(freeRects, new RectI(used.X + width + padding, used.Y, remainW, height + padding));
        } else if (remainH > 0) {
            append(freeRects, new RectI(used.X, used.Y + height + padding, width + padding, remainH));
        }
        
        merge(freeRects);
        
        result = new RectI(dx, dy, width, height);
        return true;
    }
    
    private static void append(List<RectI> rects, RectI rect) {
        if (rect.Width > 0 && rect.Height > 0) {
            rects.Add(rect);
        }
    }
    
    private static void merge(List<RectI> rects) {
        bool merged;
        do {
            merged = false;
            Span<RectI> span = CollectionsMarshal.AsSpan(rects);
            
            for (int i = 0; i < rects.Count; i++) {
                if (span[i].Width == 0 || span[i].Height == 0) {
                    continue;
                }

                for (int j = i + 1; j < rects.Count; j++) {
                    if (span[j].Width == 0 || span[j].Height == 0) {
                        continue;
                    }
                    
                    if (span[i].X == span[j].X && span[i].Width == span[j].Width) {
                        if (span[i].Y + span[i].Height == span[j].Y) {
                            span[i].Height += span[j].Height;
                            rects.RemoveAt(j);
                            merged = true;
                            break;
                        }
                        if (span[j].Y + span[j].Height == span[i].Y) {
                            span[i].Y = span[j].Y;
                            span[i].Height += span[j].Height;
                            rects.RemoveAt(j);
                            merged = true;
                            break;
                        }
                    }
                    
                    if (span[i].Y == span[j].Y && span[i].Height == span[j].Height) {
                        if (span[i].X + span[i].Width == span[j].X) {
                            span[i].Width += span[j].Width;
                            rects.RemoveAt(j);
                            merged = true;
                            break;
                        }
                        if (span[j].X + span[j].Width == span[i].X) {
                            span[i].X = span[j].X;
                            span[i].Width += span[j].Width;
                            rects.RemoveAt(j);
                            merged = true;
                            break;
                        }
                    }
                }
                if (merged) {
                    break;
                }
            }
        } while (merged);
    }

	public struct RectI {
		public int X;
		public int Y;
		public int Width;
		public int Height;

		public RectI(int x, int y, int width, int height) {
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

        public static implicit operator Box2(in RectI rect) {
            return Box2.Create(rect.X, rect.Y, rect.Width, rect.Height);
        }
	}
}
