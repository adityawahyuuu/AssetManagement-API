namespace API.Constants
{
    public static class AssetConstants
    {
        public static class Categories
        {
            public const string TempatTidur = "tempat_tidur";
            public const string Meja = "meja";
            public const string Lemari = "lemari";
            public const string Kursi = "kursi";
            public const string Lainnya = "lainnya";

            public static readonly string[] AllowedValues =
            {
                TempatTidur,
                Meja,
                Lemari,
                Kursi,
                Lainnya
            };
        }

        public static class FunctionZones
        {
            public const string Sleeping = "sleeping";
            public const string Study = "study";
            public const string Storage = "storage";
            public const string Leisure = "leisure";

            public static readonly string[] AllowedValues =
            {
                Sleeping,
                Study,
                Storage,
                Leisure
            };
        }

        public static class Conditions
        {
            public const string New = "new";
            public const string Good = "good";
            public const string Fair = "fair";
            public const string NeedsRepair = "needs_repair";

            public static readonly string[] AllowedValues =
            {
                New,
                Good,
                Fair,
                NeedsRepair
            };
        }
    }
}
