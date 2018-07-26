using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIAEKtool
{
    public class IntSet
    {
        public class Range
        {
            public int low; // Inclusive
            public int high; // Exclusive

            public override bool Equals(object obj)
            {
                if (!(obj is Range)) return false; 
                    var r = (Range)obj;
                return low == r.low && high == r.high;
            }

            public override int GetHashCode()
            {
                return low.GetHashCode() ^ high.GetHashCode();
            }

        }

        List<Range> ranges = new List<Range>();
        public IntSet()
        {
        }

        class RangeCompare : IComparer<Range>
        {
            public int Compare(Range x, Range y)
            {
                return Comparer<int>.Default.Compare(x.low, y.low);
            }
        }

        
        /// <summary>
        /// Remove all following ranges covered by the first range, extending it if necessary
        /// </summary>
        /// <param name="first"></param>
        void CollapseRanges(int first)
        {
            int end = ranges[first].high;
            int i = first + 1;
            while (i < ranges.Count && ranges[i].low <= end) i++;
            
            if (ranges[i-1].high > end) ranges[first].high = ranges[i-1].high;
           
            if (first + 1 < i)
            {
                ranges.RemoveRange(first + 1, i - first - 1);
            }

        }

        public override bool Equals(object obj)
        {
            if (!(obj is IntSet)) return false;
            var r = ((IntSet)obj).ranges;
            int l = ranges.Count;
            if (l != r.Count) return false;
            for (int i = 0; i < l; i++)
            {
                if (!ranges[i].Equals(r[i])) return false;
            }
            return true;

        }

        public override int GetHashCode()
        {
            return ranges.GetHashCode();
        }

        RangeCompare range_comp = new RangeCompare();
        /// <summary>
        /// Adds a set range of integers to the set
        /// </summary>
        /// <param name="r"></param>
        /// <returns>True if all the integers in the set did not already exist in the set.</returns>
        public bool Add(Range r)
        {
            int i = ranges.BinarySearch(r, range_comp);
            if (i >= 0)
            {
                Range ri = ranges[i];
                ri.high = Math.Max(ri.high, r.high);
                CollapseRanges(i);
                return false;
            }
            else
            {
                i = ~i;

                if (i > 0)
                {
                    if (ranges[i - 1].high >= r.low)
                    {
                        // Merge with preceding range
                        bool overlap = ranges[i - 1].high > r.low || (i < ranges.Count && ranges[i].low < r.high);
                        ranges[i - 1].high = Math.Max(ranges[i - 1].high, r.high); ;
                        CollapseRanges(i - 1);
                        return !overlap;
                    }
                }
                if (i < ranges.Count)
                {
                    // Merge with following range
                    if (ranges[i].low <= r.high)
                    {
                        bool overlap = ranges[i].low < r.high;
                        ranges[i].low = r.low;
                        ranges[i].high = Math.Max(ranges[i].high, r.high);
                        CollapseRanges(i);
                        return !overlap;
                    }
                }
                // New range
                ranges.Insert(i, new Range() { low = r.low, high = r.high });
                return true;
            }
        }

        public bool Add(int low, int high)
        {
            return Add(new Range() { low = low, high = high });
        }

        public bool Add(int v)
        {
            return Add(v, v + 1);
        }

        public int LowestFree(int start = 1)
        {
            if (ranges.Count == 0) return start;
            if (ranges[0].low > start) return start;
            int r;
            for (r = 1; r < ranges.Count; r++)
            {
                if (ranges[r].low > start)
                {
                    if (ranges[r - 1].high <= start) return start;
                    return ranges[r - 1].high;
                }
            }
            if (ranges[r - 1].high <= start) return start;
            return ranges[r - 1].high;
        }
    }
}
