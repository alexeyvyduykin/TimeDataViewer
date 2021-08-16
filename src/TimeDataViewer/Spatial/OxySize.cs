#nullable enable

namespace TimeDataViewer.Spatial
{
    public struct OxySize
    {
        /// <summary>
        /// Empty Size.
        /// </summary>
        public static readonly OxySize Empty = new OxySize(0, 0);

        /// <summary>
        /// The height
        /// </summary>
        private readonly double height;

        /// <summary>
        /// The width
        /// </summary>
        private readonly double width;

        /// <summary>
        /// Initializes a new instance of the <see cref="OxySize" /> struct.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public OxySize(double width, double height)
        {
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Gets the height.
        /// </summary>
        /// <value>The height.</value>
        public double Height
        {
            get
            {
                return this.height;
            }
        }

        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <value>The width.</value>
        public double Width
        {
            get
            {
                return this.width;
            }
        }
    }
}
