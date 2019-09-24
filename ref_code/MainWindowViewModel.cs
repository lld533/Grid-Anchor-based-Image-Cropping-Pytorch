namespace ImageCrop2
{
    public class MainWindowViewModel : ViewModelBase
    {
		private static readonly int BINS = 12;

        private static readonly float UPPER_RATIO = 2.2f;

        private static readonly float LOWER_RATIO = 0.5f;

        private static readonly float[] RATIOS =  {
            16.0f / 9.0f,
            3.0f / 2.0f,
            4.0f / 3.0f,
            1.0f / 1.0f,
            2.0f/ 3.0f,
            9.0f / 16.0f
        };

        private static readonly String[] RATIO_NAMES = {
            "16:9",
            "3:2",
            "4:3",
            "1:1",
            "2:3",
            "9:16"
        };

        private void ComputeCategories()
        {
            categories.Clear();
            sorted_categories.Clear();
            sorted_indices.Clear();
            ratios.Clear();

            float step_y = (float)originalImageSource.PixelWidth / (float)BINS;
            float step_x = (float)originalImageSource.PixelHeight / (float)BINS;

            var cost = new List<float>(RATIOS);

            float min_cost, width, height, top, left, bottom, right, this_ratio;
            int x1, x2, y1, y2, i;

            for (int idx = 0; idx < 90; ++idx)
            {
                x1 = CROP_MAPPING[idx, 0];
                y1 = CROP_MAPPING[idx, 1];
                x2 = CROP_MAPPING[idx, 2];
                y2 = CROP_MAPPING[idx, 3];

                top = step_x * (0.5f + (float)x1) + 0.5f;
                left = step_y * (0.5f + (float)y1) + 0.5f;
                bottom = step_x * (0.5f + (float)x2) + 0.5f;
                right = step_y * (0.5f + (float)y2) + 0.5f;

                width = right - left + 1.0f;
                height = bottom - top + 1.0f;

                this_ratio = width / height;
                ratios.Add(this_ratio);

                for (i = 0; i < RATIOS.Length; ++i)
                {
                    cost[i] = Math.Abs(this_ratio - RATIOS[i]);
                }

                min_cost = cost.Min();
                categories.Add(cost.IndexOf(min_cost));
            }

            var groups = categories
                .Select((x, si) => new KeyValuePair<int, int>(x, si))
                .GroupBy(x => x.Key)
                .OrderBy(x => x.Key);

            foreach (var g in groups)
            {
                var group_cost = g.Select(x => new KeyValuePair<float, int>(ratios[x.Value], x.Value))
                    .OrderByDescending(x => x.Key)
                    .ToList();

                for (int it = 0; it < g.Count(); ++it)
                {
                    sorted_categories.Add(g.Key);
                }
                sorted_indices.AddRange(group_cost.Select(x => x.Value));
            }
        }
	}
}