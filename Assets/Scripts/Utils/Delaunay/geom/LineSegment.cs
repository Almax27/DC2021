using UnityEngine;
using System;

namespace Delaunay
{
	namespace Geo
	{
		public sealed class LineSegment
		{
			public static int CompareLengths_MAX (LineSegment segment0, LineSegment segment1)
			{
				float length0 = Vector2.Distance ((Vector2)segment0.p0, (Vector2)segment0.p1);
				float length1 = Vector2.Distance ((Vector2)segment1.p0, (Vector2)segment1.p1);
				if (length0 < length1) {
					return 1;
				}
				if (length0 > length1) {
					return -1;
				}
				return 0;
			}
		
			public static int CompareLengths (LineSegment edge0, LineSegment edge1)
			{
				return - CompareLengths_MAX (edge0, edge1);
			}

            public static bool Intersect(LineSegment seg0, LineSegment seg1, ref Vector2 intersection)
            {
                Vector2 a = (Vector2)seg0.p1 - (Vector2)seg0.p0;
                Vector2 b = (Vector2)seg1.p0 - (Vector2)seg1.p1;
                Vector2 c = (Vector2)seg0.p0 - (Vector2)seg1.p0;

                float alphaNumerator = b.y * c.x - b.x * c.y;
                float betaNumerator = a.x * c.y - a.y * c.x;
                float denominator = a.y * b.x - a.x * b.y;

                if (denominator == 0)
                {
                    return false;
                }
                else if (denominator > 0)
                {
                    if (alphaNumerator < 0 || alphaNumerator > denominator || betaNumerator < 0 || betaNumerator > denominator)
                    {
                        return false;
                    }
                }
                else if (alphaNumerator > 0 || alphaNumerator < denominator || betaNumerator > 0 || betaNumerator < denominator)
                {
                    return false;
                }

                // compute intersection coordinates //
                {
                    float num = alphaNumerator * a.x; // numerator //
                    intersection.x = ((Vector2)seg0.p0).x + num / denominator;
                }
                {
                    float num = alphaNumerator * a.y;
                    intersection.y = ((Vector2)seg0.p0).y + num / denominator;
                }

                return true;
            }

			public Nullable<Vector2> p0;
			public Nullable<Vector2> p1;
		
			public LineSegment (Nullable<Vector2> p0, Nullable<Vector2> p1)
			{
				this.p0 = p0;
				this.p1 = p1;
			}
		
		}
	}
}